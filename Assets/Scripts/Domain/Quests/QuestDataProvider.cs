using Session;
using Constructor;
using Constructor.Ships;
using GameDatabase;
using System.Linq;
using System.Collections.Generic;
using Services.InternetTime;
using GameDatabase.DataModel;
using GameDatabase.Extensions;
using GameDatabase.Query;
using GameServices.Player;
using ResearchService = GameServices.Research.Research;

namespace Domain.Quests
{
    public class QuestDataProvider : IQuestDataStorage
    {
        private const int GrantAllContentQuestId = 201;
        private const int AbundantAmount = 100000000;

        private readonly ISessionData _session;
        private readonly GameTime _gameTime;
        private readonly IDatabase _database;
        private readonly PlayerFleet _playerFleet;
        private readonly PlayerInventory _playerInventory;
        private readonly PlayerResources _playerResources;
        private readonly PlayerSkills _playerSkills;
        private readonly ResearchService _research;
        private readonly Utilites.PcgRandom _random = new();

        public QuestDataProvider(
            ISessionData session,
            GameTime gameTime,
            IDatabase database,
            PlayerFleet playerFleet,
            PlayerInventory playerInventory,
            PlayerResources playerResources,
            PlayerSkills playerSkills,
            ResearchService research)
        {
            _session = session;
            _gameTime = gameTime;
            _database = database;
            _playerFleet = playerFleet;
            _playerInventory = playerInventory;
            _playerResources = playerResources;
            _playerSkills = playerSkills;
            _research = research;
        }

        public bool HasBeenCompleted(int id) => _session.Quests.HasBeenCompleted(id);
        public bool IsActive(int id) => _session.Quests.IsQuestActive(id);
        public bool IsActive(int id, int starId) => _session.Quests.IsQuestActive(id, starId);
        public bool IsActiveOrCompleted(int id) => _session.Quests.IsActiveOrCompleted(id);
        public long LastCompletionTime(int id) => _session.Quests.LastCompletionTime(id);
        public long LastStartTime(int id) => _session.Quests.LastStartTime(id);
        public long QuestStartTime(int id, int starId) => _session.Quests.QuestStartTime(id, starId);
        public int TotalQuestCount() => _session.Quests.TotalQuestCount();
        public IEnumerable<QuestProgress> GetActiveQuests() =>
            _session.Quests.GetActiveQuests().Select(item => new QuestProgress(item.QuestId, item.StarId, item.ActiveNode, item.Seed));

        public void SetQuestProgress(QuestProgress data) =>
            _session.Quests.SetQuestProgress(data.QuestId.Value, data.StarId, data.Seed, data.ActiveNode, _gameTime.TotalPlayTime);
        public void SetQuestCompleted(int questId, int starId)
        {
            if (questId == GrantAllContentQuestId && !_session.Quests.HasBeenCompleted(questId))
                GrantAllContent();

            _session.Quests.SetQuestCompleted(questId, starId, true, _gameTime.TotalPlayTime);
        }
        public void SetQuestFailed(int questId, int starId) => 
            _session.Quests.SetQuestCompleted(questId, starId, false, _gameTime.TotalPlayTime);
        public void SetQuestCancelled(int questId, int starId) => _session.Quests.CancelQuest(questId, starId);
        public int GetFactionRelations(int starId) => _session.Quests.GetFactionRelations(starId);
        public void SetFactionRelations(int starId, int value) => _session.Quests.SetFactionRelations(starId, value);

        public int GenerateSeed(QuestModel quest, int starId)
        {
            if (quest.UseRandomSeed) return _random.Next();

            var id = quest.Id.Value;
            var statistics = _session.Quests.GetQuestStatistics(id);
            var totalStartCount = _session.Quests.GetQuestProgress(id).Count() + statistics.CompletionCount + statistics.FailureCount;
            var seed = _session.Game.Seed + (id + starId + 1)*(totalStartCount + 1);
            return seed;
        }

        private void GrantAllContent()
        {
            foreach (var build in ShipBuildQuery.PlayerShips(_database).All)
                _playerFleet.Ships.Add(new CommonShip(build, _database));

            foreach (var component in _database.ComponentList)
                _playerInventory.Components.Add(new ComponentInfo(component), 99);

            foreach (var satellite in _database.SatelliteList)
                _playerInventory.Satellites.Add(satellite, 99);

            _playerSkills.Experience = GameModel.Skills.Experience.FromLevel(int.MaxValue);

            _playerResources.Money += AbundantAmount;
            _playerResources.Stars += AbundantAmount;
            _playerResources.Tokens += AbundantAmount;
            _playerResources.Snowflakes += AbundantAmount;
            _playerResources.Fuel += AbundantAmount;

            foreach (var resource in _database.QuestItemList)
                _playerResources.AddResource(resource.Id, AbundantAmount);

            foreach (var faction in _database.FactionsWithEmpty.WithTechTree())
                _research.AddResearchPoints(faction, AbundantAmount);
        }
    }
}
