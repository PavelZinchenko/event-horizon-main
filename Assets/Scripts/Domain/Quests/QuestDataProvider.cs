using Session;

namespace Domain.Quests
{
    public class QuestDataProvider : IQuestDataProvider
    {
        private readonly ISessionData _session;

        public QuestDataProvider(ISessionData session)
        {
            _session = session;
        }

        public bool HasBeenCompleted(int id) => _session.Quests.HasBeenCompleted(id);
        public bool IsActive(int id) => _session.Quests.IsQuestActive(id);
        public bool IsActive(int id, int starId) => _session.Quests.IsQuestActive(id, starId);
        public bool IsActiveOrCompleted(int id) => _session.Quests.IsActiveOrCompleted(id);
        public long LastCompletionTime(int id) => _session.Quests.LastCompletionTime(id);
        public long LastStartTime(int id) => _session.Quests.LastStartTime(id);
        public long QuestStartTime(int id, int starId) => _session.Quests.QuestStartTime(id, starId);
        public int GameSeed => _session.Game.Seed;
    }
}
