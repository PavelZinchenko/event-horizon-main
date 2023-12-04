using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Services.Localization
{
    [CreateAssetMenu(fileName = "FontTable", menuName = "Localization/FontTable", order = 1)]
    public class FontTable : ScriptableObject
    {
        [SerializeField] private FontData[] _windowsFonts = { };
        [SerializeField] private FontData[] _androidFonts = { };
        [SerializeField] private FontData[] _iosFonts = { };
        [SerializeField] private FontData[] _macFonts = { };
        [SerializeField] private FontData[] _linuxFonts = { };

        [SerializeField] private string[] _noExtraFontLanguages;

#if UNITY_ANDROID
        private FontData[] FontList => _androidFonts;
#elif UNITY_IOS
        private FontData[] FontList => _iosFonts;
#elif UNITY_STANDALONE_WIN
        private FontData[] FontList => _windowsFonts;
#elif UNITY_STANDALONE_OSX
        private FontData[] FontList => _macFonts;
#elif UNITY_STANDALONE_LINUX
        private FontData[] FontList => _linuxFonts;
#endif

        public IEnumerable<string> FindSystemFonts(string language)
        {
            bool wasFound = false;

            for (int i = 0; i < FontList.Length; i++)
            {
                var data = FontList[i];
                if (!string.Equals(data.Language, language, StringComparison.OrdinalIgnoreCase)) continue;

                wasFound = true;
                if (string.IsNullOrEmpty(data.Filename)) continue;

                yield return data.Filename;
            }

            if (wasFound) yield break;

            if (_noExtraFontLanguages.Contains(language, StringComparer.OrdinalIgnoreCase)) yield break;

            foreach (var data in FontList.Where(data => string.IsNullOrEmpty(data.Language)))
                yield return data.Filename;
        }

        [Serializable]
        public struct FontData
        {
            public string Language;
            public string Filename;
        }
    }
}
