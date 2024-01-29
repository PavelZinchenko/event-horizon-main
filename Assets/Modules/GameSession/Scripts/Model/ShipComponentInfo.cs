using GameDatabase;
using GameDatabase.Enums;
using GameDatabase.Model;
using GameDatabase.DataModel;
using Constructor;

namespace Session.Model
{
	public readonly partial struct ShipComponentInfo
	{
		public ShipComponentInfo(IntegratedComponent component)
		{
			_id = component.Info.Data.Id.Value;
			_quality = (sbyte)component.Info.ModificationQuality;
			_modification = component.Info.ModificationType.Id.Value;
			_upgradeLevel = 0;//component.Info.Level;
			_x = (short)component.X;
			_y = (short)component.Y;
			_barrelId = (sbyte)component.BarrelId;
			_keyBinding = (sbyte)component.KeyBinding;
			_behaviour = (sbyte)component.Behaviour;
			_locked = component.Locked;
		}

		public ShipComponentInfo(
			int id,
			int quality,
			int modification,
			int upgradeLevel, 
			int x, 
			int y,
			int barrelId,
			int keyBinding,
			int behaviour,
			bool locked)
		{
			_id = id;
			_quality = (sbyte)quality;
			_modification = (sbyte)modification;
			_upgradeLevel = (sbyte)upgradeLevel;
			_x = (short)x;
			_y = (short)y;
			_barrelId = (sbyte)barrelId;
			_keyBinding = (sbyte)keyBinding;
			_behaviour = (sbyte)behaviour;
			_locked = locked;
		}

		public IntegratedComponent ToIntegratedComponent(IDatabase database)
		{
			var component = database.GetComponent(new ItemId<Component>(_id));
			if (component == null)
			{
				UnityEngine.Debug.LogException(new System.ArgumentException("Unknown component - " + _id));
				return null;
			}

			var info = new ComponentInfo(component, database.GetComponentMod(new(_modification)), (ModificationQuality)_quality, _upgradeLevel);			
			return new IntegratedComponent(info, _x, _y, _barrelId, _keyBinding, _behaviour, _locked);
		}
	}
}
