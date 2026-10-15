using Utilites;

namespace GameDatabase.Model
{
    public readonly struct StatMultiplier
    {
        public StatMultiplier(float valueMinusOne)
        {
            _value = valueMinusOne;
        }

        public static StatMultiplier FromValue(float value)
        {
            return new StatMultiplier(value - 1.0f);
        }

        public float Value { get { return 1.0f + _value; } }
        public float Bonus { get { return _value; } }

        public bool HasValue { get { return !MathEx.Approximately(_value, 0); } }

        public static StatMultiplier operator +(StatMultiplier first, StatMultiplier second) => new(first._value + second._value);
        public static StatMultiplier operator -(StatMultiplier first, StatMultiplier second) => new(first._value - second._value);
        public static StatMultiplier operator *(StatMultiplier first, StatMultiplier second) => new(first.Value*second.Value - 1f);
		public static StatMultiplier operator +(StatMultiplier first, float modifier) => new(first._value + modifier);
		public static StatMultiplier operator *(StatMultiplier first, float multiplier) => new(first.Value*multiplier - 1f);
		public StatMultiplier AmplifyDelta(float multiplier) => new(_value * multiplier);

		public override string ToString()
        {
            return (_value >= 0 ? "+" : "") + MathEx.RoundToInt(100*_value) + "%";
        }

        private readonly float _value;
    }

}
