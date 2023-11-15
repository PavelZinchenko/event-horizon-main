using System;
using System.Text;
using System.Collections.Generic;
using UnityEngine.Assertions;
using GameDatabase;

namespace Services.Localization
{
    internal class Localization
    {
		private const char SpecialChar = '$';
		private List<PluralForm> _pluralForms = new();
        private Dictionary<string, string> _keys;

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

			localization = new Localization(keys);
			return true;
		}

		public bool TryGetString(string key, object[] parameters, out string result)
		{
			try
			{
				if (string.IsNullOrEmpty(key) || key[0] != SpecialChar)
				{
					result = key;
					return true;
				}

				if (!_keys.TryGetValue(key.Substring(1), out var value))
				{
					result = key;
					return false;
				}

				value = value.Replace("\\n", "\n").Replace("\\", string.Empty);
				result = ApplyParameters(value, parameters);
				return true;
			}
			catch (Exception e)
			{
				UnityEngine.Debug.LogException(e);
				result = string.Empty;
				return false;
			}
		}

		private Localization(Dictionary<string, string> keys)
		{
			_keys = keys;
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

		private string ApplyParameters(string value, object[] parameters)
		{
			var index = value.IndexOf(SpecialChar);
			if (index < 0)
				return value;

			var builder = new StringBuilder(value.Substring(0, index));

			while (true)
			{
				++index;
				var ch = value[index];
				if (ch == '{')
				{
					var end = value.IndexOf('}', index + 1);
					Assert.IsTrue(end > index);
					AddPluralForm(builder, value.Substring(index + 1, end - index - 1), parameters);
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

					if (id > 0 && id <= parameters.Length)
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
				index = value.IndexOf(SpecialChar, index);
				if (index < 0)
				{
					builder.Append(value.Substring(old));
					break;
				}
				else
				{
					builder.Append(value.Substring(old, index - old));
				}
			}

			return builder.ToString();
		}

		private void AddPluralForm(StringBuilder builder, string format, object[] parameters)
		{
			var items = format.Split('|');
			var paramId = System.Convert.ToInt32(items[0]);
			if (paramId <= 0 || paramId > parameters.Length)
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

		private void AddInvalidParameter(StringBuilder builder, int id)
		{
			builder.Append('[');
			builder.Append(SpecialChar);
			builder.Append(id);
			builder.Append(']');
		}

		private void AddInvalidParameter(StringBuilder builder, string key)
		{
			builder.Append('[');
			builder.Append(SpecialChar);
			builder.Append(key);
			builder.Append(']');
		}
	}
}
