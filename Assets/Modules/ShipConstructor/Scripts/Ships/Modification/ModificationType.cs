using System;
using Economy;
using GameDatabase.Model;
using UnityEngine;

namespace Constructor.Ships.Modification
{
    public enum ModificationType
    {
        Empty          =-1,
        AutoTargeting  = 0,
        HeatDefense    = 1,
        KineticDefense = 2,
        EnergyDefense  = 3,
        ExtraBlueCells = 4,
        LightWeight    = 5,
        Infected       = 6,
        UnlimitedRespawn = 7,
        WeaponClass    = 8,
        SatelliteSize  = 9,
        EnergyRechargeCooldown = 10,
        ShieldRechargeCooldown = 11,
    }

    public static class ModificationTypeExtension
    {
        public static ModificationType ToModificationType(this GameDatabase.Enums.ShipPerkType perkType)
        {
            return perkType switch
            {
                GameDatabase.Enums.ShipPerkType.AutoTargeting => ModificationType.AutoTargeting,
                GameDatabase.Enums.ShipPerkType.HeatDefense => ModificationType.HeatDefense,
                GameDatabase.Enums.ShipPerkType.KineticDefense => ModificationType.KineticDefense,
                GameDatabase.Enums.ShipPerkType.EnergyDefense => ModificationType.EnergyDefense,
                GameDatabase.Enums.ShipPerkType.LightWeight => ModificationType.LightWeight,
                GameDatabase.Enums.ShipPerkType.Infected => ModificationType.Infected,
                GameDatabase.Enums.ShipPerkType.UnlimitedRespawn => ModificationType.UnlimitedRespawn,
                GameDatabase.Enums.ShipPerkType.WeaponClass => ModificationType.WeaponClass,
                GameDatabase.Enums.ShipPerkType.SatelliteSize => ModificationType.SatelliteSize,
                GameDatabase.Enums.ShipPerkType.EnergyRechargeCooldown => ModificationType.EnergyRechargeCooldown,
                GameDatabase.Enums.ShipPerkType.ShieldRechargeCooldown => ModificationType.ShieldRechargeCooldown,
                _ => ModificationType.Empty
            };
        }

        public static bool IsSuitable(this ModificationType type, IShipModel ship)
        {
            if (type == ModificationType.Empty)
                return true;

            if (!type.AllowMultiple() && ship.HasModification(type))
                return false;

            switch (type)
            {
                case ModificationType.AutoTargeting:
                    return AutoTargetingModification.IsSuitable(ship.OriginalShip);
                case ModificationType.WeaponClass:
                    return WeaponClassMod.IsSuitable(ship.OriginalShip);
                case ModificationType.SatelliteSize:
                    return SatelliteSizeMod.IsSuitable(ship.OriginalShip);
                default:
                    return true;
            }
        }

        public static bool HasModification(this IShipModel shipModel, ModificationType type)
        {
            var modifications = shipModel.Modifications;
            for (int i = 0; i < modifications.Count; ++i)
                if (modifications[i].Type == type)
                    return true;

            return false;
        }

        public static bool AllowMultiple(this ModificationType type)
        {
            switch (type)
            {
                case ModificationType.EnergyDefense:
                case ModificationType.KineticDefense:
                case ModificationType.HeatDefense:
                case ModificationType.LightWeight:
                    return true;
                default:
                    return false;
            }
        }

        public static Price GetInstallPrice(this ModificationType type)
        {
            if (type == ModificationType.Empty)
                return Price.Premium(0);
            else
                return Price.Premium(10);
        }

        public static SpriteId GetIconId(this ModificationType type)
        {
            switch (type)
            {
                case ModificationType.AutoTargeting:
                    return new SpriteId("icon_weapon", SpriteId.Type.GuiIcon);
                case ModificationType.Empty:
                    return new SpriteId(string.Empty, SpriteId.Type.GuiIcon);
                case ModificationType.EnergyDefense:
                    return new SpriteId("icon_energy_resist", SpriteId.Type.GuiIcon);
                case ModificationType.HeatDefense:
                    return new SpriteId("icon_fire_resist", SpriteId.Type.GuiIcon);
                case ModificationType.KineticDefense:
                    return new SpriteId("icon_impact_resist", SpriteId.Type.GuiIcon);
                case ModificationType.ExtraBlueCells:
                    return new SpriteId("icon_cargo", SpriteId.Type.GuiIcon);
                case ModificationType.LightWeight:
                    return new SpriteId("icon_gear", SpriteId.Type.GuiIcon);
                case ModificationType.Infected:
                    return new SpriteId("icon_virus", SpriteId.Type.GuiIcon);
                case ModificationType.UnlimitedRespawn:
                    return new SpriteId("icon_refresh", SpriteId.Type.GuiIcon);
                case ModificationType.WeaponClass:
                    return new SpriteId("icon_blueprint", SpriteId.Type.GuiIcon);
                case ModificationType.SatelliteSize:
                    return new SpriteId("icon_scanner", SpriteId.Type.GuiIcon);
                case ModificationType.EnergyRechargeCooldown:
                    return new SpriteId("icon_battery", SpriteId.Type.GuiIcon);
                case ModificationType.ShieldRechargeCooldown:
                    return new SpriteId("ship", SpriteId.Type.GuiIcon);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static Color GetColor(this ModificationType type)
        {
            switch (type)
            {
                case ModificationType.EnergyDefense:
                case ModificationType.HeatDefense:
                case ModificationType.KineticDefense:
                    return Color.white;
                case ModificationType.Empty:
                    return new Color(0,0,0,0);
                default:
                    return new Color(0.5f,1f,1f);
            }
        }
    }
}
