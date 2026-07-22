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
	public partial class WeaponSlots
	{
		partial void OnDataDeserialized(WeaponSlotsSerializable serializable, Database database);
		partial void OnDataSerialized(ref WeaponSlotsSerializable serializable);

		public static WeaponSlots Create(WeaponSlotsSerializable serializable, Database database)
		{
			if (serializable == null) return DefaultValue;
			return new WeaponSlots(serializable, database);
		}

		private WeaponSlots(WeaponSlotsSerializable serializable, Database database)
		{
			Slots = serializable.Slots?.Select(item => WeaponSlot.Create(item, database)).ToArray();
			DefaultSlotName = serializable.DefaultSlotName;
			DefaultSlotIcon = serializable.DefaultSlotIcon;
			OnDataDeserialized(serializable, database);
		}

		public void Save(WeaponSlotsSerializable serializable)
		{
			if (Slots == null || Slots.Length == 0)
			    serializable.Slots = null;
			else
			    serializable.Slots = Slots.Select(item => item.Serialize()).ToArray();
			serializable.DefaultSlotName = DefaultSlotName;
			serializable.DefaultSlotIcon = DefaultSlotIcon;
			OnDataSerialized(ref serializable);
		}

		public WeaponSlot[] Slots;
		public string DefaultSlotName;
		public string DefaultSlotIcon;

		public static WeaponSlots DefaultValue { get; private set; }
	}
}
