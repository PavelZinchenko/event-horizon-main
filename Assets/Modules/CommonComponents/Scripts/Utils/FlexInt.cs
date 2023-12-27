using System;

namespace CommonComponents.Utils
{
	public readonly struct FlexInt
	{
		private readonly double _value;

		public static FlexInt operator +(FlexInt first, FlexInt second) => new(first._value + second._value);
		public static FlexInt operator -(FlexInt first, FlexInt second) => new(first._value - second._value);
		public static FlexInt operator *(FlexInt first, FlexInt second) => new(first._value * second._value);
		public static FlexInt operator /(FlexInt first, FlexInt second) => new(first._value / second._value);

		public static bool operator >(FlexInt first, FlexInt second) => first._value > second._value;
		public static bool operator <(FlexInt first, FlexInt second) => first._value < second._value;
		public static bool operator >=(FlexInt first, FlexInt second) => first._value >= second._value;
		public static bool operator <=(FlexInt first, FlexInt second) => first._value <= second._value;

		public static implicit operator FlexInt(int value) => new(value);
		public static implicit operator FlexInt(long value) => new(value);
		public static implicit operator FlexInt(double value) => new(Math.Round(value));

		public static explicit operator long(FlexInt value) => (long)Math.Clamp(value._value, long.MinValue, long.MaxValue);
		public static explicit operator int(FlexInt value) => (int)Math.Clamp(value._value, int.MinValue, int.MaxValue);

		public override string ToString()
		{
			return DoubleFormatter.ToInGameString(_value, BigFormat.Expanded);
		}

		private FlexInt(double value) => _value = value;
	}
}
