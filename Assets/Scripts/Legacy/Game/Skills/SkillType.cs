﻿using System.Linq;
using GameDatabase.DataModel;
using GameServices.Player;
using Services.Localization;
using Session;

namespace GameModel.Skills
{
    public enum SkillType
    {
        Undefined           = 0,

        MainFuelCapacity    = 1,
        MainEnginePower     = 2,
        MainRescueUnit      = 3,

        ShipAttack          = 4,
        ShipDefense         = 5,
        ShipExperience      = 6,

        HeatDefense         = 7,
        EnergyDefense       = 8,
        KineticDefense      = 9,

        Trading             = 10,
        MasterTrader        = 11,

        PlanetaryScanner    = 12,
        SpaceScanner        = 13,

        RequierementMaxLevel = 14,

        HangarSlot1 = 15,
        HangarSlot2 = 16,
        HangarSlot3 = 17,
        HangarSlot4 = 18,
        HangarSlot5 = 19,

        CraftingPrice = 20,
        CraftingLevel = 21,

        ShieldStrength = 22,
        ShieldRecharge = 23,

        RequierementBeatAllEnemies = 24,

        ExceedTheLimits = 25,
    }

 //   public enum SkillClass
	//{
	//	Common,
	//	Free,
	//	Locked,
	//}

	public static class SkillTypeExtensions
	{
	    public static bool IsCommonSkill(this SkillType type)
	    {
	        switch (type)
	        {
                case SkillType.Undefined:
                case SkillType.RequierementBeatAllEnemies:
                case SkillType.RequierementMaxLevel:
                    return false;
                default:
                    return true;
	        }
	    }

        public static bool IsEnabled(this SkillType type, SkillSettings settings)
        {
            if (type == SkillType.ExceedTheLimits && settings.DisableExceedTheLimits) 
                return false;
            
            return true;
        }

		//public static SkillClass Class(this SkillType type, PlayerFleet fleet = null, ISessionData session = null)
		//{
		//	switch (type)
		//	{
		//	case SkillType.Undefined:
		//		return SkillClass.Free;
		//	case SkillType.RequierementMaxLevel:
		//		return fleet != null && fleet.Ships.Any(item => item.Experience.Level == Maths.Experience.MaxPlayerRank) ? SkillClass.Free : SkillClass.Locked;
		//	case SkillType.RequierementBeatAllEnemies:
		//	    {
                    
		//	        session.Regions.GetCapturedFactions()

		//	    }

  //                  return SkillClass.Locked;
		//	default:
		//		return SkillClass.Common;
		//	}
		//}

		public static string GetName(this SkillType type, ILocalization localization)
		{
			switch (type) 
			{
			case SkillType.MainFuelCapacity:
				return localization.GetString("$FuelTankCapacity");
			case SkillType.MainEnginePower:
				return localization.GetString("$MainEnginePower");
			case SkillType.MainRescueUnit:
				return localization.GetString("$MainRescueUnit");
			case SkillType.ShipAttack:
				return localization.GetString("$AttackBooster");
			case SkillType.ShipDefense:
				return localization.GetString("$DefenseBooster");
			case SkillType.ShipExperience:
				return localization.GetString("$ExperienceBooster");
			case SkillType.RequierementMaxLevel:
			case SkillType.RequierementBeatAllEnemies:
				return localization.GetString("$ConditionNode");
            case SkillType.HangarSlot1:
                return localization.GetString("$HangarSlotNode");
            case SkillType.HangarSlot2:
            case SkillType.HangarSlot3:
            case SkillType.HangarSlot4:
            case SkillType.HangarSlot5:
                return localization.GetString("$UpgradeHangarSlotNode");
            case SkillType.PlanetaryScanner:
                return localization.GetString("$PlanetaryScannerNode");
            case SkillType.HeatDefense:
                return localization.GetString("$HeatDefenseNode");
            case SkillType.EnergyDefense:
                return localization.GetString("$EnergyDefenseNode");
            case SkillType.KineticDefense:
                return localization.GetString("$KineticDefenseNode");
            case SkillType.Trading:
                return localization.GetString("$TradingNode");
            case SkillType.MasterTrader:
                return localization.GetString("$MasterTraderNode");
            case SkillType.CraftingPrice:
                return localization.GetString("$CraftingPriceNode");
            case SkillType.CraftingLevel:
                return localization.GetString("$CraftingLevelNode");
			case SkillType.SpaceScanner:
			    return localization.GetString("$SpaceScannerNode");
			case SkillType.ShieldStrength:
			    return localization.GetString("$ShieldStrengthNode");
			case SkillType.ShieldRecharge:
			    return localization.GetString("$ShieldRechargeNode");
			case SkillType.ExceedTheLimits:
			    return localization.GetString("$ExceedTheLimitsNode");
            }

            return string.Empty;
		}

