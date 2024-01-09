using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Constructor;
using Constructor.Model;
using Constructor.Modification;
using GameDatabase.DataModel;
using GameDatabase.Enums;
using Economy.ItemType;
using Services.Localization;
using Services.Resources;
using Constructor.Component;
using GameDatabase.Extensions;
using ShipEditor.Model;

namespace ShipEditor.UI
{
	public class ComponentItem : MonoBehaviour
	{
	    [Inject] private readonly ILocalization _localization;
		[Inject] private readonly IResourceLocator _resourceLocator;
		[Inject] private readonly IShipEditorModel _shipEditor;

		[SerializeField] private Image _icon;
		[SerializeField] private Text _name;
        [SerializeField] private Text _modification;
        [SerializeField] private Sprite _emptyIcon;
        [SerializeField] private LayoutGroup _description;

		[SerializeField] private Text _sizeText;
		[SerializeField] private Image _requiredCellIcon;
		[SerializeField] private Text _requiredCellText;

		[SerializeField] private Color _weaponCellColor;
		[SerializeField] private Color _outerCellColor;
		[SerializeField] private Color _innerCellColor;
		[SerializeField] private Color _engineCellColor;
		[SerializeField] private Color _emptyCellColor;

		public void Initialize(ComponentInfo component)
		{
			_requiredCellIcon.color = GetCellColor(component.Data.CellType);
			_requiredCellText.text = component.Data.CellType == CellType.Weapon ? SlotTypeToString(component.Data.WeaponSlotType) : string.Empty;
			_sizeText.text = component.Data.Layout.CellCount.ToString();

			UpdateDescription(component);
		}

		private Color GetCellColor(CellType cellType)
		{
			switch (cellType)
			{
				case CellType.Weapon: return _weaponCellColor;
				case CellType.Outer: return _outerCellColor;
				case CellType.Inner: return _innerCellColor;
				case CellType.InnerOuter: return Color.Lerp(_innerCellColor, _outerCellColor, 0.5f);
				case CellType.Engine: return _engineCellColor;
				case CellType.Empty:
				default:
					return _emptyCellColor;
			}
		}

		private static string SlotTypeToString(WeaponSlotType type)
		{
			if (type == WeaponSlotType.Default)
				return string.Empty;

			return ((char)type).ToString();
		}

		public void Clear()
		{
			_icon.sprite = _emptyIcon;
			_icon.color = Color.white;
			_name.text = "-";
		}

		private void UpdateDescription(ComponentInfo info)
		{
			var component = info.CreateComponent(_shipEditor.Ship.Model.Layout.CellCount);

			_name.text = info.GetName(_localization);
		    _name.color = info.ItemQuality.ToColor();

			_icon.sprite = _resourceLocator.GetSprite(info.Data.Icon);
			_icon.color = info.Data.Color;

		    var modification = component.Modification ?? EmptyModification.Instance;
		    _modification.gameObject.SetActive(!string.IsNullOrEmpty(_modification.text = modification.GetDescription(_localization)));
		    _modification.color = info.ItemQuality.ToColor();

			if (_description)
			{
				_description.transform.InitializeElements<NameValueItem, KeyValuePair<string, string>>(
					GetDescription(component, _localization), UpdateTextField);
			}
		}

		private void UpdateTextField(NameValueItem item, KeyValuePair<string, string> data)
		{
			item.Label.text = _localization.GetString(data.Key);
			item.Value.text = data.Value;
		}

