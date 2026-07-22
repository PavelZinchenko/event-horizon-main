using DatabaseCodeGenerator.Schema;

namespace DatabaseCodeGenerator.MigrationCode.Templates
{
    public enum ObjectType
    {
        Struct,
        Class,
        Configuration,
    }

    public partial class SerializableItemTemplate
    {
        public SerializableItemTemplate(DatabaseSchema schema) 
        {
            Schema = schema; 
        }

        protected DatabaseSchema Schema { get; }
    }

    public partial class EnumTemplate
    {
        public EnumTemplate(XmlEnumItem enumData, DatabaseSchema schema) 
        {
            EnumData = enumData; 
            Schema = schema; 
        }

        protected XmlEnumItem EnumData { get; }
        protected DatabaseSchema Schema { get; }
    }

    public partial class SerializableTemplate
    {
        public SerializableTemplate(XmlClassItem objectData, DatabaseSchema schema, ObjectType type, Builder.Context context)
        {
            ObjectData = objectData; 
            Schema = schema; ObjectType = type;
            ObjectType = type;
            Context = context; 
        }

        protected XmlClassItem ObjectData { get; }
        protected DatabaseSchema Schema { get; }
        protected ObjectType ObjectType { get; }
        protected Builder.Context Context { get; }
    }

    public partial class DatabaseContentTemplate
    {
        public DatabaseContentTemplate(DatabaseSchema schema, Builder.Context context) 
        {
            Schema = schema; 
            Context = context; 
        }

        protected DatabaseSchema Schema { get; }
        protected Builder.Context Context { get; }
    }

    public partial class DatabaseUpgraderTemplate
    {
        public DatabaseUpgraderTemplate(VersionList versionList, Builder.Context context)
        {
            VersionList = versionList;
            Context = context;
        }

        protected VersionList VersionList { get; }
        protected Builder.Context Context { get; }
    }
}
