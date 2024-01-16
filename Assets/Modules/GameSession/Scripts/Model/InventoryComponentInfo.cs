using GameDatabase;
using GameDatabase.Enums;
using GameDatabase.Model;
using GameDatabase.DataModel;
using Constructor;

namespace Session.Model
{
	public readonly partial struct InventoryComponentInfo
	{
		public InventoryComponentInfo(
			uint id,
			byte modification,
			byte quality,
			byte upgradeLevel)
		{
			_id = id;
			_modification = modification;
			_quality = quality;
			_upgradeLevel = upgradeLevel;
		}

		public InventoryComponentInfo(ComponentInfo componentInfo)
		{
			_id = (uint)componentInfo.Data.Id.Value;
			_modification = (byte)componentInfo.ModificationType;
			_quality = (byte)componentInfo.ModificationQuality;
			_upgradeLevel = (byte)componentInfo.Level;
		}

		public ComponentInfo ToComponentInfo(IDatabase database)
		{
			var component = database.GetComponent(new ItemId<Component>((int)_id));
			if (component == null)
			{
				UnityEngine.Debug.LogException(new System.ArgumentException("Unknown component - " + _id));
				return ComponentInfo.Empty;
			}

			return new ComponentInfo(component, (ComponentModType)_modification, (ModificationQuality)_quality, _upgradeLevel);			
		}
	}
}
