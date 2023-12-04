using UnityEngine;
using System.Collections.Generic;
using GameDatabase;

namespace Services.Localization
{
	public class LocalizationManager : ILocalization
	{
        public LocalizationManager(LocalizationChangedSignal.Trigger localizationChangedTrigger)
        {
            _localizationChangedTrigger = localizationChangedTrigger;
        }

        public string Language => _localization?.Language ?? _defaultLanguage;

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

        public void Initialize(string language, IDatabase database, bool forceReload = false)
		{
            if (string.IsNullOrEmpty(language))
                language = GetSystemLanguage();

            UnityEngine.Debug.Log("LocalizationManager.Initialize - " + language);

            if (_defaultLocalization == null || forceReload)
                if (!Localization.TryLoad(_defaultLanguage, database, out _defaultLocalization))
                    UnityEngine.Debug.LogError("Can't load localization for " + _defaultLanguage);

            if (string.Equals(language, _defaultLanguage, System.StringComparison.OrdinalIgnoreCase))
                _localization = null;
            else if (_localization == null || forceReload || !string.Equals(language, _localization.Language, System.StringComparison.OrdinalIgnoreCase))
                if (!Localization.TryLoad(language, database, out _localization))
                    UnityEngine.Debug.LogError("Can't load localization for " + language);

            _localizationChangedTrigger.Fire(language);
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

        private Localization _localization;
        private Localization _defaultLocalization;
        private readonly LocalizationChangedSignal.Trigger _localizationChangedTrigger;
        private const string _defaultLanguage = "English";
    }
}