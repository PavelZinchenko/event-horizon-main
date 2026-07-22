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
	public partial class ShipBuildPerks
	{
		partial void OnDataDeserialized(ShipBuildPerksSerializable serializable, Database database);
		partial void OnDataSerialized(ref ShipBuildPerksSerializable serializable);

		public static ShipBuildPerks Create(ShipBuildPerksSerializable serializable, Database database)
		{
			if (serializable == null) return DefaultValue;
			return new ShipBuildPerks(serializable, database);
		}

		public ShipBuildPerks() {}

		private ShipBuildPerks(ShipBuildPerksSerializable serializable, Database database)
		{
			Perk1 = serializable.Perk1;
			Perk2 = serializable.Perk2;
			Perk3 = serializable.Perk3;
			OnDataDeserialized(serializable, database);
		}

		public ShipBuildPerksSerializable Serialize()
		{
			var serializable = new ShipBuildPerksSerializable();
			serializable.Perk1 = Perk1;
			serializable.Perk2 = Perk2;
			serializable.Perk3 = Perk3;
			OnDataSerialized(ref serializable);
			return serializable;
		}

		public ShipPerkType Perk1;
		public ShipPerkType Perk2;
		public ShipPerkType Perk3;

		public static ShipBuildPerks DefaultValue { get; private set; }
	}
}
