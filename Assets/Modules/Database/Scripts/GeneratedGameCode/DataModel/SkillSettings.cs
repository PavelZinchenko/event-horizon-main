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
	public partial class SkillSettings
	{
		partial void OnDataDeserialized(SkillSettingsSerializable serializable, Database.Loader loader);

		public static SkillSettings Create(SkillSettingsSerializable serializable, Database.Loader loader)
		{
			return new SkillSettings(serializable, loader);
		}

		private SkillSettings(SkillSettingsSerializable serializable, Database.Loader loader)
		{
			BeatAllEnemiesFactionList = new ImmutableCollection<Faction>(serializable.BeatAllEnemiesFactionList?.Select(item => loader.GetFaction(new ItemId<Faction>(item), true)));
			DisableExceedTheLimits = serializable.DisableExceedTheLimits;
			FuelTankCapacity = UnityEngine.Mathf.Clamp(serializable.FuelTankCapacity, 0, 100);
			MapFlightRange = UnityEngine.Mathf.Clamp(serializable.MapFlightRange, 0, 100);
			MapFlightSpeed = UnityEngine.Mathf.Clamp(serializable.MapFlightSpeed, 0, 100);
			AttackBonus = UnityEngine.Mathf.Clamp(serializable.AttackBonus, 0, 100);
			DefenseBonus = UnityEngine.Mathf.Clamp(serializable.DefenseBonus, 0, 100);
			ExperienceBonus = UnityEngine.Mathf.Clamp(serializable.ExperienceBonus, 0, 100);
			ExplorationLootBonus = UnityEngine.Mathf.Clamp(serializable.ExplorationLootBonus, 0, 100);
			HeatDefenseBonus = UnityEngine.Mathf.Clamp(serializable.HeatDefenseBonus, 0, 100);
			KineticDefenseBonus = UnityEngine.Mathf.Clamp(serializable.KineticDefenseBonus, 0, 100);
			EnergyDefenseBonus = UnityEngine.Mathf.Clamp(serializable.EnergyDefenseBonus, 0, 100);
			MerchantPriceReduction = UnityEngine.Mathf.Clamp(serializable.MerchantPriceReduction, 0, 100);
			CraftPriceReduction = UnityEngine.Mathf.Clamp(serializable.CraftPriceReduction, 0, 100);
			CraftLevelReduction = UnityEngine.Mathf.Clamp(serializable.CraftLevelReduction, 0, 100);
			ShieldStrengthBonus = UnityEngine.Mathf.Clamp(serializable.ShieldStrengthBonus, 0, 100);
			ShieldRechargeBonus = UnityEngine.Mathf.Clamp(serializable.ShieldRechargeBonus, 0, 100);
			IncreasedLevelLimit = UnityEngine.Mathf.Clamp(serializable.IncreasedLevelLimit, 100, 1000);

			OnDataDeserialized(serializable, loader);
		}

		public ImmutableCollection<Faction> BeatAllEnemiesFactionList { get; private set; }
		public bool DisableExceedTheLimits { get; private set; }
		public int FuelTankCapacity { get; private set; }
		public int MapFlightRange { get; private set; }
		public int MapFlightSpeed { get; private set; }
		public int AttackBonus { get; private set; }
		public int DefenseBonus { get; private set; }
		public int ExperienceBonus { get; private set; }
		public int ExplorationLootBonus { get; private set; }
		public int HeatDefenseBonus { get; private set; }
		public int KineticDefenseBonus { get; private set; }
		public int EnergyDefenseBonus { get; private set; }
		public int MerchantPriceReduction { get; private set; }
		public int CraftPriceReduction { get; private set; }
		public int CraftLevelReduction { get; private set; }
		public int ShieldStrengthBonus { get; private set; }
		public int ShieldRechargeBonus { get; private set; }
		public int IncreasedLevelLimit { get; private set; }

		public static SkillSettings DefaultValue { get; private set; }
	}
}
