using Services.Localization;

namespace Domain.Quests
{
    public class StarRequirements : IRequirements
    {
        public StarRequirements(int starId, bool allowUnsafe, IPlayerDataProvider playerData)
        {
            _playerData = playerData;
            _starId = starId;
			_allowUnsafe = allowUnsafe;
        }

        public bool IsMet => _playerData.CurrentStar.Id == _starId && (_allowUnsafe || _playerData.CurrentStar.IsSecured);

        public bool CanStart(int starId, int seed) => IsMet;

        public string GetDescription(ILocalization localization)
        {
            return localization.GetString("$Condition_GoToStar", Model.Generators.NameGenerator.GetStarName(_starId));
        }

        public int BeaconPosition => _starId;

		private readonly bool _allowUnsafe;
		private readonly int _starId;
        private readonly IPlayerDataProvider _playerData;
    }
}
