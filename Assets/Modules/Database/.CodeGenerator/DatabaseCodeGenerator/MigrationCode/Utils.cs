using DatabaseCodeGenerator.Schema;

namespace DatabaseCodeGenerator.MigrationCode
{
    public static class Utils
    {
        public static string SerializableClassName(string name) { return name + "Serializable"; }
        public static string DataClassName(XmlClassItem data) { return string.IsNullOrEmpty(data.alias) ? data.name : data.alias; }
        public static string ObjectGetterName(string name) { return "Get" + name; }
        public static string ObjectSetterName(string name) { return "Add" + name; }
        public static string ObjectListPropertyName(string name) { return name + "List"; }
        public static string PrivateMemberName(string name) => "_" + char.ToLower(name[0]) + name.Substring(1);

        public const string FactoryMethodName = "Create";
        public const string DatabaseUpgraderClassName = "DatabaseUpgrader";

        public const string DatabaseSettings = "DatabaseSettings";
        public const string VersionMajor = "DatabaseVersion";
        public const string VersionMinor = "DatabaseVersionMinor";

        public const string ImageType = "IImageData";
        public const string AudioClipType = "IAudioClipData";
        public const string EmptyImage = "ImageData.Empty";
        public const string EmptyAudioClip = "AudioClipData.Empty";

        public const string RootNamespace = "DatabaseMigration";
        public const string TypesNamespace = "Model";
        public const string EnumsNamespace = "Enums";
        public const string SerializableNamespace = "Serializable";
        public const string StorageNamespace = "Storage";
        public const string ClassesNamespace = "DataModel";
    }
}
