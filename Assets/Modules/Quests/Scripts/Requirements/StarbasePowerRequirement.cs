using Services.Localization;
using UnityEngine.Assertions;

namespace Domain.Quests
{
    public class StarbasePowerRequirement : IRequirements
    {
        public StarbasePowerRequirement(int starId, int minValue, int maxValue, IStarMapDataProvider starMapData)
        {
            if (maxValue == 0 && minValue > 0)
                maxValue = int.MaxValue;

            Assert.IsTrue(minValue <= maxValue);

            _minValue = minValue;
            _maxValue = maxValue;
            _starId = starId;
            _starMapData = starMapData;
        }

        public bool IsMet
        {
            get
            {
                var star = _starId > 0 ? _starMapData.GetStarData(_starId) : _starMapData.CurrentStar;
                var value = star.Region.StarbasePower;
                return value >= _minValue && value <= _maxValue;
            }
        }

        public bool CanStart(int starId, int seed) { return IsMet; }

        public string GetDescription(ILocalization localization)
        {
#if UNITY_EDITOR
            return "FACTION POWER " + _starId + " in [" + _minValue + "," + _maxValue + "]";
#else
            return string.Empty;
#endif
        }

        public int BeaconPosition { get { return -1; } }

        private readonly int _minValue;
        private readonly int _maxValue;
        private readonly int _starId;
        private readonly IStarMapDataProvider _starMapData;
    }
}
