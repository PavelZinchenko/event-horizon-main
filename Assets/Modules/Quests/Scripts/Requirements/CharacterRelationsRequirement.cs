using GameDatabase.DataModel;
using Services.Localization;
using UnityEngine.Assertions;

namespace Domain.Quests
{
    public class CharacterRelationsRequirement : IRequirements
    {
        public CharacterRelationsRequirement(Character character, int minValue, int maxValue, ICharacterDataProvider relationshipData)
        {
            if (minValue > maxValue)
            {
                if (maxValue == 0) maxValue = int.MaxValue;
                else if (minValue == 0) minValue = int.MinValue;
            }

            Assert.IsTrue(minValue <= maxValue);

            _minValue = minValue;
            _maxValue = maxValue;
            _character = character;
            _relationshipData = relationshipData;
        }

        public bool IsMet
        {
            get
            {
                var value = _relationshipData.GetCharacterAttitude(_character.Id);
                return value >= _minValue && value <= _maxValue;
            }
        }

        public bool CanStart(int starId, int seed) { return IsMet; }

        public string GetDescription(ILocalization localization)
        {
#if UNITY_EDITOR
            return "CHARACTER RELATIONS " + _character.Id + " " + _relationshipData.GetCharacterAttitude(_character.Id) + " -> [" + _minValue + "," + _maxValue + "]";
#else
            return string.Empty;
#endif
        }

        public int BeaconPosition { get { return -1; } }

        private readonly int _minValue;
        private readonly int _maxValue;
        private readonly Character _character;
        private readonly ICharacterDataProvider _relationshipData;
    }
}
