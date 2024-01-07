using Galaxy.StarContent;

namespace Domain.Quests
{
    public class StarDataProvider : IStarDataProvider
    {
        private readonly int _starId;
		private readonly int _gameSeed;
		private readonly Occupants _occupants;
		private readonly GameModel.RegionMap _regionMap;
		private RegionDataProvider _region;

        public StarDataProvider(int starId, Occupants occupants, GameModel.RegionMap regionMap, int gameSeed)
        {
            _starId = starId;
			_gameSeed = gameSeed;
			_occupants = occupants;
			_regionMap = regionMap;
        }

        public int Id => _starId;
        public int Level => GameModel.StarLayout.GetStarLevel(_starId, _gameSeed);
        public bool IsSecured => !_occupants.IsAggressive(_starId);
        public IRegionDataProvider Region => _region ??= new RegionDataProvider(_regionMap.GetStarRegion(_starId));
    }
}
