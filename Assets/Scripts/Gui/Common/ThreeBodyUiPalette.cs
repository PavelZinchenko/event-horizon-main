using GameDatabase.DataModel;
using ReUI;
using UnityEngine;

namespace Gui.Common
{
    public static class ThreeBodyUiPalette
    {
        public static Color PanelDeep { get; private set; } = new(0.035f, 0.012f, 0.075f, 0.985f);
        public static Color Panel { get; private set; } = new(0.10f, 0.035f, 0.18f, 0.97f);
        public static Color PanelSoft { get; private set; } = new(0.16f, 0.06f, 0.27f, 0.95f);
        public static Color PanelSelected { get; private set; } = new(0.28f, 0.10f, 0.46f, 0.98f);
        public static Color Button { get; private set; } = new(0.32f, 0.11f, 0.52f, 1f);
        public static Color ButtonDim { get; private set; } = new(0.21f, 0.07f, 0.35f, 0.97f);
        public static Color Accent { get; private set; } = new(0.72f, 0.45f, 1f, 1f);
        public static Color AccentSoft { get; private set; } = new(0.86f, 0.74f, 1f, 1f);
        public static Color TextMuted { get; private set; } = new(0.78f, 0.72f, 0.88f, 1f);

        public static void Configure(UiSettings settings)
        {
            if (settings == null)
                return;

            PanelDeep = WithAlpha(settings.BackgroundDark, 0.985f);
            Panel = WithAlpha(settings.WindowColor, 0.97f);
            PanelSoft = WithAlpha(settings.SelectionColor, 0.70f);
            PanelSelected = WithAlpha(settings.SelectionColor, 0.98f);
            Button = WithAlpha(settings.ButtonColor, 1f);
            ButtonDim = Dim(WithAlpha(settings.ButtonColor, 0.97f), 0.70f);
            Accent = WithAlpha(settings.IconColor, 1f);
            AccentSoft = WithAlpha(settings.HeaderTextColor, 1f);
            TextMuted = WithAlpha(settings.PaleTextColor, 1f);

            ApplyLocalThemeOverride();
        }

        /// <summary>
        /// The database remains the authoritative default palette. A local
        /// Settings selection only changes this runtime presentation palette;
        /// it never writes database JSON, mod content or a save file.
        /// </summary>
        private static void ApplyLocalThemeOverride()
        {
            if (!ReUIBootstrap.IsEnabled || !ReUIPalette.HasCustomThemeColor)
                return;

            Color selected = ReUIPalette.ThemeColor;
            Color.RGBToHSV(selected, out float hue, out float saturation, out float value);
            saturation = Mathf.Clamp01(Mathf.Max(0.22f, saturation));
            value = Mathf.Clamp(value, 0.32f, 1f);

            PanelDeep = FromHsv(hue, Mathf.Lerp(0.26f, 0.54f, saturation), 0.055f, 0.985f);
            Panel = FromHsv(hue, Mathf.Lerp(0.30f, 0.60f, saturation), 0.120f, 0.970f);
            PanelSoft = FromHsv(hue, Mathf.Lerp(0.36f, 0.68f, saturation), 0.220f, 0.950f);
            PanelSelected = FromHsv(hue, Mathf.Lerp(0.42f, 0.76f, saturation), 0.340f, 0.980f);
            Button = FromHsv(hue, saturation, Mathf.Max(0.40f, value * 0.72f), 1f);
            ButtonDim = FromHsv(hue, saturation * 0.82f, Mathf.Max(0.20f, value * 0.45f), 0.970f);
            Accent = WithAlpha(selected, 1f);
            AccentSoft = WithAlpha(Color.Lerp(selected, Color.white, 0.48f), 1f);
            TextMuted = WithAlpha(Color.Lerp(selected, Color.white, 0.58f), 1f);
        }

        private static Color FromHsv(float hue, float saturation, float value, float alpha)
        {
            Color color = Color.HSVToRGB(hue, Mathf.Clamp01(saturation), Mathf.Clamp01(value));
            color.a = alpha;
            return color;
        }

        /// <summary>
        /// Loads the authored captain shortcut sprite.  This deliberately uses
        /// Unity's normal Sprite importer: the shortcut must never fall back to
        /// a Base64 texture, a relation-panel preview, or a runtime vector icon.
        /// </summary>
        public static Sprite LoadCaptainIcon()
        {
            if (_captainIcon == null)
                _captainIcon = Resources.Load<Sprite>("Textures/UI/captain");
            return _captainIcon;
        }

        private static Color WithAlpha(Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }

        private static Color Dim(Color color, float multiplier)
        {
            color.r *= multiplier;
            color.g *= multiplier;
            color.b *= multiplier;
            return color;
        }

        private static Sprite _captainIcon;
    }
}