		public static string GetDescription(this SkillType type, ILocalization localization, SkillSettings settings, int level = 1)
		{
			switch (type) 
			{
			case SkillType.MainFuelCapacity:
				return localization.GetString("$FuelTankCapacityDesc", settings.FuelTankCapacity(level) - settings.BaseFuelCapacity);
			case SkillType.MainEnginePower:
				return localization.GetString("$MainEnginePowerDesc", 
                    ToPercentage((settings.FlightRange(level) - settings.BaseFlightRange) / settings.BaseFlightRange), 
                    ToPercentage((settings.FlightSpeed(level) - settings.BaseFlightSpeed) / settings.BaseFlightSpeed));
			case SkillType.MainRescueUnit:
				return localization.GetString("$MainRescueUnitDesc");
			case SkillType.ShipAttack:
				return localization.GetString("$AttackBoosterDesc", ToPercentage(settings.AttackBonus(level)));
			case SkillType.ShipDefense:
				return localization.GetString("$DefenseBoosterDesc", ToPercentage(settings.DefenseBonus(level)));
			case SkillType.ShipExperience:
				return localization.GetString("$ExperienceBoosterDesc", ToPercentage(settings.ExperienceBonus(level)));
			case SkillType.RequierementMaxLevel:
				return localization.GetString("$ShipLevelsRequirement", level, settings.MaxPlayerShipsLevel);
            case SkillType.HangarSlot1:
                return localization.GetString("$NewHangarSlotDesc");
            case SkillType.HangarSlot2:
                return localization.GetString("$UpgradeHangarSlotDesc1");
            case SkillType.HangarSlot3:
                return localization.GetString("$UpgradeHangarSlotDesc2");
            case SkillType.HangarSlot4:
                return localization.GetString("$UpgradeHangarSlotDesc3");
            case SkillType.HangarSlot5:
                return localization.GetString("$UpgradeHangarSlotDesc4");
            case SkillType.PlanetaryScanner:
                return localization.GetString("$PlanetaryScannerDesc", ToPercentage(settings.ExplorationLootBonus(level)));
            case SkillType.HeatDefense:
                return localization.GetString("$HeatDefenseDesc", ToPercentage(settings.HeatResistance(level)));
            case SkillType.EnergyDefense:
                return localization.GetString("$EnergyDefenseDesc", ToPercentage(settings.EnergyResistance(level)));
            case SkillType.KineticDefense:
                return localization.GetString("$KineticDefenseDesc", ToPercentage(settings.KineticResistance(level)));
            case SkillType.Trading:
                return localization.GetString("$TradingDesc", ToPercentage(1.0f - settings.MerchantPriceFactor(level)));
            case SkillType.MasterTrader:
                return localization.GetString("$MasterTraderDesc");
            case SkillType.CraftingPrice:
                return localization.GetString("$CraftingPriceDesc", ToPercentage(1.0f - settings.CraftingPriceFactor(level)));
            case SkillType.CraftingLevel:
                return localization.GetString("$CraftingLevelDesc", settings.CraftingLevelReduction(level));
			case SkillType.SpaceScanner:
			    return localization.GetString("$SpaceScannerDesc", level);
			case SkillType.ShieldStrength:
			    return localization.GetString("$ShieldStrengthDesc", ToPercentage(settings.ShieldStrengthBonus(level)));
			case SkillType.ShieldRecharge:
			    return localization.GetString("$ShieldRechargeDesc", ToPercentage(settings.ShieldRechargeBonus(level)));
			case SkillType.RequierementBeatAllEnemies:
			    return localization.GetString("$BeatAllEnemiesRequirement");
			case SkillType.ExceedTheLimits:
			    return localization.GetString("$ExceedTheLimitsDesc");
            }

            return string.Empty;
		}

        private static int ToPercentage(float value) => UnityEngine.Mathf.RoundToInt(value*100);
	}
}
