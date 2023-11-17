using Services.Localization;

namespace Domain.Quests
{
    public class StarRequirements : IRequirements
    {
        public StarRequirements(int starId, IPlayerDataProvider playerData)
        {
            _playerData = playerData;
            _starId = starId;
        }

        public bool IsMet
        {
            get
            {
                return _playerData.CurrentStar.Id == _starId && _playerData.CurrentStar.IsSecured;
            }
        }

        public bool CanStart(int starId, int seed) { return IsMet; }

        public string GetDescription(ILocalization localization)
        {
            return localization.GetString("$Condition_GoToStar", Model.Generators.NameGenerator.GetStarName(_starId));
        }

        public int BeaconPosition { get { return _starId; } }

        private readonly int _starId;
        private readonly IPlayerDataProvider _playerData;
    }
}
