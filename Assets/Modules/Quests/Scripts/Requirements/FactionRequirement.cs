using GameDatabase.DataModel;
using Services.Localization;

namespace Domain.Quests
{
    public class FactionRequirements : IRequirements
    {
        public FactionRequirements(Faction faction, IPlayerDataProvider playerData)
        {
            _faction = faction;
            _playerData = playerData;
        }

        public bool IsMet { get { return _playerData.CurrentStar.Region.Faction == _faction; } }

        public bool CanStart(int starId, int seed) { return IsMet; }

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
        private readonly IPlayerDataProvider _playerData;
    }
}
