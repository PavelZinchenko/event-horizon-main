using GameDatabase.Model;
using Utilites;

namespace Constructor.Ships
{
    public class ShipColorScheme
    {
        public ShipColorScheme()
        {
            Type = SchemeType.Default;
            Color = ColorData.White;
            Hue = 0;
            Saturation = 0;
        }

        public enum SchemeType : byte
        {
            Default = 0,
            Hsv = 1,
        }

        public long Value
        {
            get
            {
                var value = (((((((((ulong)(byte)_type << 8) + _saturation << 8) + _hue) << 8) + _r) << 8) + _g) << 8) + _b;
                return (long)value;
            }
            set
            {
                var data = (ulong)value;

                _b = (byte)data; data >>= 8;
                _g = (byte)data; data >>= 8;
                _r = (byte)data; data >>= 8;
                _hue = (byte)data; data >>= 8;
                _saturation = (byte)data; data >>= 8;
                _type = (SchemeType)(byte)data; data >>= 8;

                IsChanged = true;
            }
        }

        public SchemeType Type
        {
            get { return _type; }
            set
            {
                if (_type == value) return;

                _type = value;
                IsChanged = true;
            }
        }

        public ColorData Color
        {
            get { return new ColorData(_r/255f, _g/255f, _b/255f); }
            set
            {
                var r = (byte)MathEx.RoundToInt(MathEx.Clamp01(value.R) * 255);
                var g = (byte)MathEx.RoundToInt(MathEx.Clamp01(value.G) * 255);
                var b = (byte)MathEx.RoundToInt(MathEx.Clamp01(value.B) * 255);
                if (r == _r && g == _g && b == _b) return;

                _r = r;
                _g = g;
                _b = b;
                IsChanged = true;
            }
        }

        public ColorData HsvColor { get { return new ColorData(_hue/255f, _saturation/255f, 0, 1); } }

        public float Hue
        {
            get { return _hue / 255f; }
            set
            {
                var hue = (byte)MathEx.RoundToInt(MathEx.Clamp01(value) * 255);
                if (_hue == hue) return;

                _hue = hue;
                IsChanged = true;
            }
        }

        public float Saturation
        {
            get { return _saturation / 255f; }
            set
            {
                var saturation = (byte)MathEx.RoundToInt(MathEx.Clamp01(value) * 255);
                if (_saturation == saturation) return;

                _saturation = saturation;
                IsChanged = true;
            }
        }

        public bool IsChanged { get; set; }

        private SchemeType _type;
        private byte _r;
        private byte _g;
        private byte _b;
        private byte _hue;
        private byte _saturation;
    }
}
