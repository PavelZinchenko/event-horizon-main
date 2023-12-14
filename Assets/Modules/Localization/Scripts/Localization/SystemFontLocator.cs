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
			foreach (var info in _fontTable.GetFontList(language))
            {
				Font font;

				if (info.BuiltInFont != null)
					font = info.BuiltInFont;
				else if (_systemFonts.TryGetValue(info.Filename, out var path))
					font = new Font(path);
				else
					continue;

                if (!_cache.TryGetValue(font.name, out var fontAsset))
                {
                    fontAsset = FontAsset.CreateFontAsset(font);
                    _cache.Add(font.name, fontAsset);
                }

                yield return fontAsset;
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
					_systemFonts.TryAdd(System.IO.Path.GetFileName(paths[i]), paths[i]);
            }

			//foreach (var item in _systemFonts)
			//	Debug.LogError(item.Key + ": " + item.Value);
		}
    }
}
