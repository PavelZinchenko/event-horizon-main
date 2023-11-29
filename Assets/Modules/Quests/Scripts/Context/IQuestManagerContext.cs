using System;

namespace Domain.Quests
{
    public interface IQuestManagerContext
    {
        IQuestManagerEventProvider EventProvider { get; }
        IQuestDataStorage QuestDataStorage { get; }
        IStarMapDataProvider StarMapDataProvider { get; }
        IGameDataProvider GameDataProvider { get; }
    }

    public interface IQuestManagerEventProvider
    {
        event Action SessionCreated;
        event Action SessionLoaded;
        event Action<IQuestEventData> QuestEventOccured;

        void FireBeaconUpdatedEvent(int starId);
        void FireQuestsUpdatedEvent();
        void FireActionRequiredEvent();
    }
}
