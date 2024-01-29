using Constructor.Model;
using GameDatabase.DataModel;
using GameDatabase.Enums;
using Services.Localization;

namespace Constructor.Modification
{
	class ComponentModification : IModification
	{
		private readonly ComponentMod _modification;
		private readonly ModificationQuality _quality;

		public static IModification Create(ComponentMod modification, ModificationQuality quality)
		{
			return modification == ComponentMod.Empty ? EmptyModification.Instance : new ComponentModification(modification, quality);
		}

		public ModificationQuality Quality => _quality;

		public string GetDescription(ILocalization localization)
		{
			if (_modification.Modifications.Count == 0)
				return string.Empty;

			var arguments = new string[_modification.Modifications.Count];
			for (int i = 0; i < _modification.Modifications.Count; ++i)
			{
				var mod = _modification.Modifications[i];
				var value = GetModifier(mod, _quality);

				switch (mod.Type)
				{
					case StatModificationType.ExtraHitPoints:
						arguments[i] = Maths.Format.SignedFloat(value);
						break;
					default:
						arguments[i] = Maths.Format.SignedPercent(value - 1.0f);
						break;
				}
			}

			return localization.GetString(_modification.Description, arguments);
		}

		public void Apply(ref ShipEquipmentStats stats)
		{
			for (int i = 0; i < _modification.Modifications.Count; ++i)
			{
				var mod = _modification.Modifications[i];
				var value = GetModifier(mod, _quality);

				switch (mod.Type)
				{
					case StatModificationType.DroneAttack:
						if (stats.DroneDamageMultiplier.HasValue)
							stats.DroneDamageMultiplier = stats.DroneDamageMultiplier.AmplifyDelta(value);
						break;
					case StatModificationType.DroneDefense:
						if (stats.DroneDefenseMultiplier.HasValue)
							stats.DroneDefenseMultiplier = stats.DroneDefenseMultiplier.AmplifyDelta(value);
						break;
					case StatModificationType.DroneSpeed:
						if (stats.DroneSpeedMultiplier.HasValue)
							stats.DroneSpeedMultiplier = stats.DroneDefenseMultiplier.AmplifyDelta(value);
						break;
					case StatModificationType.DroneRange:
						if (stats.DroneRangeMultiplier.HasValue)
							stats.DroneRangeMultiplier = stats.DroneDefenseMultiplier.AmplifyDelta(value);
						break;
					case StatModificationType.EnergyCapacity:
						if (stats.EnergyPoints > 0)
							stats.EnergyPoints *= value;
						break;
					case StatModificationType.EnergyRechargeRate:
						stats.EnergyRecharge *= value;
						break;
					case StatModificationType.ShieldPoints:
						if (stats.ShieldPoints > 0)
							stats.ShieldPoints *= value;
						break;
					case StatModificationType.ShieldRechargeRate:
						if (stats.ShieldRechargeRate > 0)
							stats.ShieldRechargeRate *= value;
						break;
					case StatModificationType.ArmorPoints:
						if (stats.ArmorPoints > 0)
							stats.ArmorPoints *= value;
						break;
					case StatModificationType.ArmorRepairRate:
						stats.ArmorRepairRate *= value;
						break;
					case StatModificationType.Resistance:
						if (stats.EnergyResistance > 0)
							stats.EnergyResistance *= value;
						if (stats.ThermalResistance > 0)
							stats.ThermalResistance *= value;
						if (stats.KineticResistance > 0)
							stats.KineticResistance *= value;
						break;
					case StatModificationType.EnginePower:
						if (stats.EnginePower > 0)
							stats.EnginePower *= value;
						break;
					case StatModificationType.EngineTurnRate:
						if (stats.TurnRate > 0)
							stats.TurnRate *= value;
						break;
					case StatModificationType.Mass:
						if (stats.Weight > 0)
							stats.Weight *= value;
						break;
					case StatModificationType.EnergyCost:
						stats.EnergyConsumption *= value;
						break;
					case StatModificationType.ExtraHitPoints:
						stats.ArmorPoints += value;
						break;
				}
			}
		}

