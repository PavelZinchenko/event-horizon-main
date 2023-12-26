using UnityEngine;

namespace CommonComponents.Utils
{
	public readonly struct FlexInt
	{
		public FlexInt(float value)
		{
			_value = value;
		}

		public static FlexInt operator +(FlexInt first, FlexInt second) => new(first._value + second._value);
		public static FlexInt operator -(FlexInt first, FlexInt second) => new(first._value - second._value);
		public static FlexInt operator *(FlexInt first, FlexInt second) => new(first._value * second._value);
		public static FlexInt operator /(FlexInt first, FlexInt second) => new(first._value / second._value);

		public static bool operator >(FlexInt first, FlexInt second) => Mathf.Round(first._value) > Mathf.Round(second._value);
		public static bool operator <(FlexInt first, FlexInt second) => Mathf.Round(first._value) < Mathf.Round(second._value);
		public static bool operator >=(FlexInt first, FlexInt second) => Mathf.Round(first._value) >= Mathf.Round(second._value);
		public static bool operator <=(FlexInt first, FlexInt second) => Mathf.Round(first._value) <= Mathf.Round(second._value);

		public static implicit operator FlexInt(int value) => new(value);
		public static implicit operator FlexInt(long value) => new(value);
		public static implicit operator FlexInt(float value) => new(value);

		public static explicit operator long(FlexInt value) => (long)Mathf.Clamp(value._value, long.MinValue, long.MaxValue);
		public static explicit operator int(FlexInt value) => (int)Mathf.Clamp(value._value, int.MinValue, int.MaxValue);

		public override string ToString()
		{
			return DoubleFormatter.ToInGameString(_value, BigFormat.Expanded);
		}

		private readonly float _value;
	}
}
