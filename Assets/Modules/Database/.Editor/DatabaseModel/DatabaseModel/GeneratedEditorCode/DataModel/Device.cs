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
	public partial class Device
	{
		partial void OnDataDeserialized(DeviceSerializable serializable, Database database);
		partial void OnDataSerialized(ref DeviceSerializable serializable);

		public static Device Create(DeviceSerializable serializable, Database database)
		{
			if (serializable == null) return DefaultValue;
			return new Device(serializable, database);
		}

		private Device(DeviceSerializable serializable, Database database)
		{
			try
			{
				Id = new ItemId<Device>(serializable.Id, serializable.FileName);
				DeviceClass = serializable.DeviceClass;
				EnergyConsumption = new NumericValue<float>(serializable.EnergyConsumption, 0f, 1E+09f);
				PassiveEnergyConsumption = new NumericValue<float>(serializable.PassiveEnergyConsumption, 0f, 1E+09f);
				ScaleEnergyWithShipSize = serializable.ScaleEnergyWithShipSize;
				Power = new NumericValue<float>(serializable.Power, -3.402823E+38f, 3.402823E+38f);
				Range = new NumericValue<float>(serializable.Range, 0f, 1000f);
				Size = new NumericValue<float>(serializable.Size, 0f, 1000f);
				Cooldown = new NumericValue<float>(serializable.Cooldown, 0f, 1000f);
				Lifetime = new NumericValue<float>(serializable.Lifetime, 0f, 1000f);
				Offset = serializable.Offset;
				ActivationType = serializable.ActivationType;
				Color = Helpers.ColorFromString(serializable.Color);
				Sound = serializable.Sound;
				EffectPrefab = serializable.EffectPrefab;
				VisualEffect = database.GetVisualEffectId(serializable.VisualEffect);
				ObjectPrefab = serializable.ObjectPrefab;
				Prefab = database.GetGameObjectPrefabId(serializable.Prefab);
				AmmunitionId = database.GetAmmunitionId(serializable.AmmunitionId);
				ControlButtonIcon = serializable.ControlButtonIcon;
			}
			catch (DatabaseException e)
			{
				throw new DatabaseException(this.GetType() + ": deserialization failed. " + serializable.FileName + " (" + serializable.Id + ")", e);
			}
			OnDataDeserialized(serializable, database);
		}

		public void Save(DeviceSerializable serializable)
		{
			serializable.DeviceClass = DeviceClass;
			serializable.EnergyConsumption = EnergyConsumption.Value;
			serializable.PassiveEnergyConsumption = PassiveEnergyConsumption.Value;
			serializable.ScaleEnergyWithShipSize = ScaleEnergyWithShipSize;
			serializable.Power = Power.Value;
			serializable.Range = Range.Value;
			serializable.Size = Size.Value;
			serializable.Cooldown = Cooldown.Value;
			serializable.Lifetime = Lifetime.Value;
			serializable.Offset = Offset;
			serializable.ActivationType = ActivationType;
			serializable.Color = Helpers.ColorToString(Color);
			serializable.Sound = Sound;
			serializable.EffectPrefab = EffectPrefab;
			serializable.VisualEffect = VisualEffect.Value;
			serializable.ObjectPrefab = ObjectPrefab;
			serializable.Prefab = Prefab.Value;
			serializable.AmmunitionId = AmmunitionId.Value;
			serializable.ControlButtonIcon = ControlButtonIcon;
			OnDataSerialized(ref serializable);
		}

		public readonly ItemId<Device> Id;

		public DeviceClass DeviceClass;
		public NumericValue<float> EnergyConsumption = new NumericValue<float>(0, 0f, 1E+09f);
		public NumericValue<float> PassiveEnergyConsumption = new NumericValue<float>(0, 0f, 1E+09f);
		public bool ScaleEnergyWithShipSize;
		public NumericValue<float> Power = new NumericValue<float>(0, -3.402823E+38f, 3.402823E+38f);
		public NumericValue<float> Range = new NumericValue<float>(0, 0f, 1000f);
		public NumericValue<float> Size = new NumericValue<float>(0, 0f, 1000f);
		public NumericValue<float> Cooldown = new NumericValue<float>(0, 0f, 1000f);
		public NumericValue<float> Lifetime = new NumericValue<float>(0, 0f, 1000f);
		public Vector2 Offset;
		public ActivationType ActivationType;
		public System.Drawing.Color Color;
		public string Sound;
		public string EffectPrefab;
		public ItemId<VisualEffect> VisualEffect = ItemId<VisualEffect>.Empty;
		public string ObjectPrefab;
		public ItemId<GameObjectPrefab> Prefab = ItemId<GameObjectPrefab>.Empty;
		public ItemId<Ammunition> AmmunitionId = ItemId<Ammunition>.Empty;
		public string ControlButtonIcon;

		public static Device DefaultValue { get; private set; }
	}
}
