using CodeGenerator.Schema;

namespace CodeGenerator.GameCode
{
    public static class Utils
    {
		public static string SerializableClassName(string name) => name + "Serializable";
		public static string InterfaceName(string name) => "I" + name;
		public static string PropertyName(string name) => char.ToUpper(name[0]) + name.Substring(1);
		public static string LocalVariableName(string name) => char.ToLower(name[0]) + name.Substring(1);
		public static string PrivateMemberName(string name) => "_" + LocalVariableName(name);

		public static string GetEncodingType(string encoding)
		{
			if (string.IsNullOrEmpty(encoding))
				encoding = Constants.DefaultEncoding;

			if (encoding == Constants.EncodingPlain)
				return EncodingPlain;
			if (encoding == Constants.EncodingElias)
				return EncodingElias;

			throw new System.InvalidOperationException("Invalid encoding type - " + encoding);
		}

		public const string EncryptedIntType = "ObscuredInt";
		public const string EncryptedLongType = "ObscuredLong";

		public const string EncodingPlain = "EncodingType.Plain";
		public const string EncodingElias = "EncodingType.EliasGamma";

		public const string CallbackInterface = "IDataChangedCallback";
		public const string CallbackMethod = "OnDataChanged";

		public const string RootDataClass = "SaveGameData";

		public const string WriterClass = "SessionDataWriter";
		public const string ReaderClass = "SessionDataReader";

		public const string ListType = "ObservableList";
		public const string MapType = "ObservableMap";
		public const string SetType = "ObservableSet";
		public const string BitsetType = "ObservableBitset";
		public const string InventoryType = "ObservableInventory";

		public const string RootNamespace = "Session";
		public const string ClassesNamespace = "Model";
		public const string UtilsNamespace = "Utils";
    }
}
