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
	public partial class ComponentRestrictions
	{
		partial void OnDataDeserialized(ComponentRestrictionsSerializable serializable, Database database);
		partial void OnDataSerialized(ref ComponentRestrictionsSerializable serializable);

		public static ComponentRestrictions Create(ComponentRestrictionsSerializable serializable, Database database)
		{
			if (serializable == null) return DefaultValue;
			return new ComponentRestrictions(serializable, database);
		}

		public ComponentRestrictions() {}

		private ComponentRestrictions(ComponentRestrictionsSerializable serializable, Database database)
		{
			ShipSizes = serializable.ShipSizes?.Select(item => new ValueWrapper<SizeClass> { Value = item }).ToArray();
			NotForOrganicShips = serializable.NotForOrganicShips;
			NotForMechanicShips = serializable.NotForMechanicShips;
			MaxComponentAmount = new NumericValue<int>(serializable.MaxComponentAmount, 0, 2147483647);
			ComponentGroupTag = database.GetComponentGroupTagId(serializable.ComponentGroupTag);
			OnDataDeserialized(serializable, database);
		}

		public ComponentRestrictionsSerializable Serialize()
		{
			var serializable = new ComponentRestrictionsSerializable();
			if (ShipSizes == null || ShipSizes.Length == 0)
			    serializable.ShipSizes = null;
			else
			    serializable.ShipSizes = ShipSizes.Select(item => item.Value).ToArray();
			serializable.NotForOrganicShips = NotForOrganicShips;
			serializable.NotForMechanicShips = NotForMechanicShips;
			serializable.MaxComponentAmount = MaxComponentAmount.Value;
			serializable.ComponentGroupTag = ComponentGroupTag.Value;
			OnDataSerialized(ref serializable);
			return serializable;
		}

		public ValueWrapper<SizeClass>[] ShipSizes;
		public bool NotForOrganicShips;
		public bool NotForMechanicShips;
		public NumericValue<int> MaxComponentAmount = new NumericValue<int>(0, 0, 2147483647);
		public ItemId<ComponentGroupTag> ComponentGroupTag = ItemId<ComponentGroupTag>.Empty;

		public static ComponentRestrictions DefaultValue { get; private set; }
	}
}
