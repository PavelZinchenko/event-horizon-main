using GameDatabase.DataModel;
using Services.Localization;

namespace Domain.Quests
{
    public class FactionRequirements : IRequirements
    {
        public FactionRequirements(Faction faction, IStarMapDataProvider starMapData)
        {
            _faction = faction;
            _starMapData = starMapData;
        }

        public bool IsMet => _starMapData.CurrentStar.Region.Faction == _faction;

        public bool CanStart(int starId, int seed) => _starMapData.GetStarData(starId).Region.Faction == _faction;

        public string GetDescription(ILocalization localization)
        {
#if UNITY_EDITOR
            return "FACTION: " + _faction;
#else
            return string.Empty;
#endif
        }

        public int BeaconPosition { get { return -1; } }

        private readonly Faction _faction;
        private readonly IStarMapDataProvider _starMapData;
    }
}
