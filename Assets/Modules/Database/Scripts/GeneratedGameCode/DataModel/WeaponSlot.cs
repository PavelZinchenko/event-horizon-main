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
	public partial class WeaponSlot 
	{
		partial void OnDataDeserialized(WeaponSlotSerializable serializable, Database.Loader loader);

		public static WeaponSlot Create(WeaponSlotSerializable serializable, Database.Loader loader)
		{
			return serializable == null ? DefaultValue : new WeaponSlot(serializable, loader);
		}

		private WeaponSlot(WeaponSlotSerializable serializable, Database.Loader loader)
		{
			Letter = string.IsNullOrEmpty(serializable.Letter) ? default : serializable.Letter[0];
			Name = serializable.Name;
			Icon = new SpriteId(serializable.Icon, SpriteId.Type.GuiIcon);

			OnDataDeserialized(serializable, loader);
		}

		public char Letter { get; private set; }
		public string Name { get; private set; }
		public SpriteId Icon { get; private set; }

		public static WeaponSlot DefaultValue { get; private set; }= new(new(), null);
	}
}
