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
	public partial class WeaponSlots 
	{
		partial void OnDataDeserialized(WeaponSlotsSerializable serializable, Database.Loader loader);

		public static WeaponSlots Create(WeaponSlotsSerializable serializable, Database.Loader loader)
		{
			return serializable == null ? DefaultValue : new WeaponSlots(serializable, loader);
		}

		private WeaponSlots(WeaponSlotsSerializable serializable, Database.Loader loader)
		{
			Slots = new ImmutableCollection<WeaponSlot>(serializable.Slots?.Select(item => WeaponSlot.Create(item, loader)));
			DefaultSlotName = serializable.DefaultSlotName;
			DefaultSlotIcon = new SpriteId(serializable.DefaultSlotIcon, SpriteId.Type.GuiIcon);

			OnDataDeserialized(serializable, loader);
		}

		public ImmutableCollection<WeaponSlot> Slots { get; private set; }
		public string DefaultSlotName { get; private set; }
		public SpriteId DefaultSlotIcon { get; private set; }

		public static WeaponSlots DefaultValue { get; private set; }
	}
}
