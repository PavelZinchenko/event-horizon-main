using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace Services.Localization
{
    public interface IKeyNameLocalizer
    {
        string Localize(string path);
    }

    public class KeyNameLocalizer : IKeyNameLocalizer
    {
        public KeyNameLocalizer(ILocalization localization/*, LanguageChangedSignal languageChangedSignal*/)
        {
            _localization = localization;
            //_languageChangedSignal = languageChangedSignal;
            //_languageChangedSignal.Event += OnLanguageChanged;
        }

        public string Localize(string path)
        {
            return TryGetName(path, out var name) ? name : GetDefaultValue(path);
        }

        private string GetDefaultValue(string path)
        {
            var options = InputControlPath.HumanReadableStringOptions.OmitDevice;
            return InputControlPath.ToHumanReadableString(path, options);
        }

        //private void OnLanguageChanged(string language)
        //{
        //    _localizedKeys.Clear();
        //}

        private bool TryGetName(string key, out string value)
        {
            if (_localizedKeys.Count == 0)
            {
                _localizedKeys.Add("<Keyboard>/leftArrow", "←");
                _localizedKeys.Add("<Keyboard>/rightArrow", "→");
                _localizedKeys.Add("<Keyboard>/upArrow", "↑");
                _localizedKeys.Add("<Keyboard>/downArrow", "↓");
                _localizedKeys.Add("<Keyboard>/space", _localization.GetString("$KeySpace"));
            }

            return _localizedKeys.TryGetValue(key, out value);
        }

        private readonly Dictionary<string, string> _localizedKeys = new();
        //private readonly LanguageChangedSignal _languageChangedSignal;
        private readonly ILocalization _localization;
    }
}
