//-------------------------------------------------------------------------------
//                                                                               
//    This code was automatically generated.                                     
//    Changes to this file may cause incorrect behavior and will be lost if      
//    the code is regenerated.                                                   
//                                                                               
//-------------------------------------------------------------------------------

using System;
using GameDatabase.Model;
using DatabaseMigration.v1.Enums;

namespace DatabaseMigration.v1.Serializable
{
	[Serializable]
	public class DeviceSerializable : SerializableItem
	{
		public DeviceSerializable()
		{
			ItemType = ItemType.Device;
		}

		public DeviceClass DeviceClass;
		public float EnergyConsumption;
		public float PassiveEnergyConsumption;
		public float Power;
		public float Range;
		public float Size;
		public float Cooldown;
		public float Lifetime;
		public UnityEngine.Vector2 Offset;
		public ActivationType ActivationType;
		public string Color;
		public string Sound;
		public string EffectPrefab;
		public string ObjectPrefab;
		public string ControlButtonIcon;
	}
}
