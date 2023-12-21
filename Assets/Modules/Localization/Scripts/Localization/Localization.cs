using System;
using System.Text;
using System.Collections.Generic;
using UnityEngine.Assertions;
using GameDatabase;

namespace Services.Localization
{
    internal class Localization
    {
		private readonly List<PluralForm> _pluralForms = new();
        private readonly Dictionary<string, string> _keys;

		public string Language { get; }

		public static bool TryLoad(string language, IDatabase database, out Localization localization)
        {
			localization = null;
			if (string.IsNullOrEmpty(language)) 
				return false;

			var keys = new Dictionary<string, string>();
			ResourceLoader.TryLoadFromResources(language, keys);
			if (database != null)
				ResourceLoader.TryLoadFromDatabase(database, language, keys);

			if (keys.Count == 0) 
				return false;

			localization = new Localization(keys, language);
			return true;
		}

		public bool TryGetString(string key, Parameters parameters, out string result)
		{
			try
			{
				if (string.IsNullOrEmpty(key))
				{
					result = string.Empty;
					return true;
				}

				if (key[0] == KeyParser.SpecialChar)
				{
					result = GetKey(key.Substring(1), parameters);
					return result != null;
				}
				else if (key[0] == KeyParser.UppedCaseChar)
				{
					result = GetKey(key.Substring(1), parameters)?.ToUpper();
					return result != null;
				}
				else
				{
					result = key;
					return true;
				}
			}
			catch (Exception e)
			{
				UnityEngine.Debug.LogException(e);
				result = string.Empty;
				return false;
			}
		}

		public string GetKey(string key, Parameters parameters)
		{
			if (string.IsNullOrEmpty(key))
				return string.Empty;

			string value;
			if (!_keys.TryGetValue(key, out value))
				return null;

			value = value.Replace("\\n", "\n").Replace("\\", string.Empty);
			return ApplyParameters(value, parameters);
		}

		private Localization(Dictionary<string, string> keys, string language)
		{
			_keys = keys;
			Language = language;
			InitPluralForms();
		}

		private void InitPluralForms()
		{
			string format;
			if (!_keys.TryGetValue("PluralFroms", out format) || string.IsNullOrEmpty(format))
				return;

			foreach (var item in format.Split(' '))
				_pluralForms.Add(PluralForm.FromString(item));
		}

		private string ApplyParameters(string value, Parameters parameters)
		{
			if (parameters.Count == 0)
				return value;

			var index = value.IndexOf(KeyParser.SpecialChar);
			if (index < 0)
				return value;

			var builder = new StringBuilder(value.Substring(0, index));

			var length = value.Length;
			index++;
			while (true)
			{
				if (index >= length)
				{
					UnityEngine.Debug.LogException(new ArgumentException("wrong localized string: " + value));
					return string.Empty;
				}

				var ch = value[index];
				if (ch == '{')
				{
					var end = value.IndexOf('}', index + 1);
					Assert.IsTrue(end > index);
					AddPluralForm(builder, value.Substring(index + 1, end - index - 1), parameters);
					index = end + 1;
				}
				else if (ch == '[')
				{
					var end = value.IndexOf(']', index + 1);
					Assert.IsTrue(end > index);
					AddEnumeration(builder, value.Substring(index + 1, end - index - 1), parameters);
					index = end + 1;
				}
				else if (char.IsDigit(ch))
				{
					var id = 0;
					while (index < value.Length && char.IsDigit(value[index]))
					{
						id = id * 10 + (value[index] - '0');
						++index;
					}

					if (id > 0 && id <= parameters.Count)
						builder.Append(parameters[id - 1]);
					else
						AddInvalidParameter(builder, id);
				}
				else if (char.IsLetter(ch))
				{
					var start = index;
					while (index < value.Length && char.IsLetterOrDigit(value[index]))
						++index;

					var key = value.Substring(start, index - start);
					string parameter;
					if (_keys.TryGetValue(key, out parameter))
						builder.Append(parameter);
					else
						AddInvalidParameter(builder, key);
				}

				var old = index;
				index = value.IndexOf(KeyParser.SpecialChar, index) + 1;
				if (index <= 0)
				{
					builder.Append(value.Substring(old));
					break;
				}
				else if (index == value.Length)
				{
					builder.Append(value.Substring(old));
					break;
				}
				else
				{
					builder.Append(value.Substring(old, index - old - 1));
				}
			}

			return builder.ToString();
		}

		private void AddPluralForm(System.Text.StringBuilder builder, string format, Parameters parameters)
		{
			var items = format.Split(KeyParser.SeparatorChar);
			if (!int.TryParse(items[0], out var paramId))
			{
				UnityEngine.Debug.LogException(new LocalizationException("Invalid string format - " + format));
				return;
			}

			if (paramId <= 0 || paramId > parameters.Count)
			{
				AddInvalidParameter(builder, paramId);
				return;
			}

			var count = System.Convert.ToInt32(parameters[paramId - 1]);
			var id = 0;
			foreach (var item in _pluralForms)
			{
				if (item.IsMatch(count))
				{
					id = item.Id;
					break;
				}
			}
			if (id > 0)
				builder.Append(ApplyParameters(items[id], parameters));
			else
				AddInvalidParameter(builder, id);
		}

		private void AddEnumeration(System.Text.StringBuilder builder, string format, Parameters parameters)
		{
			var items = format.Split(KeyParser.SeparatorChar);
			if (items.Length < 2)
			{
				AddInvalidParameter(builder, 0);
				return;
			}

			var index = 1 + System.Convert.ToInt32(parameters[0]) % (items.Length - 1);
			builder.Append(items[index]);
		}

		private void AddInvalidParameter(StringBuilder builder, int id)
		{
			builder.Append('[');
			builder.Append(KeyParser.SpecialChar);
			builder.Append(id);
			builder.Append(']');
		}

		private void AddInvalidParameter(StringBuilder builder, string key)
		{
			builder.Append('[');
			builder.Append(KeyParser.SpecialChar);
			builder.Append(key);
			builder.Append(']');
		}
	}
}
