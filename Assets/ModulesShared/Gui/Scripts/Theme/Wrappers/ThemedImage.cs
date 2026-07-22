using System;
using UnityEngine;
using UnityEngine.UI;

namespace Gui.Theme.Wrappers
{
    [AddComponentMenu("UI/ThemedImage")]
    public class ThemedImage : Image
    {
        [SerializeField] private ThemeColor _themeColor;
        [SerializeField] private ThemeColorMode _colorMode;

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
            }
            catch (System.Exception e)
            {
                GameDiagnostics.Debug.LogException(e, gameObject);
            }
        }
    }
}
