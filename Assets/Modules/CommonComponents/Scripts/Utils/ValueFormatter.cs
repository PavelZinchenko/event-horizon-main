using System;
using System.Globalization;
using UnityEngine;

namespace CommonComponents.Utils
{
	public static class ValueFormatter
	{
		public static string AsPercent(this float value)
		{
			if (value >= 1)
				return "+" + Mathf.RoundToInt(100 * (value - 1)) + "%";
			else
				return "-" + Mathf.RoundToInt(100 * (1 - value)) + "%";
		}

		public static string AsInteger(this float value)
		{
			return DoubleFormatter.ToInGameString(value, BigFormat.Decimal);
		}

		public static string AsSignedInteger(this float value)
		{
			return DoubleFormatter.ToSignedInGameString(value, BigFormat.Decimal);
		}

		public static string AsDecimal(this float value, int digits = -1)
		{
			if (digits < 0 || digits > 99) 
				return value.ToString("N");

			return value.ToString("N" + digits);
		}

		public static string AsSignedDecimal(this float value)
		{
			return value >= 0 ? "+" + AsDecimal(value) : AsDecimal(value);
		}

		public static string AsMoney(this long value, int maxDigits = 9)
		{
			var digits = DigitsInLong(value);

			if (digits <= maxDigits)
				return value.ToString("N0", CultureInfo.InvariantCulture);

			var sections = (digits - maxDigits + 2) / 3;
			for (int i = 0; i < sections; i++)
				value /= 1000;

			return value.ToString("N0", CultureInfo.InvariantCulture) + " " + DoubleFormatter.GetDigitsSuffix(sections*3);
		}

		private static int DigitsInLong(this long n) => n == 0L ? 1 : (n > 0L ? 1 : 2) + (int)Math.Log10(Math.Abs((double)n));
	}

	public static class DoubleFormatter
	{
		public static string ToInGameString(this double val, BigFormat format)
		{
			switch (format)
			{
				case BigFormat.Truncated:
					return ToTruncatedInGameString(val);
				case BigFormat.Expanded:
					return ToExpandedInGameString(val);
				case BigFormat.Decimal:
					return ToDecimalInGameString(val);
				default:
					throw new ArgumentOutOfRangeException(nameof(format), format, null);
			}
		}

		public static string ToSignedInGameString(this double val, BigFormat format)
		{
			return (val > 0 ? "+" : "") + val.ToInGameString(format);
		}

		public static string ToTruncatedInGameString(this double val)
		{
			// We don't format very small values for now
			if (Math.Abs(val) < 1) return Math.Truncate(val).ToString(CultureInfo.InvariantCulture);
			// Get amount of digits in steps of 3
			var digs = (DigitsInDouble(val) - 1) / 3 * 3;
			val /= Math.Pow(10, digs);
			return Math.Floor(val).ToString(CultureInfo.InvariantCulture) + GetDigitsSuffix(digs);
		}

		public static string ToExpandedInGameString(this double val)
		{
			if (Math.Abs(val) < 1e5) return Math.Floor(val).ToString(CultureInfo.InvariantCulture);
			var digs = DigitsInDouble(val) / 3 * 3 - 3;
			val /= Math.Pow(10, digs);
			return Math.Floor(val).ToString(CultureInfo.InvariantCulture) + GetDigitsSuffix(digs);
		}

		public static string ToDecimalInGameString(this double val, int significant = 3)
		{
			significant = Math.Max(significant, 1);
			// We don't format small values for now
			if (Math.Abs(val) < 1e5) return Math.Truncate(val).ToString(CultureInfo.InvariantCulture);
			// Get amount of digits in steps of 3
			var digs = (DigitsInDouble(val) - 1) / 3 * 3;

			var significantDigits = DigitsInDouble(val) - significant;
			var result = Math.Floor(val / Math.Pow(10, significantDigits)) * Math.Pow(10, significantDigits - digs);

			return result.ToString(CultureInfo.InvariantCulture) + GetDigitsSuffix(digs);
		}

		internal static string GetDigitsSuffix(int digits)
		{
			// Currently hardcoded endings
			switch (digits)
			{
				case 0:
					return "";
				case 3:
					return "K";
				case 6:
					return "M";
				case 9:
					return "B";
				default:
					return "e" + digits;
			}
		}

		private static int DigitsInDouble(double value)
		{
			if (value == 0 || value == 1) return 1;
			return (int)Math.Floor(Math.Log10(Math.Abs(value)) + 1);
		}
	}

	public enum BigFormat
	{
		/// <summary>
		/// Truncated numerical format, aka 1, 12, 123, 1K, 12K, 123K, 1M
		/// </summary>
		Truncated,

		/// <summary>
		/// Expanded numerical format, aka 1, 12, 123, 1234, 12345, 123K, 1234K, 12345K, 123M
		/// </summary>
		Expanded,

		/// <summary>
		/// Decimal numerical format, aka 1, 12, 123, 1234, 123K, 1.23M, 12.3M, 123M 
		/// </summary>
		Decimal,
	}
}
