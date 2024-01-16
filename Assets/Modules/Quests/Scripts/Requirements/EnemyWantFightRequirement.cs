using Services.Localization;

namespace Domain.Quests
{
    public class OccupantsAttackingRequirements : IRequirements
    {
        public OccupantsAttackingRequirements(IPlayerDataProvider playerData)
        {
            _playerData = playerData;
        }

        public bool IsMet => !_playerData.CurrentStar.IsSecured;
        public bool CanStart(int starId, int seed) => IsMet;

        public string GetDescription(ILocalization localization) => string.Empty;
        public int BeaconPosition => -1;

        private readonly IPlayerDataProvider _playerData;
    }
}
