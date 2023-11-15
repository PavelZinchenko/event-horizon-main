using UnityEngine;
using System.Collections.Generic;
using GameDatabase;

namespace Services.Localization
{
	public class LocalizationManager : ILocalization
	{
		public string GetString(string key, params object[] parameters)
		{
            string result;
            if (_localization != null && _localization.TryGetString(key, parameters, out result))
                return result;

            if (_defaultLocalization != null && _defaultLocalization.TryGetString(key, parameters, out result))
                return result;

            Debug.Log("key not found: '" + key + "'");
            return key;
		}

        public void Initialize(string language, IDatabase database = null)
		{
            if (string.IsNullOrEmpty(language))
                language = GetSystemLanguage();

            UnityEngine.Debug.Log("LocalizationManager.Initialize - " + language);

            if (!Localization.TryLoad(_defaultLanguage, database, out _defaultLocalization))
                UnityEngine.Debug.LogError("Can't load localization for " + _defaultLanguage);

            if (!string.Equals(language, _defaultLanguage, System.StringComparison.InvariantCultureIgnoreCase))
                if (!Localization.TryLoad(language, database, out _localization))
                    UnityEngine.Debug.LogError("Can't load localization for " + language);
        }

        public List<XmlLanguageInfo> LoadLocalizationList()
        {
			return ResourceLoader.LoadLocalizationList();
        }

        private static string GetSystemLanguage()
        {
            var language = Application.systemLanguage;

            switch (language)
            {
                case SystemLanguage.Chinese:
                case SystemLanguage.ChineseSimplified:
                case SystemLanguage.ChineseTraditional:
                    return SystemLanguage.Chinese.ToString();
                default:
                    return language.ToString();
            }
        }

        private const string _defaultLanguage = "English";
        private Localization _localization;
        private Localization _defaultLocalization;
    }
}