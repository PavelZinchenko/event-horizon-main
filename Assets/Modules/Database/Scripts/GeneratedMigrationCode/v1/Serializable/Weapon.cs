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
	public class WeaponSerializable : SerializableItem
	{
		public WeaponSerializable()
		{
			ItemType = ItemType.Weapon;
		}

		public WeaponClass WeaponClass;
		public float FireRate;
		public float Spread;
		public int Magazine;
		public ActivationType ActivationType;
		public string ShotSound;
		public string ChargeSound;
		public string ShotEffectPrefab;
		public int VisualEffect;
		public float EffectSize;
		public string ControlButtonIcon;
	}
}
