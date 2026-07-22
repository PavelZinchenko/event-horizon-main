using System;
using UnityEngine;

namespace ReUI
{
    /// <summary>
    /// Shared ReUI colour palette.  The selected theme is deliberately kept in
    /// PlayerPrefs instead of the game database: choosing a colour is a local
    /// presentation preference and must not alter saves, ships or mod content.
    /// </summary>
    public static class ReUIPalette
    {
        private const string ThemeColorPreference = "ReUI.ThemeColor";

        private static readonly Color DefaultThemeColor = new(0.700f, 0.440f, 1.000f, 1.000f);
        private static bool _initialized;
        private static Color _themeColor;

        public static event Action<Color> ThemeChanged;

        public static Color ThemeColor
        {
            get
            {
                EnsureInitialized();
                return _themeColor;
            }
        }

        /// <summary>
        /// Indicates that the player has explicitly chosen a local override.
        /// This lets authored database colours remain untouched until a player
        /// actually selects a new theme in Settings.
        /// </summary>
        public static bool HasCustomThemeColor
        {
            get
            {
                EnsureInitialized();
                return PlayerPrefs.HasKey(ThemeColorPreference);
            }
        }

        // The named properties are intentionally preserved so the existing ReUI
        // stylers can update with a new theme without changing gameplay UI code.
        public static Color CanvasTint => MakeGlass(0.075f, 0.09f);
        public static Color GlassPrimary => MakeGlass(0.245f, 0.26f);
        public static Color GlassSecondary => MakeGlass(0.300f, 0.20f);
        public static Color GlassElevated => MakeGlass(0.390f, 0.34f);
        public static Color GlassSoft => MakeGlass(0.430f, 0.16f);

        public static Color Outline => WithAlpha(BlendWithWhite(0.42f), 0.32f);
        public static Color OutlineStrong => WithAlpha(BlendWithWhite(0.70f), 0.52f);
        public static Color Highlight => WithAlpha(BlendWithWhite(0.86f), 0.16f);

        public static Color TextPrimary => new(0.980f, 0.950f, 1.000f, 1.000f);
        public static Color TextSecondary => Color.Lerp(TextPrimary, ThemeColor, 0.32f);
        public static Color TextMuted => Color.Lerp(new Color(0.650f, 0.620f, 0.720f, 1.000f), ThemeColor, 0.22f);

        // These names are retained for semantic callers. AccentCyan is the
        // selected theme accent; the two variants provide readable nearby hues.
        public static Color AccentCyan => ThemeColor;
        public static Color AccentBlue => ShiftHue(-0.045f, 0.96f, 1.00f);
        public static Color AccentPurple => ShiftHue(0.055f, 0.92f, 1.00f);
        public static readonly Color AccentGreen = new(0.220f, 0.900f, 0.650f, 1.000f);
        public static readonly Color AccentGold = new(1.000f, 0.710f, 0.240f, 1.000f);
        public static readonly Color AccentRed = new(1.000f, 0.330f, 0.400f, 1.000f);

        public static void SetThemeColor(Color color)
        {
            color.a = 1f;
            _themeColor = color;
            _initialized = true;
            PlayerPrefs.SetString(ThemeColorPreference, "#" + ColorUtility.ToHtmlStringRGBA(color));
            PlayerPrefs.Save();
            ThemeChanged?.Invoke(color);
        }

        public static void ResetThemeColor()
        {
            _themeColor = DefaultThemeColor;
            _initialized = true;
            PlayerPrefs.DeleteKey(ThemeColorPreference);
            PlayerPrefs.Save();
            ThemeChanged?.Invoke(_themeColor);
        }

        public static Color WithAlpha(Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }

        public static Color AccentFor(ReUIIconKind kind)
        {
            return kind switch
            {
                ReUIIconKind.Fleet => AccentCyan,
                ReUIIconKind.Technology => AccentBlue,
                ReUIIconKind.ShipEditor => AccentPurple,
                ReUIIconKind.Equipment => AccentGold,
                ReUIIconKind.Missions => AccentGreen,
                ReUIIconKind.Settings => AccentCyan,
                ReUIIconKind.Battle => AccentRed,
                ReUIIconKind.StarMap => AccentBlue,
                ReUIIconKind.Store => AccentGold,
                ReUIIconKind.Multiplayer => AccentPurple,
                ReUIIconKind.Encyclopedia => AccentGreen,
                ReUIIconKind.Back => TextSecondary,
                ReUIIconKind.Undo => TextPrimary,
                ReUIIconKind.Close => AccentRed,
                ReUIIconKind.Skills => AccentGreen,
                ReUIIconKind.Faction => AccentPurple,
                ReUIIconKind.Captain => AccentCyan,
                _ => AccentCyan,
            };
        }

        private static void EnsureInitialized()
        {
            if (_initialized) return;

            _themeColor = DefaultThemeColor;
            string serialized = PlayerPrefs.GetString(ThemeColorPreference, string.Empty);
            if (!string.IsNullOrEmpty(serialized) &&
                ColorUtility.TryParseHtmlString(serialized, out Color parsed))
            {
                parsed.a = 1f;
                _themeColor = parsed;
            }

            _initialized = true;
        }

        private static Color MakeGlass(float value, float alpha)
        {
            Color.RGBToHSV(ThemeColor, out float hue, out float saturation, out _);
            Color color = Color.HSVToRGB(hue, Mathf.Lerp(0.32f, 0.58f, saturation), value);
            color.a = alpha;
            return color;
        }

        private static Color BlendWithWhite(float amount)
        {
            return Color.Lerp(ThemeColor, Color.white, Mathf.Clamp01(amount));
        }

        private static Color ShiftHue(float offset, float saturationMultiplier, float value)
        {
            Color.RGBToHSV(ThemeColor, out float hue, out float saturation, out _);
            hue = Mathf.Repeat(hue + offset, 1f);
            return Color.HSVToRGB(hue, Mathf.Clamp01(saturation * saturationMultiplier), value);
        }
    }
}
