using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using UnityEngine.UIElements;
using UnityEngine.TextCore.Text;

namespace Services.Localization
{
    public class SystemFontLocator : MonoBehaviour
    {
        [SerializeField] private int _fontSize = 12;
        [SerializeField] private PanelTextSettings _textSettings;
        [SerializeField] private FontTable _fontTable;

        private string _language;
        private ILocalization _localization;
        private LocalizationChangedSignal _localizationChangedSignal;
        private readonly Dictionary<string, FontAsset> _cache = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, string> _systemFonts = new(StringComparer.OrdinalIgnoreCase);

        [Inject]
        private void Initialize(
            ILocalization localization,
            LocalizationChangedSignal localizationChangedSignal)
        {
            _localization = localization;
            _localizationChangedSignal = localizationChangedSignal;
            _localizationChangedSignal.Event += OnLocalizationChanged;

            LoadFontList();
        }

        private void OnDestroy()
        {
            foreach (var item in _cache)
                Destroy(item.Value);

            _cache.Clear();
        }

        private IEnumerable<FontAsset> LookForFont(string language)
        {
            foreach (var name in _fontTable.FindSystemFonts(language))
            {
                if (!_systemFonts.TryGetValue(name, out var path)) continue;

                if (!_cache.TryGetValue(name, out var font))
                {
                    font = FontAsset.CreateFontAsset(new Font(path));
                    _cache.Add(name, font);
                }

                yield return font;
            }
        }

        private void OnLocalizationChanged(string language)
        {
            if (_language == language) return;
            _language = language;

            _textSettings.fallbackFontAssets.Clear();

            foreach (var font in LookForFont(_localization.Language))
            {
                Debug.Log("Font loaded: " + font.sourceFontFile.name);
                _textSettings.fallbackFontAssets.Add(font);
            }
        }

        private void LoadFontList()
        {
            if (_systemFonts.Count == 0)
            {
                var paths = Font.GetPathsToOSFonts();

                for (var i = 0; i < paths.Length; ++i)
                    _systemFonts.Add(System.IO.Path.GetFileName(paths[i]), paths[i]);
            }

            //foreach (var item in _systemFonts)
            //    Debug.LogError(item.Key + ": " + item.Value);
        }

        [Serializable]
        public struct FontInfo
        {
            public string Language;
            public string[] FontList;
        }

        [Serializable]
        public struct FontData
        {
            public string Language;
            public string Filename;
        }
    }
}
