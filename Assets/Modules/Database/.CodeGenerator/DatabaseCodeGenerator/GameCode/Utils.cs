using DatabaseCodeGenerator.Schema;
using System.Xml.Linq;

namespace DatabaseCodeGenerator.GameCode
{
    public static class Utils
    {
        public static string SerializableClassName(string name) => name + "Serializable";
        public static string DataClassName(XmlClassItem data) => string.IsNullOrEmpty(data.alias) ? data.name : data.alias;
        public static string GetterName(string name) => "Get" + char.ToUpper(name[0]) + name.Substring(1);
        public static string ObjectGetterName(string name) => "Get" + name;
        public static string ObjectSetterName(string name) => "Add" + name;
        public static string ObjectListPropertyName(string name) => name + "List";
        public static string ExpressionClassName(XmlExpressionItem data) => data.name;
        public static string DelegateName(string name) => name + "Delegate";
        public static string PrivateMemberName(string name) =>  "_" + char.ToLower(name[0]) + name.Substring(1);

        public const string FactoryMethodName = "Create";
        public const string DatabaseClassName = "Database";

        public const string ImageType = "IImageData";
        public const string AudioClipType = "IAudioClipData";
        public const string EmptyImage = "ImageData.Empty";
        public const string EmptyAudioClip = "AudioClipData.Empty";
        public const string VectorType = "UnityEngine.Vector2";

        public const string VariantType = "Variant";

        public const string RootNamespace = "GameDatabase";
        public const string TypesNamespace = "Model";
        public const string EnumsNamespace = "Enums";
        public const string SerializableNamespace = "Serializable";
        public const string StorageNamespace = "Storage";
        public const string ClassesNamespace = "DataModel";
        public const string ExpressionsNamespace = "Expressions";
    }
}
