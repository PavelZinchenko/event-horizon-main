using System;
using System.Collections.Generic;
using System.Linq;
using GameDatabase;
using GameDatabase.DataModel;
using GameDatabase.Enums;
using Zenject;

namespace Domain.Quests
{
	public class QuestManager : IQuestManager, IInitializable, IDisposable, ITickable
	{
	    public QuestManager(
            IDatabase database,
			QuestFactory questFactory,
			RequirementsFactory requirementsFactory,
			IQuestManagerContext questManagerContext)
	    {
			_database = database;
			_factory = questFactory;
			_requirementsFactory = requirementsFactory;
			_context = questManagerContext;
		}

		public bool ActionRequired { get; private set; }

	    public void InvokeAction(IQuestActionProcessor processor)
	    {
	        if (_activeQuest != null && _activeQuest.TryInvokeAction(processor))
                _recentlyUpdatedQuests.Add(_activeQuest);
	    }

	    public IEnumerable<IQuest> Quests => _quests.Cast<IQuest>(); 

        public bool IsQuestObjective(int starId)
        {
            return _questBeacons.Count > 0 && _questBeacons.Contains(starId);
        }

		public void Initialize()
        {
			_context.EventProvider.QuestEventOccured += OnQuestEvent;
			_context.EventProvider.SessionLoaded += Reset;
			_context.EventProvider.SessionCreated += LoadQuests;
		}

		public void Dispose()
        {
			_context.EventProvider.QuestEventOccured -= OnQuestEvent;
			_context.EventProvider.SessionLoaded -= Reset;
			_context.EventProvider.SessionCreated -= LoadQuests;
		}

		public void Tick()
	    {
			if (!_context.GameDataProvider.IsGameStarted) return;

            var counter = 100;
            while (_recentlyUpdatedQuests.Count > 0)
            {
                if (--counter == 0)
                {
                    var currentQuest = _recentlyUpdatedQuests.Last();
                    UnityEngine.Debug.LogException(new InvalidOperationException(
                        "QuestManager - infinite loop. Quest:" + currentQuest.Id + " Node:" + currentQuest.NodeId));
                    UnityEngine.Debug.Break();
                    break;
                }

                var index = _recentlyUpdatedQuests.Count - 1;
                var quest = _recentlyUpdatedQuests[index];
                _recentlyUpdatedQuests.RemoveAt(index);
                OnQuestUpdated(quest);
            }

            if (_context.GameDataProvider.TotalPlayTime - _lastUpdateTime > UpdateCooldown)
            {
                _lastUpdateTime = _context.GameDataProvider.TotalPlayTime;
				OnQuestEvent(new SimpleEventData(QuestEventType.Timer));
            }
        }

        public void AbandonQuest(IQuest quest)
	    {
	        var result = _quests.Find(item => item == quest);
	        if (result == null) return;

    	    CompleteQuest(result);
	        UpdateQuestBeacons();
        }

		private void Reset()
	    {
			_quests.Clear();
	        _questBeacons.Clear();
            _questBeaconsOld.Clear();
            _recentlyUpdatedQuests.Clear();
	        _activeQuest = null;
            _lastUpdateTime = 0;

            _beaconQuests.Clear();
            _localEncounterQuests.Clear();
            _arrivedAtStarQuests.Clear();
            _newStarExploredQuests.Clear();
            _factionQuests.Clear();
            _dailyQuests.Clear();
        }

        private void LoadQuests()
	    {
			foreach (var item in _context.QuestDataStorage.GetActiveQuests())
				Add(_factory.Create(item));

	        var starId = _context.StarMapDataProvider.CurrentStar.Id;

            foreach (var questData in _database.QuestList)
                if (questData.StartCondition == StartCondition.GameStart && questData.CanBeStarted(_context.QuestDataStorage, starId))
	                Add(_factory.Create(questData, starId));

            var seed = _context.GameDataProvider.GameSeed;
            _beaconQuests.Assign(_database.QuestList.Where(item => item.StartCondition == StartCondition.Beacon), _requirementsFactory, seed);
	        _localEncounterQuests.Assign(_database.QuestList.Where(item => item.StartCondition == StartCondition.LocalEncounter), _requirementsFactory, seed);
	        _arrivedAtStarQuests.Assign(_database.QuestList.Where(item => item.StartCondition == StartCondition.ArrivedAtStar), _requirementsFactory, seed);
	        _newStarExploredQuests.Assign(_database.QuestList.Where(item => item.StartCondition == StartCondition.NewStarExplored), _requirementsFactory, seed);
            _factionQuests.Assign(_database.QuestList.Where(item => item.StartCondition == StartCondition.FactionMission), _requirementsFactory, seed);
			_dailyQuests.Assign(_database.QuestList.Where(item => item.StartCondition == StartCondition.Daily), _requirementsFactory, seed);
        }

