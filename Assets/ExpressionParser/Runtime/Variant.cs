using System;
using System.Globalization;
using UnityEngine;

namespace CodeWriter.ExpressionParser
{
    public readonly struct Variant
    {
        public enum ValueType
        {
            TypeInt,
            TypeSingle,
            TypeBool,
        }

        public readonly static Variant True = new Variant(true);
        public readonly static Variant False = new Variant(false);

        private readonly ValueType _type;
        private readonly int _intValue;
        private readonly float _singleValue;

        public int AsInt => _type == ValueType.TypeSingle ? (int) _singleValue : _intValue;
        public float AsSingle => _type == ValueType.TypeSingle ? _singleValue : _intValue;
        public bool AsBool => _intValue != 0 || _singleValue != 0;

        public Variant(int value)
        {
            _type = ValueType.TypeInt;
            _intValue = value;
            _singleValue = 0f;
        }

        public Variant(float value)
        {
            _type = ValueType.TypeSingle;
            _singleValue = value;
            _intValue = 0;
        }

        public Variant(bool value)
        {
            _type = ValueType.TypeBool;
            _intValue = value ? 1 : 0;
            _singleValue = 0f;
        }

        public static Variant Parse(string input)
        {
            if (int.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out int intValue))
                return new Variant(intValue);
            else if (float.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out float floatValue))
                return new Variant(floatValue);
            else if (bool.TryParse(input, out bool boolValue))
                return new Variant(boolValue);
            else
                throw new FormatException();
        }

        public static implicit operator Variant(bool value) => new(value);
        public static implicit operator Variant(int value) => new(value);
        public static implicit operator Variant(float value) => new(value);

        public static Variant operator -(Variant value)
        {
            if (value._type == ValueType.TypeInt)
                return new Variant(-value._intValue);
            if (value._type == ValueType.TypeSingle)
                return new Variant(-value._singleValue);
            else
                throw new InvalidOperationException();
        }

        public static Variant operator +(Variant first, Variant second)
        {
            if (first._type == ValueType.TypeInt && second._type == ValueType.TypeInt)
                return new Variant(first._intValue + second._intValue);
            else
                return new Variant(first.AsSingle + second.AsSingle);
        }

        public static Variant operator -(Variant first, Variant second)
        {
            if (first._type == ValueType.TypeInt && second._type == ValueType.TypeInt)
                return new Variant(first._intValue - second._intValue);
            else
                return new Variant(first.AsSingle - second.AsSingle);
        }

        public static Variant operator *(Variant first, Variant second)
        {
            if (first._type == ValueType.TypeInt && second._type == ValueType.TypeInt)
                return new Variant(first._intValue * second._intValue);
            else
                return new Variant(first.AsSingle * second.AsSingle);
        }

        public static Variant operator /(Variant first, Variant second)
        {
            if (first._type == ValueType.TypeInt && second._type == ValueType.TypeInt)
                return new Variant(first._intValue / second._intValue);
            else
                return new Variant(first.AsSingle / second.AsSingle);
        }

        public static Variant operator %(Variant first, Variant second)
        {
            if (first._type == ValueType.TypeInt && second._type == ValueType.TypeInt)
                return new Variant(first._intValue % second._intValue);
            else
                return new Variant(first.AsSingle % second.AsSingle);
        }

        public static bool operator ==(Variant first, Variant second)
        {
            if (first._type == ValueType.TypeSingle || second._type == ValueType.TypeSingle)
                return Mathf.Approximately(first.AsSingle, second.AsSingle);
            else if (first._type == second._type)
                return first._intValue == second._intValue;
            else
                return false;
        }

        public static bool operator !=(Variant first, Variant second)
        {
            return !(first == second);
        }

        public static bool operator <(Variant first, Variant second)
        {
            if (first._type == ValueType.TypeInt && second._type == ValueType.TypeInt)
                return first._intValue < second._intValue;
            else
                return first.AsSingle < second.AsSingle;
        }

        public static bool operator >(Variant first, Variant second)
        {
            if (first._type == ValueType.TypeInt && second._type == ValueType.TypeInt)
                return first._intValue > second._intValue;
            else
                return first.AsSingle > second.AsSingle;
        }

        public static bool operator <=(Variant first, Variant second)
        {
            if (first._type == ValueType.TypeInt && second._type == ValueType.TypeInt)
                return first._intValue <= second._intValue;
            else
                return first.AsSingle <= second.AsSingle;
        }

        public static bool operator >=(Variant first, Variant second)
        {
            if (first._type == ValueType.TypeInt && second._type == ValueType.TypeInt)
                return first._intValue >= second._intValue;
            else
                return first.AsSingle >= second.AsSingle;
        }

        public static Variant Round(Variant value)
        {
            if (value._type == ValueType.TypeInt) return value;
            return Mathf.RoundToInt(value.AsSingle);
        }

        public static Variant Ceil(Variant value)
        {
            if (value._type == ValueType.TypeInt) return value;
            return Mathf.CeilToInt(value.AsSingle);
        }

        public static Variant Floor(Variant value)
        {
            if (value._type == ValueType.TypeInt) return value;
            return Mathf.FloorToInt(value.AsSingle);
        }
        
        public static Variant Pow(Variant a, Variant b)
        {
            var pow = Mathf.Pow(a.AsSingle, b.AsSingle);
            // Return fraction if either base or power is fractional, or the result is outside of float precision
            // (see https://stackoverflow.com/a/3793950)
            if (a._type == ValueType.TypeSingle || b._type == ValueType.TypeSingle || pow < -16777217 || pow > 16777217)
                return pow; 
            return (int) pow;
        }
        
        public static Variant Log10(Variant value)
        {
            // Logarithm results are almost always decimal, so just convert everything to float
            return Mathf.Log10(value.AsSingle);
        }
        
        public static Variant Log(Variant value, Variant newBase)
        {
            // Logarithms are almost always decimal, so just convert everything to float
            return Mathf.Log(value.AsSingle, newBase.AsSingle);
        }
        
        // Trigonometric functions are almost always decimal, so just convert everything to float
        public static Variant Sin(Variant value) => Mathf.Sin(value.AsSingle);
        public static Variant Cos(Variant value) => Mathf.Cos(value.AsSingle);
        public static Variant Tan(Variant value) => Mathf.Tan(value.AsSingle);
        public static Variant Atan(Variant value) => Mathf.Atan(value.AsSingle);
        public static Variant Atan2(Variant x, Variant y) => Mathf.Atan2(x.AsSingle, y.AsSingle);
        public static Variant Abs(Variant value)
        {
            if (value._type == ValueType.TypeInt) return Math.Abs(value._intValue);
            return Mathf.Abs(value._singleValue);
        }
        public static Variant Sign(Variant value)
        {
            if (value._type == ValueType.TypeInt) return Math.Sign(value._intValue);
            return Mathf.Sign(value._singleValue);
        }

        public override string ToString()
        {
            if (_type == ValueType.TypeBool) return (_intValue != 0).ToString();
            if (_type == ValueType.TypeInt) return _intValue.ToString();
            if (_type == ValueType.TypeSingle) return _singleValue.ToString();
            return string.Empty;
        }
    }
}
