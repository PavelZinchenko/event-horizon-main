using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Constructor;
using Constructor.Model;
using Constructor.Modification;
using GameDatabase.DataModel;
using GameDatabase.Enums;
using Economy.ItemType;
using Gui.ComponentList;
using Model;
using Services.Localization;
using Services.Reources;
using Zenject;
using Constructor.Component;

namespace ViewModel
{
	public class ComponentViewModel : ComponentListItemBase
	{
	    [Inject] private readonly ILocalization _localization;
	    [Inject] private readonly IResourceLocator _resourceLocator;

        [SerializeField] public ConstructorViewModel ConstructorViewModel;
        [SerializeField] public Button Button;
        [SerializeField] public Image Icon;
        [SerializeField] public Text Name;
        [SerializeField] public Text Modification;
        [SerializeField] public Text Count;
        [SerializeField] public Sprite EmptyIcon;
        [SerializeField] public LayoutGroup DescriptionLayout;

        public override void Initialize(ComponentInfo data, int count)
		{
		    if (Count != null)
		    {
                Count.gameObject.SetActive(count > 0);
		        Count.text = count.ToString();
		    }

            if (_component == data)
				return;

			_component = data;
            var model = _component.CreateComponent(ConstructorViewModel.ShipSize);

            if (Button)
                Button.interactable = model.IsSuitable(ConstructorViewModel.Ship.Model);

			UpdateDescription(model);
		}

		public void Clear()
		{
			Icon.sprite = EmptyIcon;
			Icon.color = Color.white;
			Name.text = "-";
			_component = new ComponentInfo();
		}

		public void OnClicked()
		{
			ConstructorViewModel.ShowComponent(_component);
		}

        public override ComponentInfo Component { get { return _component; } }

		private void UpdateDescription(Constructor.Component.IComponent component)
		{
		    if (Name != null)
		    {
				Name.text = _component.GetName(_localization);
		        Name.color = _component.ItemQuality.ToColor();
		    }
		    if (Icon != null)
			{
				Icon.sprite = _resourceLocator.GetSprite(_component.Data.Icon);
				Icon.color = _component.Data.Color;
			}

		    if (Modification != null)
		    {
		        var modification = component.Modification ?? EmptyModification.Instance;
		        Modification.gameObject.SetActive(!string.IsNullOrEmpty(Modification.text = modification.GetDescription(_localization)));
		        Modification.color = _component.ItemQuality.ToColor();
		    }

            if (DescriptionLayout)
		        DescriptionLayout.transform.InitializeElements<TextFieldViewModel, KeyValuePair<string,string>>(
				    GetDescription(component, _localization), UpdateTextField);
		}

		private void UpdateTextField(TextFieldViewModel viewModel, KeyValuePair<string, string> data)
		{
			viewModel.Label.text = _localization.GetString(data.Key);
			viewModel.Value.text = data.Value;
		}

		public static IEnumerable<KeyValuePair<string, string>> GetDescription(Constructor.Component.IComponent component, ILocalization localization)
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

        public override bool Selected { get; set; }

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

            var weapon = droneBay.Value.Components.Select<InstalledComponent,IntegratedComponent>(ComponentExtensions.FromDatabase).FirstOrDefault(item => item.Info.Data.Weapon != null);
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

		private ComponentInfo _component;
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
