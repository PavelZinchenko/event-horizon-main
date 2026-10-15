using System;
using Utilites;

namespace GameDatabase.Model
{
    // Engine-free RGBA color. Engine code converts it with ToUnity().
    public readonly struct ColorData
    {
        public static readonly ColorData White = new ColorData(1f, 1f, 1f, 1f);
        public static readonly ColorData Gray = new ColorData(0.5f, 0.5f, 0.5f, 1f);
        public static readonly ColorData Cyan = new ColorData(0f, 1f, 1f, 1f);

        public ColorData(string color)
        {
            this = FromString(color);
        }

        public ColorData(float r, float g, float b, float a = 1f)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        public float R { get; }
        public float G { get; }
        public float B { get; }
        public float A { get; }

        private static ColorData FromString(string color)
        {
            if (string.IsNullOrEmpty(color) || color[0] != '#')
                return White;

            uint value;

            try
            {
                value = Convert.ToUInt32(color.Substring(1), 16);
            }
            catch (Exception)
            {
                GameDiagnostics.Trace.LogError($"Invalid color: {color}");
                return White;
            }

            if (value <= 0xffffff)
                value |= 0xff000000;

            var b = (byte)(value & 0xff); value >>= 8;
            var g = (byte)(value & 0xff); value >>= 8;
            var r = (byte)(value & 0xff); value >>= 8;
            var a = (byte)(value & 0xff);

            return new ColorData(r / 255f, g / 255f, b / 255f, a / 255f);
        }

        public override string ToString()
        {
            var a = ToByte(A);
            return "#" + (a < 0xff ? a.ToString("X2") : string.Empty) + ToByte(R).ToString("X2") + ToByte(G).ToString("X2") + ToByte(B).ToString("X2");
        }

        // Same as Unity's Color -> Color32 conversion.
        private static byte ToByte(float value)
        {
            float scaled = MathEx.Clamp01(value) * 255f;
            return (byte)Math.Round(scaled);
        }
    }
}
