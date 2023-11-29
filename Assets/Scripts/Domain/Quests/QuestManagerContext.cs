namespace Domain.Quests
{
    public class QuestManagerContext : IQuestManagerContext
    {
        public QuestManagerContext(
            IQuestManagerEventProvider eventProvider,
            IStarMapDataProvider starMapDataProvider,
            IQuestDataStorage questDataStorage,
            IGameDataProvider timeDataProvider)
        {
            EventProvider = eventProvider;
            StarMapDataProvider = starMapDataProvider;
            QuestDataStorage = questDataStorage;
            GameDataProvider = timeDataProvider;
        }

        public IQuestManagerEventProvider EventProvider { get; }
        public IQuestDataStorage QuestDataStorage { get; }
        public IStarMapDataProvider StarMapDataProvider { get; }
        public IGameDataProvider GameDataProvider { get; }
    }
}