		public static IEnumerable<KeyValuePair<string, string>> GetDescription(IComponent component, ILocalization localization)
		{
			var stats = new ShipEquipmentStats();
			component.UpdateStats(ref stats);

			if (stats.ArmorPoints != 0)
				yield return new KeyValuePair<string, string>("$HitPoints", FormatFloat(stats.ArmorPoints));
            if (stats.ArmorRepairRate > 0)
                yield return new KeyValuePair<string, string>("$RepairRate", FormatFloat(stats.ArmorRepairRate));

			if (!Mathf.Approximately(stats.EnergyPoints, 0))
				yield return new KeyValuePair<string, string>("$Energy", FormatFloat(stats.EnergyPoints));

			if (stats.EnergyConsumption > 0)
				yield return new KeyValuePair<string, string>("$EnergyConsumption", FormatFloat(stats.EnergyConsumption));
			if (stats.EnergyRecharge > 0)
				yield return new KeyValuePair<string, string>("$RechargeRate", FormatFloat(stats.EnergyRecharge));

		    if (!Mathf.Approximately(stats.ShieldPoints, 0))
		        yield return new KeyValuePair<string, string>("$ShieldPoints", FormatFloat(stats.ShieldPoints));
            if (!Mathf.Approximately(stats.ShieldRechargeRate, 0))
		        yield return new KeyValuePair<string, string>("$ShieldRechargeRate", FormatFloat(stats.ShieldRechargeRate));

		    if (stats.EnginePower != 0)
				yield return new KeyValuePair<string, string>("$Velocity", FormatFloat(stats.EnginePower));
			if (stats.TurnRate != 0)
				yield return new KeyValuePair<string, string>("$TurnRate", FormatFloat(stats.TurnRate));

			if (stats.WeaponDamageMultiplier.HasValue)
				yield return new KeyValuePair<string, string>("$DamageModifier", stats.WeaponDamageMultiplier.ToString());
			if (stats.WeaponFireRateMultiplier.HasValue)
				yield return new KeyValuePair<string, string>("$FireRateModifier", stats.WeaponFireRateMultiplier.ToString());
			if (stats.WeaponRangeMultiplier.HasValue)
				yield return new KeyValuePair<string, string>("$RangeModifier", stats.WeaponRangeMultiplier.ToString());
			if (stats.WeaponEnergyCostMultiplier.HasValue)
				yield return new KeyValuePair<string, string>("$EnergyModifier", stats.WeaponEnergyCostMultiplier.ToString());

			if (stats.RammingDamage != 0)
				yield return new KeyValuePair<string, string>("$RamDamage", FormatFloat(stats.RammingDamage));
			if (stats.EnergyAbsorption != 0)
				yield return new KeyValuePair<string, string>("$DamageAbsorption", FormatFloat(stats.EnergyAbsorption));

			if (stats.KineticResistance != 0)
				yield return new KeyValuePair<string, string>("$KineticDamageResistance", FormatFloat(stats.KineticResistance));
			if (stats.ThermalResistance != 0)
				yield return new KeyValuePair<string, string>("$ThermalDamageResistance", FormatFloat(stats.ThermalResistance));
			if (stats.EnergyResistance != 0)
				yield return new KeyValuePair<string, string>("$EnergyDamageResistance", FormatFloat(stats.EnergyResistance));

			if (stats.DroneDamageMultiplier.HasValue)
				yield return new KeyValuePair<string, string>("$DroneDamageModifier", stats.DroneDamageMultiplier.ToString());
            if (stats.DroneDefenseMultiplier.HasValue)
                yield return new KeyValuePair<string, string>("$DroneDefenseModifier", stats.DroneDefenseMultiplier.ToString());
            if (stats.DroneRangeMultiplier.HasValue)
				yield return new KeyValuePair<string, string>("$DroneRangeModifier", stats.DroneRangeMultiplier.ToString());
			if (stats.DroneSpeedMultiplier.HasValue)
				yield return new KeyValuePair<string, string>("$DroneSpeedModifier", stats.DroneSpeedMultiplier.ToString());
			if (stats.DroneReconstructionSpeed > 0)
				yield return new KeyValuePair<string, string>("$DroneReconstructionTime", (1f/stats.DroneReconstructionSpeed).ToString("N1"));
            if (stats.DroneReconstructionTimeMultiplier.HasValue)
                yield return new KeyValuePair<string, string>("$DroneReconstructionTime", stats.DroneReconstructionTimeMultiplier.ToString());

			var platform = new DummyPlatform();
			component.UpdateWeaponPlatform(platform);
            if (platform.TurnRate != 0)
                yield return new KeyValuePair<string, string>("$TurretTurnRate", localization.GetString("$ValuePerSecond", FormatFloat(platform.TurnRate)));

            // TODO: display component type
            //DeviceInfo.SetActive(component.Devices.Any());
            //DroneBayInfo.SetActive(component.DroneBays.Any());

            if (component.Weapons.Any())
			{
				var data = component.Weapons.First();
                var info = new WeaponDamageCalculator().CalculateWeaponDamage(data.Weapon.Stats, data.Ammunition, data.StatModifier);
                foreach (var item in GetWeaponDescription(info, localization))
					yield return item;
			}

			if (component.WeaponsObsolete.Any())
			{
                var data = component.WeaponsObsolete.First();
                var info = new WeaponDamageCalculator().CalculateWeaponDamage(data.Key, data.Value);
                foreach (var item in GetWeaponDescription(info, localization))
					yield return item;
			}

            if (component.DroneBays.Any())
                foreach (var item in GetDroneBayDescription(component.DroneBays.First(), localization))
                    yield return item;

            if (!Mathf.Approximately(stats.Weight + stats.WeightReduction, 0))
				yield return new KeyValuePair<string, string>("$Weight", Mathf.RoundToInt(stats.Weight + stats.WeightReduction).ToString());
		}