	    public void StartQuest(QuestModel questModel, int seedIncrement = 0)
	    {
            if (questModel == null) return;

	        var starId = _context.StarMapDataProvider.CurrentStar.Id;
            var seed = _context.QuestDataStorage.GenerateSeed(questModel, starId) + seedIncrement;

	        if (questModel.StartCondition != StartCondition.Manual)
	        {
	            UnityEngine.Debug.LogException(new ArgumentException("QuestManager.StartQuest: Wrong start condition - " + questModel.StartCondition));
	            return;
	        }

            if (!questModel.CanBeStarted(_context.QuestDataStorage, starId))
	        {
	            UnityEngine.Debug.LogError(new ArgumentException("QuestManager.StartQuest: Quest can't be started - " + questModel.Id.Value));
                return;
	        }

	        if (!_requirementsFactory.CreateForQuest(questModel.Requirement, seed).CanStart(starId, seed))
	        {
	            UnityEngine.Debug.LogError(new ArgumentException("QuestManager.StartQuest: Requirements are not met - " + questModel.Id.Value));
	            return;
	        }

            Add(_factory.Create(questModel, starId, seedIncrement));
	    }

        private void Add(Quest quest)
	    {
			if (quest == null)
	        {
	            //UnityEngine.Debug.LogException(new ArgumentException("QuestManager: quest is null"));
                return;
	        }

	        UnityEngine.Debug.Log("new quest: " + quest.Model.Name);

	        _quests.Add(quest);

			_recentlyUpdatedQuests.Add(quest);
			_context.EventProvider.FireQuestsUpdatedEvent();
	    }

        private void OnQuestUpdated(Quest quest)
        {
			if (quest.Model.QuestType != QuestType.Temporary)
				_context.QuestDataStorage.SetQuestProgress(new QuestProgress(quest.Id, quest.StarId, quest.NodeId, quest.Seed));

            if (quest.Status.IsFinished())
            {
                CompleteQuest(quest);
                FindActiveQuest();
            }
            else if (quest.Status == QuestStatus.ActionRequired)
		    {
		        if (_activeQuest == null || _activeQuest == quest || _activeQuest.Status != QuestStatus.ActionRequired || _quests.IndexOf(quest) > _quests.IndexOf(_activeQuest))
		        {
		            _activeQuest = quest;
		            ActionRequired = true;
					_context.EventProvider.FireActionRequiredEvent();
				}
            }
            else if (quest == _activeQuest || _activeQuest == null)
            {
                FindActiveQuest();
            }

            UpdateQuestBeacons();
        }

	    private void UpdateQuestBeacons()
	    {
	        var temp = _questBeaconsOld;
	        _questBeaconsOld = _questBeacons;
	        _questBeacons = temp;

	        _questBeacons.Clear();
	        foreach (var quest in _quests)
	            quest.TryGetBeacons(_questBeacons);

            _questBeaconsOld.SymmetricExceptWith(_questBeacons);
	        if (_questBeaconsOld.Count <= 0) return;

	        foreach (var id in _questBeaconsOld)
				_context.EventProvider.FireBeaconUpdatedEvent(id);
	    }

        private void FindActiveQuest()
	    {
	        _activeQuest = null;
	        ActionRequired = false;

	        for (var i = _quests.Count - 1; i >= 0; --i)
	        {
	            var item = _quests[i];
	            if (item.Status != QuestStatus.ActionRequired)
                    continue;

                _activeQuest = item;
	            ActionRequired = true;
				_context.EventProvider.FireActionRequiredEvent();
				break;
	        }
	    }

