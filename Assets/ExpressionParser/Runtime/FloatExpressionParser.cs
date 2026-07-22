using System;
using System.Globalization;
using UnityEngine;

namespace CodeWriter.ExpressionParser
{
    public class FloatExpressionParser : ExpressionParser<float>
    {
        public static readonly ExpressionParser<float> Instance = new FloatExpressionParser();

        protected override float False { get; } = 0f;
        protected override float True { get; } = 1f;

        protected override float Parse(string input) =>
            float.Parse(input, NumberStyles.Any, CultureInfo.InvariantCulture);

        protected override float Negate(float v) => -v;
        protected override float Add(float a, float b) => a + b;
        protected override float Sub(float a, float b) => a - b;
        protected override float Mul(float a, float b) => a * b;
        protected override float Div(float a, float b) => a / b;
        protected override float Mod(float a, float b) => a % b;
        protected override float Pow(float a, float b) => Mathf.Pow(a, b);
        protected override float Equal(float a, float b) => Mathf.Approximately(a, b) ? 1 : 0;
        protected override float NotEqual(float a, float b) => !Mathf.Approximately(a, b) ? 1 : 0;
        protected override float LessThan(float a, float b) => a < b ? 1 : 0;
        protected override float LessThanOrEqual(float a, float b) => a <= b ? 1 : 0;
        protected override float GreaterThan(float a, float b) => a > b ? 1 : 0;
        protected override float GreaterThanOrEqual(float a, float b) => a >= b ? 1 : 0;
        protected override bool IsTrue(float v) => !Mathf.Approximately(v, 0);
        protected override float Round(float v) => (float) Math.Round(v);
        protected override float Ceiling(float v) => (float) Math.Ceiling(v);
        protected override float Floor(float v) => (float) Math.Floor(v);
        protected override float Log10(float v) => (float) Math.Log10(v);
        protected override float Sin(float v) => Mathf.Sin(v);

        protected override float Cos(float v) => Mathf.Cos(v);

        protected override float Tan(float v) => Mathf.Tan(v);

        protected override float Atan(float v) => Mathf.Atan(v);
        protected override float Atan2(float x, float y) => Mathf.Atan2(x, y);
        protected override float Rad2Deg(float v) => Mathf.Rad2Deg * v;
        protected override float Deg2Rad(float v) => Mathf.Deg2Rad * v;

        protected override float Abs(float v) => Mathf.Abs(v);

        protected override float Sign(float v) => Mathf.Sign(v);

        protected override float Random(float minInclusive, float maxExclusive)
        {
            return UnityEngine.Random.Range(minInclusive, maxExclusive);
        }

        protected override float RandomInt(float minInclusive, float maxExclusive)
        {
            return UnityEngine.Random.Range((int) minInclusive, (int) maxExclusive);
        }

        protected override float Log(float v, float newBase) => (float) Math.Log(v, newBase);
    }
}