//-------------------------------------------------------------------------------
//                                                                               
//    This code was automatically generated.                                     
//    Changes to this file may cause incorrect behavior and will be lost if      
//    the code is regenerated.                                                   
//                                                                               
//-------------------------------------------------------------------------------

using System.Linq;
using GameDatabase.Enums;
using GameDatabase.Serializable;
using GameDatabase.Model;

namespace GameDatabase.DataModel
{
	public partial class ShipToValue
	{
		partial void OnDataDeserialized(ShipToValueSerializable serializable, Database.Loader loader);

		public static ShipToValue Create(ShipToValueSerializable serializable, Database.Loader loader)
		{
			return new ShipToValue(serializable, loader);
		}

		private ShipToValue(ShipToValueSerializable serializable, Database.Loader loader)
		{
			Ship = loader.GetShip(new ItemId<Ship>(serializable.Ship));
			Value = UnityEngine.Mathf.Clamp(serializable.Value, 0, 2147483647);

			OnDataDeserialized(serializable, loader);
		}

		public Ship Ship { get; private set; }
		public int Value { get; private set; }

		public static ShipToValue DefaultValue { get; private set; }
	}
}
