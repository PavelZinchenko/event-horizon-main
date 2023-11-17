using GameDatabase.DataModel;

namespace Domain.Quests
{
    public class RegionDataProvider : IRegionDataProvider
    {
        private readonly GameModel.Region _region;

        public RegionDataProvider(GameModel.Region region)
        {
            _region = region;
        }

        public int HomeStarId => _region.HomeStar;
        public Faction Faction => _region.Faction;
        public int Relations => _region.PlayerReputation;
        public bool IsHome => _region.Id == GameModel.Region.PlayerHomeRegionId;
        public bool IsCaptured => _region != GameModel.Region.Empty && _region.IsCaptured;
        public bool IsVisited => _region != GameModel.Region.Empty && _region.IsVisited;
    }
}
