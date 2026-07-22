using CodeGenerator.Schema;

namespace CodeGenerator.GameCode.Templates
{
    public partial class ObjectTemplate
    {
        public ObjectTemplate(XmlClassItem objectData, SchemaVersionInfo context) { ObjectData = objectData; Context = context; }
        protected XmlClassItem ObjectData { get; }
        protected SchemaVersionInfo Context { get; }
    }

	public partial class StructTemplate
	{
		public StructTemplate(XmlClassItem objectData, SchemaVersionInfo context) { ObjectData = objectData; Context = context; }
		protected XmlClassItem ObjectData { get; }
		protected SchemaVersionInfo Context { get; }
	}

	public partial class SessionLoaderTemplate
	{
		public SessionLoaderTemplate(VersionList versionList, BuilderContext context)
		{
			VersionList = versionList;
			Context = context;
		}

		protected VersionList VersionList { get; }
		protected BuilderContext Context { get; }
	}
}
