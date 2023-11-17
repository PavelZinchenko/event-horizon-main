using Session;
using System;
using System.Collections.Generic;

namespace Domain.Quests
{
    public class StarMapDataProvider : IStarMapDataProvider, IPlayerDataProvider
    {
        private readonly ISessionData _session;
        private readonly Galaxy.StarData _starData;
        private readonly GameModel.RegionMap _regionMap;

        private StarDataProvider _currentStar;
        private StarDataProvider _lastStar;

        public StarMapDataProvider(
            ISessionData session,
            Galaxy.StarData starData,
            GameModel.RegionMap regionMap)
        {
            _session = session;
            _starData = starData;
            _regionMap = regionMap;
        }

        public IStarDataProvider CurrentStar
        {
            get
            {
                var id = _session.StarMap.PlayerPosition;
                if (_currentStar != null && _currentStar.Id == id) 
                    return _currentStar;

                return _currentStar = new StarDataProvider(new Galaxy.Star(id, _starData));
            }
        }

        public IStarDataProvider GetStarData(int id)
        {
            if (_lastStar != null && _lastStar.Id == id) 
                return _lastStar;

            return _lastStar = new StarDataProvider(new Galaxy.Star(id, _starData));
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
