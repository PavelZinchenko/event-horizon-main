using System.Collections.Generic;

namespace Services.Localization
{
	internal struct KeyParser
	{
		public KeyParser(ILocalization localization)
		{
			_localization = localization;
			_key = string.Empty;
			_params = null;
		}

		public bool TryParse(string text, ref int index)
		{
			var length = text.Length;
			var startIndex = index + 1;

			if (index >= length || text[index++] != SpecialChar)
				return false;

			if (index >= length || !char.IsLetter(text[index]))
				return false;

			var localizedParam = false;
			var firstParamIndex = 0;
			var paramIndex = 0;
			if (_params != null) _params.Clear();

			while (++index < text.Length)
			{
				var ch = text[index];
				if (paramIndex > 0 && ch == SeparatorChar)
					AddParam(text.Substring(paramIndex, index - paramIndex), localizedParam);

				if (ch == SeparatorChar)
				{
					if (firstParamIndex == 0)
						firstParamIndex = index;

					paramIndex = index + 1;
					localizedParam = false;
				}
				else if (ch == SpecialChar)
				{
					localizedParam = true;
				}
				else if (!IsValidChar(ch))
				{
					break;
				}
			}

			if (paramIndex > 0)
				AddParam(text.Substring(paramIndex, index - paramIndex), localizedParam);

			_key = text.Substring(startIndex, firstParamIndex > 0 ? firstParamIndex - startIndex : index - startIndex);
			return true;
		}

		private void AddParam(string param, bool localized)
		{
			if (_params == null) _params = new List<string>();
			_params.Add(localized ? _localization.GetString(param, null) : param);
		}

		private static bool IsValidChar(char ch)
		{
			if (ch == '_')
				return true;

			return char.IsLetterOrDigit(ch);
		}

		public string Key => _key;
		public Parameters Params => new Parameters(_params);

		private string _key;
		private List<string> _params;
		private readonly ILocalization _localization;

		public const char SpecialChar = '$';
		public const char UppedCaseChar = '&';
		public const char SeparatorChar = '|';
	}

	internal readonly struct Parameters
	{
		private readonly IList<object> _objectValues;
		private readonly IList<string> _stringValues;

		public int Count
		{
			get
			{
				if (_stringValues != null) return _stringValues.Count;
				if (_objectValues != null) return _objectValues.Count;
				return 0;
			}
		}

		public string this[int index] => _stringValues?[index] ?? _objectValues?[index].ToString();

		public Parameters(IList<object> values)
		{
			_objectValues = values;
			_stringValues = null;
		}

		public Parameters(IList<string> values)
		{
			_stringValues = values;
			_objectValues = null;
		}
	}
}
