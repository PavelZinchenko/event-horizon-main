using Services.Localization;
using Session;

namespace Domain.Quests
{
    public class StarbaseCaprturedRequirement : IRequirements
    {
        public StarbaseCaprturedRequirement(int starId, GameModel.RegionMap regionMap, ISessionData session)
        {
            _starId = starId;
            _session = session;
            _regionMap = regionMap;
        }

        public bool IsMet
        {
            get
            {
                var starId = _starId > 0 ? _starId : _session.StarMap.PlayerPosition;
                var region = _regionMap.GetStarRegion(starId);
                return region != GameModel.Region.Empty && region.IsCaptured;
            }
        }

        public bool CanStart(int starId, int seed) { return IsMet; }

        public string GetDescription(ILocalization localization)
        {
#if UNITY_EDITOR
            return "STARBASE CAPTURED " + _starId;
#else
            return string.Empty;
#endif
        }

        public int BeaconPosition { get { return -1; } }

        private readonly int _starId;
        private readonly ISessionData _session;
        private readonly GameModel.RegionMap _regionMap;
    }
}
