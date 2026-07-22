namespace CodeGenerator.Schema
{
    public static class Helpers
    {
		public static bool IsCollectionType(string type)
		{
			return type == Constants.TypeSet || type == Constants.TypeMap || type == Constants.TypeList ||
				type == Constants.TypeInventory || type == Constants.TypeBitset;
		}

		public static bool AreEqual(XmlClassItem first, XmlClassItem second)
		{
			if (first == second) return true;
			if (first == null || second == null) return false;
			if (first.name != second.name) return false;
			if (first.type != second.type) return false;
			if (first.members.Count != second.members.Count) return false;

			for (int i = 0; i < first.members.Count; ++i)
				if (!AreEqual(first.members[i], second.members[i])) return false;

			return true;
		}

		public static bool AreEqual(XmlClassMember first, XmlClassMember second)
		{
			if (first.name != second.name) return false;
			if (first.type != second.type) return false;
			if (first.key != second.key) return false;
			if (first.value != second.value) return false;
			if (first.options != second.options) return false;
			if (first.encoding != second.encoding) return false;
			if (first.@default != second.@default) return false;

			return true;
		}
	}
}
