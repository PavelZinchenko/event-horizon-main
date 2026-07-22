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
	public partial class WeaponSlot
	{
		partial void OnDataDeserialized(WeaponSlotSerializable serializable, Database database);
		partial void OnDataSerialized(ref WeaponSlotSerializable serializable);

		public static WeaponSlot Create(WeaponSlotSerializable serializable, Database database)
		{
			if (serializable == null) return DefaultValue;
			return new WeaponSlot(serializable, database);
		}

		public WeaponSlot() {}

		private WeaponSlot(WeaponSlotSerializable serializable, Database database)
		{
			Letter = string.IsNullOrEmpty(serializable.Letter) ? default : serializable.Letter[0];
			Name = serializable.Name;
			Icon = serializable.Icon;
			OnDataDeserialized(serializable, database);
		}

		public WeaponSlotSerializable Serialize()
		{
			var serializable = new WeaponSlotSerializable();
			serializable.Letter = Letter == default ? string.Empty : Letter.ToString();
			serializable.Name = Name;
			serializable.Icon = Icon;
			OnDataSerialized(ref serializable);
			return serializable;
		}

		public char Letter;
		public string Name;
		public string Icon;

		public static WeaponSlot DefaultValue { get; private set; }
	}
}
