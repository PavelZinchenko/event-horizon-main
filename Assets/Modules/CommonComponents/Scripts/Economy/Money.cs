using System;
using CommonComponents.Utils;

namespace CommonComponents
{
	public readonly struct Money
	{
		private static readonly long _mask = DateTime.UtcNow.Ticks;
		private readonly long _amount;

		private long Amount => _amount ^ _mask;

		public static Money operator +(Money first, Money second) => new(SafeAdd(first.Amount, second.Amount));
		public static Money operator -(Money first, Money second) => new(first.Amount - second.Amount);
		public static Money operator /(Money first, Money second) => new(SafeDiv(first.Amount, second.Amount));
		public static Money operator *(Money first, Money second) => new(SafeMul(first.Amount, second.Amount));

		public static Money operator *(Money first, double second) => new(Clamp(first.Amount * second));
		public static Money operator *(Money first, long second) => new(SafeMul(first.Amount, second));

		public static bool operator >(Money first, Money second) => first.Amount > second.Amount;
		public static bool operator <(Money first, Money second) => first.Amount < second.Amount;
		public static bool operator >=(Money first, Money second) => first.Amount >= second.Amount;
		public static bool operator <=(Money first, Money second) => first.Amount <= second.Amount;
		public static bool operator ==(Money first, Money second) => first.Amount == second.Amount;
		public static bool operator !=(Money first, Money second) => first.Amount != second.Amount;

		public static implicit operator Money(int value) => new(value);
		public static implicit operator Money(long value) => new(value);
		public static implicit operator Money(double value) => new(Clamp(value));

		public static explicit operator long(Money value) => value.Amount;
		public static explicit operator int(Money value) => ToInt(value.Amount);

		public string ToString(int maxDigits) => Amount.AsMoney(maxDigits);
		public override string ToString() => Amount.AsMoney();
		public override bool Equals(object obj) => obj is Money other && this == other;
		public override int GetHashCode() => (int)_amount;

		private Money(long amount)
		{
			_amount = (amount > 0 ? amount : 0) ^ _mask;
		}

		private static long SafeAdd(long a, long b)
		{
			if (a < 0 && b < 0) return 0;

			if (a > 0 && b > 0)
			{
				var result = unchecked(a + b);
				return result < 0 ? long.MaxValue : result;
			}

			return a + b;
		}

		private static long SafeMul(long a, long b)
		{
			if (a <= 0 || b <= 0) return 0;

			try
			{
				return checked(a * b);
			}
			catch (Exception)
			{
				return long.MaxValue;
			}
		}

		private static long SafeDiv(long a, long b)
		{
			var result = a / b;
			return a % b == 0 ? result : result + 1;
		}

		private static int ToInt(long value)
		{
			if (value < 0) return 0;
			if (value > int.MaxValue) return int.MaxValue;
			return (int)value;
		}

		private static long Clamp(double value)
		{
			return (long)Math.Clamp(Math.Ceiling(value), 0, long.MaxValue);
		}
	}
}
