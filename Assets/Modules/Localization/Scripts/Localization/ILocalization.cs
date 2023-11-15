using System.Collections.Generic;
using System.Xml.Serialization;

namespace Services.Localization
{
	public interface ILocalization
	{
		string GetString(string key, params object[] parameters);
        void Initialize(string language, GameDatabase.IDatabase database = null);
        List<XmlLanguageInfo> LoadLocalizationList();
    }

    public class XmlLanguageInfo
    {
        [XmlAttribute]
        public string name = string.Empty;
        [XmlAttribute]
        public string folder = string.Empty;
        [XmlAttribute]
        public string iso = string.Empty;
        [XmlAttribute]
        public string icon = string.Empty;
    }

    [XmlRootAttribute("languages")]
    public class XmlLocalizationList
    {
        [XmlElement("string")]
        public List<XmlLanguageInfo> items = new();
    }

    public class LocalizationException : System.Exception
    {
        public LocalizationException() { }
        public LocalizationException(string message) : base(message) { }
        public LocalizationException(string message, System.Exception inner) : base(message, inner) { }
    }

    public class XmlKeyValuePair
    {
        [XmlAttribute]
        public string name = string.Empty;
        [XmlText]
        public string value = string.Empty;
    }

    [XmlRootAttribute("resources")]
    public class XmlLocalization
    {
        [XmlElement("string")]
        public List<XmlKeyValuePair> items = new();
    }
}
