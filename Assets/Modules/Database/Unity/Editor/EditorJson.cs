using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using BigInteger = System.Numerics.BigInteger;

namespace GameDatabase.Editor
{
    // Writes database JSON the way the Database Editor does: 2-space indents, LF line ends, the shortest float text,
    // and no field that holds its default value.
    internal static class EditorJson
    {
        public static string Serialize(JObject json)
        {
            using var text = new StringWriter { NewLine = "\n" };
            using (var writer = new JsonTextWriter(text) { Formatting = Formatting.Indented, Indentation = 2 })
                json.WriteTo(writer);
            return text.ToString();
        }

        // A serializable record: base class fields first (ItemType, Id), then the fields in declaration order. A field
        // equal to its value in a new instance (the schema default) is left out; so are null, empty strings, empty
        // arrays and nested objects without a non-default field.
        public static string Serialize(object item)
        {
            using var text = new StringWriter { NewLine = "\n" };
            using (var writer = new JsonTextWriter(text) { Formatting = Formatting.Indented, Indentation = 2 })
                WriteObject(writer, item, true);
            return text.ToString();
        }

        public static JToken Number(float value) => new JRaw(Shortest(value));

        private static void WriteObject(JsonWriter writer, object item, bool isRecord)
        {
            writer.WriteStartObject();
            var defaults = Activator.CreateInstance(item.GetType());
            foreach (var field in Fields(item.GetType()))
            {
                var value = field.GetValue(item);
                var required = isRecord && (field.Name == "ItemType" || field.Name == "Id");
                if (!required && IsDefault(value, field.GetValue(defaults))) continue;

                writer.WritePropertyName(field.Name);
                WriteValue(writer, value);
            }
            writer.WriteEndObject();
        }

        private static void WriteValue(JsonWriter writer, object value)
        {
            switch (value)
            {
                case float number:
                    writer.WriteRawValue(Shortest(number));
                    break;
                case int number:
                    writer.WriteValue(number);
                    break;
                case bool flag:
                    writer.WriteValue(flag);
                    break;
                case string text:
                    writer.WriteValue(text);
                    break;
                case Enum enumValue:
                    writer.WriteValue(System.Convert.ToInt32(enumValue, CultureInfo.InvariantCulture));
                    break;
                case IList list:
                    writer.WriteStartArray();
                    foreach (var element in list)
                        WriteValue(writer, element);
                    writer.WriteEndArray();
                    break;
                default:
                    WriteObject(writer, value, false);
                    break;
            }
        }

        private static bool IsDefault(object value, object defaultValue)
        {
            switch (value)
            {
                case null:
                    return true;
                case string text:
                    return text.Length == 0 || text == (string)defaultValue;
                case IList list:
                    return list.Count == 0;
                case float _:
                case int _:
                case bool _:
                case Enum _:
                    return value.Equals(defaultValue);
                default:
                    if (value.GetType().IsValueType) return value.Equals(defaultValue);
                    var empty = Activator.CreateInstance(value.GetType());
                    return Fields(value.GetType()).All(field => IsDefault(field.GetValue(value), field.GetValue(empty)));
            }
        }

        private static IEnumerable<FieldInfo> Fields(Type type)
        {
            return type.GetFields(BindingFlags.Public | BindingFlags.Instance)
                .OrderBy(field => Depth(field.DeclaringType))
                .ThenBy(field => field.MetadataToken);
        }

        private static int Depth(Type type)
        {
            var depth = 0;
            for (; type != null; type = type.BaseType) ++depth;
            return depth;
        }

        // The Database Editor (.NET) writes a float as the shortest text that reads back as the same value, with ties
        // between two closest candidates going to the even digit. Unity's Mono formats differently (9 digits for "R",
        // ties away from zero), so this builds the editor's text from the exact binary value.
        private static string Shortest(float value)
        {
            if (value == 0f) return "0.0";

            var bits = BitConverter.SingleToInt32Bits(value);
            var biased = (bits >> 23) & 0xFF;
            var mantissa = new BigInteger(bits & 0x7FFFFF);
            var exponent = -149;
            if (biased != 0)
            {
                mantissa += 1 << 23;
                exponent = biased - 150;
            }

            var estimate = (int)Math.Floor(Math.Log10(Math.Abs((double)value)));
            for (var precision = 1; precision <= 9; ++precision)
            {
                var digits = RoundToDigits(mantissa, exponent, precision, estimate, out var power);
                var text = (value < 0f ? "-" : string.Empty) + Format(digits.ToString(CultureInfo.InvariantCulture), power);
                if (float.Parse(text, CultureInfo.InvariantCulture) == value) return text;
            }

            throw new InvalidOperationException($"no round-trip text for {value:R}");
        }

        // |mantissa × 2^exponent| rounded half to even to 'precision' digits. 'power' is the decimal exponent of the first digit.
        private static BigInteger RoundToDigits(BigInteger mantissa, int exponent, int precision, int power0, out int power)
        {
            power = power0;
            var lower = BigInteger.Pow(10, precision - 1);
            while (true)
            {
                var shift = precision - 1 - power;
                var numerator = exponent >= 0 ? mantissa << exponent : mantissa;
                var denominator = exponent >= 0 ? BigInteger.One : BigInteger.One << -exponent;
                if (shift >= 0) numerator *= BigInteger.Pow(10, shift);
                else denominator *= BigInteger.Pow(10, -shift);

                var digits = BigInteger.DivRem(numerator, denominator, out var remainder);
                var twice = remainder * 2;
                if (twice > denominator || (twice == denominator && !digits.IsEven)) ++digits;

                if (digits < lower) { --power; continue; }
                if (digits == lower * 10) { ++power; return lower; }
                if (digits > lower * 10) { ++power; continue; }
                return digits;
            }
        }

        // .NET layout: an exponent below 1e-4 ("2.6382452E-10"), plain digits otherwise, always with a decimal point.
        private static string Format(string digits, int power)
        {
            digits = digits.TrimEnd('0');
            if (power < -4)
                return digits.Substring(0, 1) + (digits.Length > 1 ? "." + digits.Substring(1) : string.Empty) +
                    "E-" + (-power).ToString("00", CultureInfo.InvariantCulture);
            if (power < 0)
                return "0." + new string('0', -power - 1) + digits;

            var whole = digits.Length > power + 1 ? digits.Substring(0, power + 1) : digits.PadRight(power + 1, '0');
            var fraction = digits.Length > power + 1 ? digits.Substring(power + 1) : "0";
            return whole + "." + fraction;
        }
    }
}
