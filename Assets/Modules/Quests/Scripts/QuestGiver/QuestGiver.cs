using System.Collections.Generic;
using GameDatabase.Enums;

namespace Domain.Quests
{
    public class QuestGiver
    {
        public QuestGiver(GameDatabase.DataModel.QuestOrigin data, IStarMapDataProvider starMapDataProvider)
        {
            _data = data;
            _starMapDataProvider = starMapDataProvider;
            _factionFilter = new FactionFilter(data.Factions, 0);
        }

        public int GetStartSystem(int currentStarId, int seed)
        {
            switch (_data.Type)
            {
                case QuestOriginType.CurrentStar:
                    return currentStarId;
                case QuestOriginType.HomeStar:
                    return 0;
                case QuestOriginType.CurrentFactionBase:
                    return _starMapDataProvider.GetStarData(currentStarId).Region.HomeStarId;
                case QuestOriginType.RandomStar:
                    return GetRandomStar(currentStarId, seed);
                case QuestOriginType.RandomFactionBase:
                    return GetRandomFactionBase(currentStarId, seed);
                default:
                    return -1;
            }
        }

        private int GetRandomStar(int center, int seed)
        {
            var random = new System.Random(seed);
            var minDistance = _data.MinDistance;
            var maxDistance = _data.MaxDistance > minDistance ? _data.MaxDistance : minDistance;
            var distance = maxDistance > minDistance ? minDistance + random.Next(maxDistance - minDistance + 1) : minDistance;
            var starId = GameModel.StarLayout.GetAdjacentStars(center, distance).RandomElement(random);
            return starId;
        }

        private int GetRandomFactionBase(int center, int seed)
        {
            if (_adjacentRegions == null)
            {
                var minDistance = _data.MinDistance;
                var maxDistance = _data.MaxDistance > minDistance ? _data.MaxDistance : minDistance;
                _adjacentRegions = new(_starMapDataProvider.GetRegionsNearby(center, minDistance, maxDistance));
            }

            var random = new System.Random(seed);
            var minRelations = _data.MinRelations;
            var maxRelations = _data.MaxRelations > minRelations ? _data.MaxRelations : minRelations;

            var index = random.Next(_adjacentRegions.Count);

            var count = _adjacentRegions.Count;
            for (var i = 0; i < count; ++i)
            {
                var region = _adjacentRegions[index + i < count ? index + i : index + i - count];

                if (region.IsHome) continue;
                if (!_factionFilter.IsSuitableForBase(region.Faction)) continue;
                if (!region.IsVisited) continue;

                if (minRelations != 0 || maxRelations != 0)
                {
                    var relations = region.Relations;
                    if (relations < minRelations || relations > maxRelations) continue;
                }

                return region.HomeStarId;
            }

            return -1;
        }

        private List<IRegionDataProvider> _adjacentRegions;
        private readonly IStarMapDataProvider _starMapDataProvider;
        private readonly FactionFilter _factionFilter;
        private readonly GameDatabase.DataModel.QuestOrigin _data;
    }
}