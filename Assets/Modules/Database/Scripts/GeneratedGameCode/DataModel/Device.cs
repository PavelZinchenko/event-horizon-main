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
	public partial class Device
	{
		public static Device Create(DeviceSerializable serializable, Database.Loader loader)
		{
			return new Device(serializable, loader);
		}

		private Device(DeviceSerializable serializable, Database.Loader loader)
		{
			Id = new ItemId<Device>(serializable.Id);
			loader.AddDevice(serializable.Id, this);
			Stats = new DeviceStats(serializable, loader);
		}

		public readonly ItemId<Device> Id;
		public readonly DeviceStats Stats;

		public static Device DefaultValue { get; private set; }
	}

	public partial struct DeviceStats 
	{
		partial void OnDataDeserialized(DeviceSerializable serializable, Database.Loader loader);

		public DeviceStats(DeviceSerializable serializable, Database.Loader loader)
		{
			DeviceClass = serializable.DeviceClass;
			EnergyConsumption = UnityEngine.Mathf.Clamp(serializable.EnergyConsumption, 0f, 1E+09f);
			PassiveEnergyConsumption = UnityEngine.Mathf.Clamp(serializable.PassiveEnergyConsumption, 0f, 1E+09f);
			ScaleEnergyWithShipSize = serializable.ScaleEnergyWithShipSize;
			Power = UnityEngine.Mathf.Clamp(serializable.Power, -3.402823E+38f, 3.402823E+38f);
			Range = UnityEngine.Mathf.Clamp(serializable.Range, 0f, 1000f);
			Size = UnityEngine.Mathf.Clamp(serializable.Size, 0f, 1000f);
			Cooldown = UnityEngine.Mathf.Clamp(serializable.Cooldown, 0f, 1000f);
			Lifetime = UnityEngine.Mathf.Clamp(serializable.Lifetime, 0f, 1000f);
			Offset = serializable.Offset;
			ActivationType = serializable.ActivationType;
			Color = new ColorData(serializable.Color);
			Sound = new AudioClipId(serializable.Sound);
			EffectPrefab = new PrefabId(serializable.EffectPrefab, PrefabId.Type.Effect);
			VisualEffect = loader?.GetVisualEffect(new ItemId<VisualEffect>(serializable.VisualEffect)) ?? VisualEffect.DefaultValue;
			ObjectPrefab = new PrefabId(serializable.ObjectPrefab, PrefabId.Type.Object);
			Prefab = loader?.GetGameObjectPrefab(new ItemId<GameObjectPrefab>(serializable.Prefab)) ?? GameObjectPrefab.DefaultValue;
			AmmunitionId = loader?.GetAmmunition(new ItemId<Ammunition>(serializable.AmmunitionId)) ?? Ammunition.DefaultValue;
			ControlButtonIcon = new SpriteId(serializable.ControlButtonIcon, SpriteId.Type.ActionButton);

			OnDataDeserialized(serializable, loader);
		}

		public DeviceClass DeviceClass;
		public float EnergyConsumption;
		public float PassiveEnergyConsumption;
		public bool ScaleEnergyWithShipSize;
		public float Power;
		public float Range;
		public float Size;
		public float Cooldown;
		public float Lifetime;
		public UnityEngine.Vector2 Offset;
		public ActivationType ActivationType;
		public ColorData Color;
		public AudioClipId Sound;
		public PrefabId EffectPrefab;
		public VisualEffect VisualEffect;
		public PrefabId ObjectPrefab;
		public GameObjectPrefab Prefab;
		public Ammunition AmmunitionId;
		public SpriteId ControlButtonIcon;
	}
}
