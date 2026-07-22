using System;
using UnityEngine;

namespace CodeWriter.ExpressionParser
{
    public class VariantExpressionParser : ExpressionParser<Variant>
    {
        public static readonly ExpressionParser<Variant> Instance = new VariantExpressionParser();

        protected override Variant False { get; } = Variant.False;
        protected override Variant True { get; } = Variant.True;

        protected override Variant Parse(string input) => Variant.Parse(input);

        protected override Variant Negate(Variant v) => -v;
        protected override Variant Add(Variant a, Variant b) => a + b;
        protected override Variant Sub(Variant a, Variant b) => a - b;
        protected override Variant Mul(Variant a, Variant b) => a * b;
        protected override Variant Div(Variant a, Variant b) => a / b;
        protected override Variant Mod(Variant a, Variant b) => a % b;
        protected override Variant Pow(Variant a, Variant b) => Variant.Pow(a, b);
        protected override Variant Equal(Variant a, Variant b) => a == b;
        protected override Variant NotEqual(Variant a, Variant b) => a != b ? 1 : 0;
        protected override Variant LessThan(Variant a, Variant b) => a < b ? 1 : 0;
        protected override Variant LessThanOrEqual(Variant a, Variant b) => a <= b ? 1 : 0;
        protected override Variant GreaterThan(Variant a, Variant b) => a > b ? Variant.True : Variant.False;
        protected override Variant GreaterThanOrEqual(Variant a, Variant b) => a >= b ? Variant.True : Variant.False;
        protected override bool IsTrue(Variant v) => v.AsBool;
        protected override Variant Round(Variant v) => Variant.Round(v);
        protected override Variant Ceiling(Variant v) => Variant.Ceil(v);
        protected override Variant Floor(Variant v) => Variant.Floor(v);
        protected override Variant Log10(Variant v) => Variant.Log10(v);
        protected override Variant Sin(Variant v) => Variant.Sin(v);
        protected override Variant Cos(Variant v) => Variant.Cos(v);
        protected override Variant Tan(Variant v) => Variant.Tan(v);
        protected override Variant Atan(Variant v) => Variant.Atan(v);
        protected override Variant Atan2(Variant x, Variant y) => Variant.Atan2(x, y);
        protected override Variant Rad2Deg(Variant v) => v.AsSingle * Mathf.Rad2Deg;
        protected override Variant Deg2Rad(Variant v) => v.AsSingle * Mathf.Deg2Rad;

        protected override Variant Abs(Variant v) => Variant.Abs(v);

        protected override Variant Sign(Variant v) => Variant.Sign(v);

        protected override Variant Random(Variant minInclusive, Variant maxExclusive)
        {
            return UnityEngine.Random.Range(minInclusive.AsSingle, maxExclusive.AsSingle);
        }

        protected override Variant RandomInt(Variant minInclusive, Variant maxExclusive)
        {
            return UnityEngine.Random.Range(minInclusive.AsInt, maxExclusive.AsInt);
        }

        protected override Variant Log(Variant v, Variant newBase) => Variant.Log(v, newBase);
    }
}
