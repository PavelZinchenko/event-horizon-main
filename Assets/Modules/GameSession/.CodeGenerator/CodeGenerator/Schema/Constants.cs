namespace CodeGenerator.Schema
{
    public static class Constants
    {
		public const string TypeInt = "int";
		public const string TypeUint = "uint";
		public const string TypeShort = "short";
		public const string TypeUshort = "ushort";
		public const string TypeLong = "long";
		public const string TypeUlong = "ulong";
		public const string TypeSbyte = "sbyte";
		public const string TypeByte = "byte";
		public const string TypeFloat = "single";
        public const string TypeBool = "bool";
        public const string TypeString = "string";
        public const string TypeObject = "object";
		public const string TypeStruct = "struct";
		public const string TypeSet = "set";
		public const string TypeMap = "map";
		public const string TypeList = "list";
		public const string TypeInventory = "inventory";
		public const string TypeBitset = "bitset";

		public const string Timestamp = "timestamp";

		public const string OptionEncrypted = "encrypted";
		public const string OptionNotImportant = "notimportant";

		public const string EncodingPlain = "plain";
		public const string EncodingElias = "elias";
		public const string DefaultEncoding = EncodingElias;

		public static readonly char[] ValueSeparators = { ',','|',';',' ','\n','\r' };
	}
}
