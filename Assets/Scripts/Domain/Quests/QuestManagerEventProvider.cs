using System;
using Domain.Quests;
using Galaxy;
using Session;
using CommonComponents.Signals;

namespace GameServices.Quests
{
	public class QuestManagerEventProvider : IQuestManagerEventProvider
	{
	    public QuestManagerEventProvider(
			QuestActionRequiredSignal.Trigger questActionRequiredTrigger,
            QuestListChangedSignal.Trigger questListChangedTrigger,
            QuestEventSignal questEventSignal,
            StarContentChangedSignal.Trigger starContentChangedTrigger,
            SessionDataLoadedSignal dataLoadedSignal, 
            SessionCreatedSignal sessionCreatedSignal)
	    {
            _questListChangedTrigger = questListChangedTrigger;
			_questActionRequiredTrigger = questActionRequiredTrigger;
	        _starContentChangedTrigger = starContentChangedTrigger;

			_dataLoadedSignal = dataLoadedSignal;
			_dataLoadedSignal.Event += OnSessionDataLoaded;
			_sessionCreatedSignal = sessionCreatedSignal;
			_sessionCreatedSignal.Event += OnSessionCreated;
			_questEventSignal = questEventSignal;
			_questEventSignal.Event += OnQuestEvent;
		}

		public event Action SessionCreated;
		public event Action SessionLoaded;
		public event Action<IQuestEventData> QuestEventOccured;

		private void OnSessionDataLoaded()
	    {
			SessionLoaded?.Invoke();
		}

		private void OnSessionCreated()
	    {
			SessionCreated?.Invoke();
        }

		private void OnQuestEvent(IQuestEventData data)
        {
			QuestEventOccured?.Invoke(data);
        }

        public void FireBeaconUpdatedEvent(int starId)
        {
			_starContentChangedTrigger.Fire(starId);
		}

		public void FireQuestsUpdatedEvent()
        {
			_questListChangedTrigger.Fire();
		}

		public void FireActionRequiredEvent()
        {
			_questActionRequiredTrigger.Fire();
        }

		private readonly QuestListChangedSignal.Trigger _questListChangedTrigger;
	    private readonly StarContentChangedSignal.Trigger _starContentChangedTrigger;
        private readonly QuestActionRequiredSignal.Trigger _questActionRequiredTrigger;
	    private readonly QuestEventSignal _questEventSignal;
		private readonly SessionDataLoadedSignal _dataLoadedSignal;
		private readonly SessionCreatedSignal _sessionCreatedSignal;
    }

	public class QuestActionRequiredSignal : SmartWeakSignal<QuestActionRequiredSignal> {}
    public class QuestListChangedSignal : SmartWeakSignal<QuestListChangedSignal> {}
}
