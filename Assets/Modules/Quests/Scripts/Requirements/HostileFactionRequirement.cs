using Services.Localization;

namespace Domain.Quests
{
    public class HostileFactionRequirement : IRequirements
    {
        public HostileFactionRequirement(IStarMapDataProvider starMapData)
        {
            _starMapData = starMapData;
        }

        public bool IsMet => _starMapData.CurrentStar.Region.Faction.NoMissions;

        public bool CanStart(int starId, int seed) => _starMapData.GetStarData(starId).Region.Faction.NoMissions;

        public string GetDescription(ILocalization localization)
        {
#if UNITY_EDITOR
            return "HOSTILE FACTION";
#else
            return string.Empty;
#endif
        }

        public int BeaconPosition => -1;

        private readonly IStarMapDataProvider _starMapData;
    }
}
