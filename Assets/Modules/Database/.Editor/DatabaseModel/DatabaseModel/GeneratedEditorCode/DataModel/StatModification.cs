//-------------------------------------------------------------------------------
//                                                                               
//    This code was automatically generated.                                     
//    Changes to this file may cause incorrect behavior and will be lost if      
//    the code is regenerated.                                                   
//                                                                               
//-------------------------------------------------------------------------------

using System.Linq;
using EditorDatabase.Enums;
using EditorDatabase.Serializable;
using EditorDatabase.Model;

namespace EditorDatabase.DataModel
{
	public partial class StatModification
	{
		partial void OnDataDeserialized(StatModificationSerializable serializable, Database database);
		partial void OnDataSerialized(ref StatModificationSerializable serializable);

		public static StatModification Create(StatModificationSerializable serializable, Database database)
		{
			if (serializable == null) return DefaultValue;
			return new StatModification(serializable, database);
		}

		public StatModification() {}

		private StatModification(StatModificationSerializable serializable, Database database)
		{
			Type = serializable.Type;
			Gray3 = new NumericValue<float>(serializable.Gray3, -3.402823E+38f, 3.402823E+38f);
			Gray2 = new NumericValue<float>(serializable.Gray2, -3.402823E+38f, 3.402823E+38f);
			Gray1 = new NumericValue<float>(serializable.Gray1, -3.402823E+38f, 3.402823E+38f);
			Green = new NumericValue<float>(serializable.Green, -3.402823E+38f, 3.402823E+38f);
			Purple = new NumericValue<float>(serializable.Purple, -3.402823E+38f, 3.402823E+38f);
			Gold = new NumericValue<float>(serializable.Gold, -3.402823E+38f, 3.402823E+38f);
			OnDataDeserialized(serializable, database);
		}

		public StatModificationSerializable Serialize()
		{
			var serializable = new StatModificationSerializable();
			serializable.Type = Type;
			serializable.Gray3 = Gray3.Value;
			serializable.Gray2 = Gray2.Value;
			serializable.Gray1 = Gray1.Value;
			serializable.Green = Green.Value;
			serializable.Purple = Purple.Value;
			serializable.Gold = Gold.Value;
			OnDataSerialized(ref serializable);
			return serializable;
		}

		public StatModificationType Type;
		public NumericValue<float> Gray3 = new NumericValue<float>(0, -3.402823E+38f, 3.402823E+38f);
		public NumericValue<float> Gray2 = new NumericValue<float>(0, -3.402823E+38f, 3.402823E+38f);
		public NumericValue<float> Gray1 = new NumericValue<float>(0, -3.402823E+38f, 3.402823E+38f);
		public NumericValue<float> Green = new NumericValue<float>(0, -3.402823E+38f, 3.402823E+38f);
		public NumericValue<float> Purple = new NumericValue<float>(0, -3.402823E+38f, 3.402823E+38f);
		public NumericValue<float> Gold = new NumericValue<float>(0, -3.402823E+38f, 3.402823E+38f);

		public static StatModification DefaultValue { get; private set; }
	}
}
