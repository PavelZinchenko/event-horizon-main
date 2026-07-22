using System;
using UnityEngine;
using UnityEngine.UI;

namespace Gui.Theme.Wrappers
{
    [AddComponentMenu("UI/ThemedText")]
    public class ThemedText : Text
    {
        [SerializeField] private ThemeColor _themeColor;
        [SerializeField] private ThemeColorMode _colorMode;
        [SerializeField] private ThemeFont _themeFont;
        [SerializeField] private ThemeFontSize _themeFontSize;

        [NonSerialized] private bool _colorInitialized;

        public override Color color
        {
            get => base.color;
            set
            {
                base.color = value;
                _colorInitialized = true;
            }
        }

        protected override void Start()
        {
            base.Start();

#if UNITY_EDITOR
            if (!UnityEditor.EditorApplication.isPlaying) return;
#endif

            try
            {
                if (!_colorInitialized && _themeColor != ThemeColor.Default)
                    color = UiTheme.Current.GetColor(_themeColor).ApplyColorMode(_colorMode);

                if (_themeFont == ThemeFont.Default) return;

                var fontInfo = UiTheme.Current.GetFont(_themeFont);
                int baseFontSize = _themeFontSize != ThemeFontSize.Default ? UiTheme.Current.GetFontSize(_themeFontSize) : fontSize;

                font = fontInfo.Font;
                fontSize = Mathf.RoundToInt(baseFontSize * fontInfo.SizeMultiplier);
                if (resizeTextForBestFit)
                {
                    resizeTextMinSize = Mathf.RoundToInt(resizeTextMinSize * fontInfo.SizeMultiplier);
                    resizeTextMaxSize = Mathf.RoundToInt(baseFontSize * fontInfo.SizeMultiplier);
                }
            }
            catch (System.Exception e)
            {
                GameDiagnostics.Debug.LogException(e, gameObject);
            }
        }
    }
}
