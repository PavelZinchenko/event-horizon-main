


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
	public partial class ShipBuildPerks 
	{
		partial void OnDataDeserialized(ShipBuildPerksSerializable serializable, Database.Loader loader);

		public static ShipBuildPerks Create(ShipBuildPerksSerializable serializable, Database.Loader loader)
		{
			return serializable == null ? DefaultValue : new ShipBuildPerks(serializable, loader);
		}

		private ShipBuildPerks(ShipBuildPerksSerializable serializable, Database.Loader loader)
		{
			Perk1 = serializable.Perk1;
			Perk2 = serializable.Perk2;
			Perk3 = serializable.Perk3;

			OnDataDeserialized(serializable, loader);
		}

		public ShipPerkType Perk1 { get; private set; }
		public ShipPerkType Perk2 { get; private set; }
		public ShipPerkType Perk3 { get; private set; }

		public static ShipBuildPerks DefaultValue { get; private set; }= new(new(), null);
	}
}
