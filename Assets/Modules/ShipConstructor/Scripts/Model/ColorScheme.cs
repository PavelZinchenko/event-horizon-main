using GameDatabase.Model;

namespace Constructor.Model
{
    public struct ColorScheme
    {
        public ColorScheme(ColorData color, float hue = 0, float saturation = 0)
        {
            _initialized = true;
            _color = color;
            Hue = hue;
            Saturation = saturation;
        }

        public ColorData Color => _initialized ? _color : ColorData.White;
        public readonly float Hue;
        public readonly float Saturation;

        public bool IsHsv => _initialized && Hue >= 0.01f && Hue <= 0.99f || Saturation > 0.01f;

        private readonly bool _initialized;
        private readonly ColorData _color;
    }

}