        private static IEnumerable<KeyValuePair<string, string>> GetWeaponDamageText(WeaponDamageCalculator.WeaponInfo data)
		{
			var damageSuffix = data.Magazine <= 1 ? string.Empty : "х" + data.Magazine;

			if (data.Damage.Kinetic > 0)
                yield return new KeyValuePair<string, string>("$KineticDamage", data.Damage.Kinetic.ToString(_floatFormat) + damageSuffix);
			else if (data.Dps.Kinetic > 0)
                yield return new KeyValuePair<string, string>("$KineticDPS", data.Dps.Kinetic.ToString(_floatFormat) + damageSuffix);

            if (data.Damage.Energy > 0)
                yield return new KeyValuePair<string, string>("$EnergyDamage", data.Damage.Energy.ToString(_floatFormat) + damageSuffix);
            else if (data.Dps.Energy > 0)
                yield return new KeyValuePair<string, string>("$EnergyDPS", data.Dps.Energy.ToString(_floatFormat) + damageSuffix);

            if (data.Damage.Heat > 0)
                yield return new KeyValuePair<string, string>("$HeatDamage", data.Damage.Heat.ToString(_floatFormat) + damageSuffix);
            else if (data.Dps.Heat > 0)
                yield return new KeyValuePair<string, string>("$HeatDPS", data.Dps.Heat.ToString(_floatFormat) + damageSuffix);

            if (data.Damage.Direct > 0)
                yield return new KeyValuePair<string, string>("$WeaponDamage", data.Damage.Direct.ToString(_floatFormat) + damageSuffix);
            else if (data.Dps.Direct > 0)
                yield return new KeyValuePair<string, string>("$WeaponDPS", data.Dps.Direct.ToString(_floatFormat) + damageSuffix);
        }

        private static IEnumerable<KeyValuePair<string, string>> GetWeaponDescription(WeaponDamageCalculator.WeaponInfo data, ILocalization localization)
	    {
            foreach (var item in GetWeaponDamageText(data))
                yield return item;

            if (data.Continuous)
	        {
	            yield return new KeyValuePair<string, string>("$WeaponEPS", data.EnergyCost.ToString(_floatFormat));
	        }
	        else
	        {
	            yield return new KeyValuePair<string, string>("$WeaponEnergy", data.EnergyCost.ToString(_floatFormat));
	            yield return new KeyValuePair<string, string>("$WeaponCooldown", (1.0f / data.FireRate).ToString(_floatFormat));
            }

            if (data.Range > 0)
                yield return new KeyValuePair<string, string>("$WeaponRange", data.Range.ToString(_floatFormat));
            if (data.BulletVelocity > 0)
                yield return new KeyValuePair<string, string>("$WeaponVelocity", data.BulletVelocity.ToString(_floatFormat));
            if (data.Impulse > 0)
                yield return new KeyValuePair<string, string>("$WeaponImpulse", (data.Impulse * 1000).ToString(_floatFormat));
            if (data.AreaOfEffect > 0)
                yield return new KeyValuePair<string, string>("$WeaponArea", data.AreaOfEffect.ToString(_floatFormat));
        }