		public void Apply(ref DeviceStats device)
		{
			for (int i = 0; i < _modification.Modifications.Count; ++i)
			{
				var mod = _modification.Modifications[i];
				var value = GetModifier(mod, _quality);

				switch (mod.Type)
				{
					case StatModificationType.DeviceCooldown:
						device.Cooldown *= value;
						break;
					case StatModificationType.DeviceRange:
						device.Range *= value;
						break;
					case StatModificationType.DevicePower:
						device.Power *= value;
						break;
					case StatModificationType.EnergyCost:
						if (device.EnergyConsumption > 0)
							device.EnergyConsumption *= value;
						break;
				}
			}
		}

		public void Apply(ref WeaponStats weapon, ref AmmunitionObsoleteStats ammunition)
		{
			for (int i = 0; i < _modification.Modifications.Count; ++i)
			{
				var mod = _modification.Modifications[i];
				var value = GetModifier(mod, _quality);

				switch (mod.Type)
				{
					case StatModificationType.WeaponAoe:
						ammunition.AreaOfEffect *= value;
						break;
					case StatModificationType.WeaponBulletMass:
						ammunition.Impulse *= value;
						ammunition.Recoil *= value;
						break;
					case StatModificationType.WeaponBulletSpeed:
						ammunition.Velocity *= value;
						break;
					case StatModificationType.WeaponDamage:
						ammunition.Damage *= value;
						break;
					case StatModificationType.WeaponFireRate:
						weapon.FireRate *= value;
						break;
					case StatModificationType.WeaponRange:
						ammunition.Range *= value;
						break;
					case StatModificationType.EnergyCost:
						if (ammunition.EnergyCost > 0)
							ammunition.EnergyCost *= value;
						break;
				}
			}
		}

		public void Apply(ref WeaponStatModifier statModifier)
		{
			for (int i = 0; i < _modification.Modifications.Count; ++i)
			{
				var mod = _modification.Modifications[i];
				var value = GetModifier(mod, _quality);

				switch (mod.Type)
				{
					case StatModificationType.WeaponAoe:
						statModifier.AoeRadiusMultiplier *= value;
						break;
					case StatModificationType.WeaponBulletMass:
						statModifier.WeightMultiplier *= value;
						break;
					case StatModificationType.WeaponBulletSpeed:
						statModifier.VelocityMultiplier *= value;
						break;
					case StatModificationType.WeaponDamage:
						statModifier.DamageMultiplier *= value;
						break;
					case StatModificationType.WeaponFireRate:
						statModifier.FireRateMultiplier *= value;
						break;
					case StatModificationType.WeaponRange:
						statModifier.RangeMultiplier *= value;
						break;
					case StatModificationType.EnergyCost:
						statModifier.EnergyCostMultiplier *= value;
						break;
				}
			}
		}

		public void Apply(ref DroneBayStats droneBay)
		{
			for (int i = 0; i < _modification.Modifications.Count; ++i)
			{
				var mod = _modification.Modifications[i];
				var value = GetModifier(mod, _quality);

				switch (mod.Type)
				{
					case StatModificationType.DroneAttack:
						droneBay.DamageMultiplier += value - 1f;
						break;
					case StatModificationType.DroneDefense:
						droneBay.DefenseMultiplier += value - 1f;
						break;
					case StatModificationType.DroneSpeed:
						droneBay.SpeedMultiplier += value - 1.0f;
						break;
					case StatModificationType.DroneRange:
						droneBay.Range *= value;
						break;
					case StatModificationType.EnergyCost:
						if (droneBay.EnergyConsumption > 0)
							droneBay.EnergyConsumption *= value;
						break;
				}
			}
		}

		private float GetModifier(StatModification mod, ModificationQuality quality)
		{
			switch (quality)
			{
				case ModificationQuality.N3: return mod.Gray3;
				case ModificationQuality.N2: return mod.Gray2;
				case ModificationQuality.N1: return mod.Gray1;
				case ModificationQuality.P1: return mod.Green;
				case ModificationQuality.P2: return mod.Purple;
				case ModificationQuality.P3: return mod.Gold;
				default:
					return 0f;
			}
		}

		private ComponentModification(ComponentMod modification, ModificationQuality quality)
		{
			_modification = modification;
			_quality = quality;
		}
	}
}
