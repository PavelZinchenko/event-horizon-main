using DatabaseCodeGenerator.Schema;

namespace DatabaseCodeGenerator.GameCode.Templates
{
    public enum ObjectType
    {
        Struct,
        Class,
        Configuration,
    }

    public partial class EnumTemplate
    {
        public EnumTemplate(XmlEnumItem enumData) { EnumData = enumData; }
        protected XmlEnumItem EnumData { get; }
    }

    public partial class ExpressionTemplate
    {
        public ExpressionTemplate(XmlExpressionItem expressionData, DatabaseSchema schema) { ExpressionData = expressionData; Schema = schema; }
        protected XmlExpressionItem ExpressionData { get; }
        protected DatabaseSchema Schema { get; }
    }

    public partial class ObjectTemplate
    {
        public ObjectTemplate(XmlClassItem objectData, DatabaseSchema schema, ObjectType type) { ObjectData = objectData; Schema = schema; ObjectType = type; }
        protected XmlClassItem ObjectData { get; }
        protected DatabaseSchema Schema { get; }
        protected ObjectType ObjectType { get; }
    }

    public partial class LegacyObjectTemplate
    {
        public LegacyObjectTemplate(XmlClassItem objectData, DatabaseSchema schema, ObjectType type) { ObjectData = objectData; Schema = schema; ObjectType = type; }
        protected XmlClassItem ObjectData { get; }
        protected DatabaseSchema Schema { get; }
        protected ObjectType ObjectType { get; }
    }

    public partial class MutableObjectTemplate
    {
        public MutableObjectTemplate(XmlClassItem objectData, DatabaseSchema schema, ObjectType type) { ObjectData = objectData; Schema = schema; ObjectType = type; }
        protected XmlClassItem ObjectData { get; }
        protected DatabaseSchema Schema { get; }
        protected ObjectType ObjectType { get; }
    }

    public partial class SerializableTemplate
    {
        public SerializableTemplate(XmlClassItem objectData, DatabaseSchema schema, ObjectType type) { ObjectData = objectData; Schema = schema; ObjectType = type; }
        protected XmlClassItem ObjectData { get; }
        protected DatabaseSchema Schema { get; }
        protected ObjectType ObjectType { get; }
    }

    public partial class DatabaseContentTemplate
    {
        public DatabaseContentTemplate(DatabaseSchema schema) { Schema = schema; }
        protected DatabaseSchema Schema { get; }
    }

    public partial class DatabaseTemplate
    {
        public DatabaseTemplate(DatabaseSchema schema) { Schema = schema; }
        protected DatabaseSchema Schema { get; }
    }
}
