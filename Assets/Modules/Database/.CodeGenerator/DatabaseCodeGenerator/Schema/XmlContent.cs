using System.Collections.Generic;
using System.Xml.Serialization;

namespace DatabaseCodeGenerator.Schema
{
    [XmlRoot("schema")]
    public class XmlVersionList
    {
        [XmlElement("version")]
        public List<XmlVersionInfo> members = new List<XmlVersionInfo>();
    }

    public class XmlVersionInfo
    {
        [XmlAttribute]
        public string name = string.Empty;
        [XmlAttribute]
        public string major = string.Empty;
        [XmlAttribute]
        public string minor = string.Empty;
    }

    [XmlRoot("data")]
    public class XmlTypeInfo
    {
        [XmlAttribute]
        public string type = string.Empty;
        [XmlAttribute]
        public string name = string.Empty;
        [XmlText]
        public string value = string.Empty;
    }

    public class XmlEnumValue
    {
        [XmlAttribute]
        public string name = string.Empty;
		[XmlAttribute]
        public string value = string.Empty;
		[XmlText]
		public string tooltip = string.Empty;
	}

	[XmlRoot("data")]
    public class XmlEnumItem
    {
        [XmlAttribute]
        public string name = string.Empty;
        [XmlElement("item")]
        public List<XmlEnumValue> items = new List<XmlEnumValue>();
    }

    [XmlRoot("data")]
    public class XmlExpressionItem
    {
        [XmlAttribute]
        public string name = string.Empty;
        [XmlAttribute]
        public string result = string.Empty;
        [XmlAttribute]
        public string typeid = string.Empty;
        [XmlElement("param")]
        public List<XmlExpressionParam> items = new List<XmlExpressionParam>();
    }

    public class XmlExpressionParam
    {
        [XmlAttribute]
        public string name = string.Empty;
        [XmlAttribute]
        public string type = string.Empty;
        [XmlAttribute]
        public string typeid = string.Empty;
    }

    public class XmlClassMember
    {
        [XmlAttribute]
        public string name = string.Empty;
        [XmlAttribute]
        public string type = string.Empty;
        [XmlAttribute]
        public string typeid = string.Empty;
        [XmlAttribute]
        public string minvalue = string.Empty;
        [XmlAttribute]
        public string maxvalue = string.Empty;
        [XmlAttribute]
        public string @default = string.Empty;
        [XmlAttribute]
        public string alias = string.Empty;
        [XmlAttribute("case")]
        public string caseValue = string.Empty;
        [XmlAttribute]
        public string options = string.Empty;
        [XmlAttribute]
        public string arguments = string.Empty;
		[XmlText]
		public string tooltip = string.Empty;
	}

	[XmlRoot("data")]
    public class XmlClassItem
    {
        [XmlAttribute]
        public string name = string.Empty;
        [XmlAttribute]
        public string alias = string.Empty;
        [XmlAttribute]
        public string typeid = string.Empty;
        [XmlAttribute("switch")]
        public string switchEnum = string.Empty;
        [XmlAttribute("options")]
        public string options = string.Empty;
        [XmlElement("member")]
        public List<XmlClassMember> members = new List<XmlClassMember>();
    }
}
