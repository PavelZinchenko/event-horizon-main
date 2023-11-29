using Session;
using System.Linq;
using System.Collections.Generic;
using Services.InternetTime;

namespace Domain.Quests
{
    public class QuestDataProvider : IQuestDataStorage
    {
        private readonly ISessionData _session;
        private readonly GameTime _gameTime;

        public QuestDataProvider(ISessionData session, GameTime gameTime)
        {
            _session = session;
            _gameTime = gameTime;
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
        public void SetQuestCompleted(int questId, int starId) => 
            _session.Quests.SetQuestCompleted(questId, starId, true, _gameTime.TotalPlayTime);
        public void SetQuestFailed(int questId, int starId) => 
            _session.Quests.SetQuestCompleted(questId, starId, false, _gameTime.TotalPlayTime);
        public void SetQuestCancelled(int questId, int starId) => _session.Quests.CancelQuest(questId, starId);
    }
}
