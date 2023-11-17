using Services.Localization;

namespace Domain.Quests
{
    public class EnemiesWantFightRequirements : IRequirements
    {
        public EnemiesWantFightRequirements(IPlayerDataProvider playerData)
        {
            _playerData = playerData;
        }

        public bool IsMet => !_playerData.CurrentStar.IsSecured;
        public bool CanStart(int starId, int seed) { return IsMet; }

        public string GetDescription(ILocalization localization) { return string.Empty; }
        public int BeaconPosition { get { return -1; } }

        private readonly IPlayerDataProvider _playerData;
    }
}
