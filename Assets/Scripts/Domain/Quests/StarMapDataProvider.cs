using Session;
using System;
using System.Collections.Generic;
using Galaxy.StarContent;
using GameModel;

namespace Domain.Quests
{
    public class StarMapDataProvider : IStarMapDataProvider, IPlayerDataProvider
    {
        private readonly ISessionData _session;
        private readonly RegionMap _regionMap;
		private readonly Occupants _occupants;

		private StarDataProvider _currentStar;
        private StarDataProvider _lastStar;

        public StarMapDataProvider(
            ISessionData session,
			Occupants occupants,
            RegionMap regionMap)
        {
            _session = session;
			_occupants = occupants;
            _regionMap = regionMap;
        }

        public IStarDataProvider CurrentStar
        {
            get
            {
                var id = _session.StarMap.PlayerPosition;
                if (_currentStar != null && _currentStar.Id == id) 
                    return _currentStar;

                return _currentStar = new StarDataProvider(id, _occupants, _regionMap, _session.Game.Seed);
            }
        }

        public IStarDataProvider GetStarData(int id)
        {
            if (_lastStar != null && _lastStar.Id == id) 
                return _lastStar;

            return _lastStar = new StarDataProvider(id, _occupants, _regionMap, _session.Game.Seed);
        }

        public int RandomStarAtDistance(int centerStarId, int distance, Random random)
        {
            return GameModel.StarLayout.GetAdjacentStars(centerStarId, distance).RandomElement(random);
        }

        public IEnumerable<IRegionDataProvider> GetRegionsNearby(int centerStarId, int minDistance, int maxDistance)
        {
            List<GameModel.Region> regions = new();
            _regionMap.GetAdjacentRegions(centerStarId, minDistance, maxDistance, regions);

            foreach (var item in regions)
                yield return new RegionDataProvider(item);
        }
    }
}