        private void CompleteQuest(Quest quest)
	    {
			_quests.Remove(quest);
	        _recentlyUpdatedQuests.Remove(quest);

			if (quest.Model.QuestType == QuestType.Temporary) return;

	        switch (quest.Status)
	        {
                case QuestStatus.Completed:
                    _context.QuestDataStorage.SetQuestCompleted(quest.Id, quest.StarId);
                    break;
	            case QuestStatus.Failed:
                    _context.QuestDataStorage.SetQuestFailed(quest.Id, quest.StarId);
                    break;
                case QuestStatus.Cancelled:
                case QuestStatus.InProgress:
                    _context.QuestDataStorage.SetQuestCancelled(quest.Id, quest.StarId);
	                break;
                case QuestStatus.Error:
                default:
                    UnityEngine.Debug.LogException(new InvalidOperationException("QuestManager: Error has occured - " + quest.Model.Name));
                    _context.QuestDataStorage.SetQuestCancelled(quest.Id, quest.StarId);
                    break;
            }

			_context.EventProvider.FireQuestsUpdatedEvent();
		}

        private void OnQuestEvent(IQuestEventData data)
	    {
            var time = _context.GameDataProvider.TotalPlayTime;

            if (data.Type == QuestEventType.BeaconActivated)
	        {
	            var eventData = (BeaconEventData)data;
                _beaconQuests.UpdateQuests(eventData.StarId, eventData.Seed, time, _context);
	            Add(_beaconQuests.CreateRandomWeighted(_factory, eventData.Seed));
                return;
	        }
	        if (data.Type == QuestEventType.LocalEncounter)
	        {
	            var eventData = (LocalEncounterEventData)data;
                _localEncounterQuests.UpdateQuests(eventData.StarId, eventData.Seed, time, _context);
	            Add(_localEncounterQuests.CreateRandomWeighted(_factory, eventData.Seed));
                return;
	        }
	        if (data.Type == QuestEventType.NewStarSystemSecured)
	        {
	            var eventData = (StarEventData)data;
                var seed = _context.GameDataProvider.GameSeed + eventData.StarId;
                _newStarExploredQuests.UpdateQuests(eventData.StarId, seed, time, _context);
	            Add(_newStarExploredQuests.CreateRandomWeighted(_factory, seed));
                return;
	        }
            if (data.Type == QuestEventType.FactionMissionAccepted)
	        {
	            var eventData = (StarEventData)data;
	            var seed = _context.GameDataProvider.GameSeed + eventData.StarId + _context.QuestDataStorage.TotalQuestCount() +
                    _context.StarMapDataProvider.GetStarData(eventData.StarId).Region.Relations;
                _factionQuests.UpdateQuests(eventData.StarId, seed, time, _context);
                Add(_factionQuests.CreateRandomWeighted(_factory, seed));
                return;
	        }

	        if (data.Type == QuestEventType.ArrivedAtStarSystem)
	        {
	            var eventData = (StarEventData)data;
	            var seed = _context.GameDataProvider.GameSeed + eventData.StarId + _context.QuestDataStorage.TotalQuestCount();
                _arrivedAtStarQuests.UpdateQuests(eventData.StarId, seed, time, _context);
	            Add(_arrivedAtStarQuests.CreateRandomWeighted(_factory, seed));
	        }

            if (data.Type == QuestEventType.Timer)
            {
                var seed = _context.GameDataProvider.GameSeed + (int)(time / TimeSpan.TicksPerMinute) + _context.QuestDataStorage.TotalQuestCount();
                _dailyQuests.UpdateQuests(_context.StarMapDataProvider.CurrentStar.Id, seed, time, _context);
                Add(_dailyQuests.CreateFirstAvailable(_factory));
            }

			ProcessQuestEvent(data);
	    }

	    private void ProcessQuestEvent(IQuestEventData data)
	    {
	        foreach (var quest in _quests)
	            if (quest.TryProcessEvent(data) && !_recentlyUpdatedQuests.Contains(quest))
	                _recentlyUpdatedQuests.Add(quest);
        }

        private Quest _activeQuest;
	    private readonly List<Quest> _recentlyUpdatedQuests = new();
        private readonly List<Quest> _quests = new();
	    private readonly QuestCollection _beaconQuests = new();
	    private readonly QuestCollection _localEncounterQuests = new();
        private readonly QuestCollection _newStarExploredQuests = new();
	    private readonly QuestCollection _arrivedAtStarQuests = new();
        private readonly QuestCollection _factionQuests = new();
        private readonly QuestCollection _dailyQuests = new();

		private HashSet<int> _questBeacons = new();
	    private HashSet<int> _questBeaconsOld = new();

        private long _lastUpdateTime;
		private readonly QuestFactory _factory;
		private readonly RequirementsFactory _requirementsFactory;
		private readonly IDatabase _database;
		private readonly IQuestManagerContext _context;

		private const long UpdateCooldown = 10 * TimeSpan.TicksPerSecond;
	}
}
