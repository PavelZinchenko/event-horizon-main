using System.Collections.Generic;
using System.Xml.Serialization;

namespace CodeGenerator.Schema
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
	}

    public class XmlClassMember
    {
        [XmlAttribute]
        public string name = string.Empty;
        [XmlAttribute]
        public string type = string.Empty;
		[XmlAttribute]
		public string key = string.Empty;
		[XmlAttribute]
		public string value = string.Empty;
		[XmlAttribute]
		public string options = string.Empty;
		[XmlAttribute]
		public string encoding = string.Empty;
		[XmlAttribute]
		public string @default = string.Empty;
	}

	[XmlRoot("data")]
    public class XmlClassItem
    {
		[XmlAttribute]
		public string type = string.Empty;
		[XmlAttribute]
		public string name = string.Empty;
		[XmlElement("member")]
        public List<XmlClassMember> members = new List<XmlClassMember>();
	}
}
