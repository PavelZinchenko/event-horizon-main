using GameDatabase.DataModel;

namespace Domain.Quests
{
    public class QuestInfo
    {
        public QuestModel QuestModel { get; }
        public int QuestId => QuestModel != null ? QuestModel.Id.Value : 0;
        public Faction Faction { get; }
        public int StarId { get; }
        public int Level { get; }
        public int Seed { get; }

        public QuestInfo(QuestModel questModel, IStarDataProvider star, int seed)
        {
            QuestModel = questModel;
            StarId = star.Id;
            Level = questModel.Level > 0 ? questModel.Level : star.Level;
            Faction = star.Region.Faction;
            Seed = seed;
        }

        public QuestInfo(int seed)
        {
            StarId = 0;
            Seed = seed;
            Level = 0;
            Faction = Faction.Empty;
        }
    }
}
