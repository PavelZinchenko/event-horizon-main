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
	public partial class ShipToValue
	{
		partial void OnDataDeserialized(ShipToValueSerializable serializable, Database database);
		partial void OnDataSerialized(ref ShipToValueSerializable serializable);

		public static ShipToValue Create(ShipToValueSerializable serializable, Database database)
		{
			if (serializable == null) return DefaultValue;
			return new ShipToValue(serializable, database);
		}

		public ShipToValue() {}

		private ShipToValue(ShipToValueSerializable serializable, Database database)
		{
			Ship = database.GetShipId(serializable.Ship);
			Value = new NumericValue<int>(serializable.Value, 0, 2147483647);
			OnDataDeserialized(serializable, database);
		}

		public ShipToValueSerializable Serialize()
		{
			var serializable = new ShipToValueSerializable();
			serializable.Ship = Ship.Value;
			serializable.Value = Value.Value;
			OnDataSerialized(ref serializable);
			return serializable;
		}

		public ItemId<Ship> Ship = ItemId<Ship>.Empty;
		public NumericValue<int> Value = new NumericValue<int>(0, 0, 2147483647);

		public static ShipToValue DefaultValue { get; private set; }
	}
}
