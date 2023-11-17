using Services.Localization;

namespace Domain.Quests
{
    public class StarbaseCaprturedRequirement : IRequirements
    {
        public StarbaseCaprturedRequirement(int starId, IStarMapDataProvider starMapData)
        {
            _starId = starId;
            _starMapData = starMapData;
        }

        public bool IsMet
        {
            get
            {
                var star = _starId > 0 ? _starMapData.GetStarData(_starId) : _starMapData.CurrentStar;
                return star.Region.IsCaptured;
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
        private readonly IStarMapDataProvider _starMapData;
    }
}