		private static IEnumerable<KeyValuePair<string, string>> GetWeaponDamageText(Ammunition ammunition, WeaponStatModifier statModifier, ILocalization localization)
		{
			while (true)
			{
				var effect = ammunition.Effects.FirstOrDefault(item => item.Type == ImpactEffectType.Damage || item.Type == ImpactEffectType.SiphonHitPoints);
				if (effect?.Power > 0)
				{
					yield return new KeyValuePair<string, string>("$DamageType", localization.GetString(effect.DamageType.Name()));
					var damage = effect.Power * statModifier.DamageMultiplier.Value;
					yield return new KeyValuePair<string, string>(ammunition.ImpactType == BulletImpactType.DamageOverTime ? "$WeaponDPS" : "$WeaponDamage", damage.ToString(_floatFormat));
					yield break;
				}

				var trigger = ammunition.Triggers.OfType<BulletTrigger_SpawnBullet>().FirstOrDefault();
				if (trigger?.Ammunition != null)
				{
					ammunition = trigger.Ammunition;
					continue;
				}

				break;
			}
		}

		private static IEnumerable<KeyValuePair<string, string>> GetDroneBayDescription(KeyValuePair<DroneBayStats,ShipBuild> droneBay, ILocalization localization)
        {
            yield return new KeyValuePair<string, string>("$DroneBayCapacity", droneBay.Key.Capacity.ToString());
            if (!Mathf.Approximately(droneBay.Key.DamageMultiplier, 1f))
                yield return new KeyValuePair<string, string>("$DroneDamageModifier", FormatPercent(droneBay.Key.DamageMultiplier - 1f));
            if (!Mathf.Approximately(droneBay.Key.DefenseMultiplier, 1f))
                yield return new KeyValuePair<string, string>("$DroneDefenseModifier", FormatPercent(droneBay.Key.DefenseMultiplier - 1f));
            if (!Mathf.Approximately(droneBay.Key.SpeedMultiplier, 1f))
                yield return new KeyValuePair<string, string>("$DroneSpeedModifier", FormatPercent(droneBay.Key.SpeedMultiplier - 1f));

            yield return new KeyValuePair<string, string>("$DroneRangeModifier", droneBay.Key.Range.ToString("N"));

            var weapon = droneBay.Value.Components.Select(Constructor.ComponentExtensions.FromDatabase).FirstOrDefault(item => item.Info.Data.Weapon != null);
            if (weapon != null)
                yield return new KeyValuePair<string, string>("$WeaponType", localization.GetString(weapon.Info.Data.Name));
        }

        private static string FormatInt(int value)
		{
			return (value >= 0 ? "+" : "") + value;
		}

		private static string FormatFloat(float value)
		{
			return (value >= 0 ? "+" : "") + value.ToString(_floatFormat);
		}

		private static string FormatPercent(float value)
		{
			return (value >= 0 ? "+" : "") + Mathf.RoundToInt(100*value) + "%";
		}

	    private const string _floatFormat = "0.##";

		private class DummyPlatform : IWeaponPlatformStats
		{
			public void ChangeAutoAimingArc(float angle)
			{
				AutoAimingArc = angle;
			}

			public void ChangeTurnRate(float deltaAngle)
			{
				TurnRate += deltaAngle;
			}

			public float AutoAimingArc { get; private set; }
			public float TurnRate { get; private set; }
		}
	}
}
