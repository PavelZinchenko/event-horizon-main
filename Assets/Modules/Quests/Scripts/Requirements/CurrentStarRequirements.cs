using Services.Localization;

namespace Domain.Quests
{
    public class CurrentStarRequirements : IRequirements
    {
        public CurrentStarRequirements(int minDistance, int maxDistance, bool allowUnsafe, IPlayerDataProvider playerData)
        {
            _playerData = playerData;

			_allowUnsafe = allowUnsafe;
			_minDistance = minDistance;
            _maxDistance = maxDistance;
        }

        public bool IsMet
        {
            get
            {
                var star = _playerData.CurrentStar;
                var level = star.Level;
				if (level < _minDistance || level > _maxDistance) return false;
                return _allowUnsafe || star.IsSecured;
            }
        }

        public bool CanStart(int starId, int seed) => IsMet;

        public string GetDescription(ILocalization localization)
        {
#if UNITY_EDITOR
            return "POSITION: " + _minDistance + " - " + _maxDistance;
#else
            return string.Empty;
#endif
        }

        public int BeaconPosition => -1;

		private readonly bool _allowUnsafe;
        private readonly int _minDistance;
        private readonly int _maxDistance;
        private readonly IPlayerDataProvider _playerData;
    }
}
