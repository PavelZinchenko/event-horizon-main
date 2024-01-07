using UnityEngine;
using System.Collections.Generic;
using GameDatabase;
using System.Text;

namespace Services.Localization
{
	public class LocalizationManager : ILocalization
	{
        public LocalizationManager(LocalizationChangedSignal.Trigger localizationChangedTrigger)
        {
            _localizationChangedTrigger = localizationChangedTrigger;
        }

        public string Language => _localization?.Language ?? _defaultLocalization?.Language;

		public string Localize(string text)
		{
			if (string.IsNullOrEmpty(text))
				return string.Empty;

			try
			{
				var length = text.Length;
				var parser = new KeyParser(this);
				var builder = new LazyStringBuilder();
				var lastPosition = 0;

				var index = 0;
				while (index < length)
				{
					if (text[index] != KeyParser.SpecialChar)
					{
						index++;
						continue;
					}

					var startIndex = index;
					if (!parser.TryParse(text, ref index))
						continue;

					if (startIndex > lastPosition)
						builder.Builder.Append(text, lastPosition, startIndex - lastPosition);

					builder.Builder.Append(GetKey(parser.Key, parser.Params));
					lastPosition = index;
				}

				if (!builder)
					return text;

				builder.Builder.Append(text.Substring(lastPosition));
				return builder.Builder.ToString();
			}
			catch (System.Exception e)
			{
				Debug.LogException(e);
				return text;
			}
		}

		public string GetString(string key, params object[] parameters)
		{
            string result;
            if (_localization != null && _localization.TryGetString(key, new Parameters(parameters), out result))
                return result;

            if (_defaultLocalization != null && _defaultLocalization.TryGetString(key, new Parameters(parameters), out result))
                return result;

            Debug.Log("key not found: '" + key + "'");
            return key;
		}

		private string GetKey(string key, Parameters parameters)
		{
			if (string.IsNullOrEmpty(key))
				return key;

			var result = _localization?.GetKey(key, parameters) ?? _defaultLocalization?.GetKey(key, parameters);
			if (result == null)
			{
				Debug.LogError("key not found: '" + key + "'");
				return key;
			}

			return result;
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

		private struct LazyStringBuilder
		{
			public static implicit operator bool(LazyStringBuilder builder)
			{
				return builder._builder != null;
			}

			public StringBuilder Builder { get { return _builder ?? (_builder = new StringBuilder()); } }

			private StringBuilder _builder;
		}
	}
}