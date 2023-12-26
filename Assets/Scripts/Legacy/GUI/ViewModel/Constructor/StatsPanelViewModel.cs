using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using CommonComponents.Utils;

namespace ViewModel
{
	public class StatsPanelViewModel : MonoBehaviour
	{
		public TextFieldViewModel ArmorPoints;
		public TextFieldViewModel RepairRate;
		public TextFieldViewModel Energy;
		public TextFieldViewModel RechargeRate;
        public TextFieldViewModel Shield;
        public TextFieldViewModel ShieldRechargeRate;
        public TextFieldViewModel Weight;
		public TextFieldViewModel RamDamage;
		public TextFieldViewModel DamageAbsorption;
		public TextFieldViewModel Velocity;
		public TextFieldViewModel TurnRate;
		public TextFieldViewModel WeaponDamage;
		public TextFieldViewModel WeaponFireRate;
		public TextFieldViewModel WeaponRange;
		public TextFieldViewModel WeaponEnergyConsumption;
		public TextFieldViewModel DroneDamageModifier;
	    public TextFieldViewModel DroneDefenseModifier;
        public TextFieldViewModel DroneRangeModifier;
		public TextFieldViewModel DroneSpeedModifier;
		public TextFieldViewModel DroneTimeModifier;
		public TextFieldViewModel EnergyDamageResistance;
		public TextFieldViewModel KineticDamageResistance;
		public TextFieldViewModel HeatDamageResistance;
		public GameObject WeaponsBlock;
		public GameObject DronesBlock;
		public GameObject ResistanceBlock;

		public Color NormalColor;
		public Color ErrorColor;

		public Text HitPointsSummaryText;
		public Text EnergySummaryText;
		public Text VelocitySummaryText;

		public CanvasGroup CanvasGroup;

		public void OnMoreInfoButtonClicked(bool isOn)
		{
			CanvasGroup.alpha = isOn ? 1 : 0;
		}

        public void UpdateStats(Constructor.IShipSpecification spec)
		{
			HitPointsSummaryText.text = spec.Stats.ArmorPoints.AsInteger();
            HitPointsSummaryText.color = spec.Stats.ArmorPoints > 0 ? NormalColor : ErrorColor;

            EnergySummaryText.text = spec.Stats.EnergyPoints.AsInteger() + " [" + spec.Stats.EnergyRechargeRate.AsSignedInteger() + "]";
            EnergySummaryText.color = spec.Stats.EnergyRechargeRate > 0 ? NormalColor : ErrorColor;
            VelocitySummaryText.text = spec.Stats.EnginePower.ToString("N1") + " / " + spec.Stats.TurnRate.ToString("N1");
            VelocitySummaryText.color = spec.Stats.EnginePower > 0 && spec.Stats.TurnRate > 0 ? NormalColor : ErrorColor;

            ArmorPoints.gameObject.SetActive(!Mathf.Approximately(spec.Stats.ArmorPoints, 0));
            ArmorPoints.Value.text = spec.Stats.ArmorPoints.AsDecimal();
            ArmorPoints.Color = spec.Stats.ArmorPoints > 0 ? NormalColor : ErrorColor;

            RepairRate.gameObject.SetActive(spec.Stats.ArmorRepairRate > 0);
			RepairRate.Value.text = spec.Stats.ArmorRepairRate.AsDecimal();

            Shield.gameObject.SetActive(spec.Stats.ShieldPoints > 0);
            Shield.Value.text = spec.Stats.ShieldPoints.AsDecimal();
            ShieldRechargeRate.gameObject.SetActive(spec.Stats.ShieldPoints > 0);
            ShieldRechargeRate.Value.text = spec.Stats.ShieldRechargeRate.AsDecimal();

            Energy.Value.text = spec.Stats.EnergyPoints.AsDecimal();
			Weight.Value.text = Mathf.RoundToInt(spec.Stats.Weight*1000).ToString();
			RechargeRate.Value.text = spec.Stats.EnergyRechargeRate.AsDecimal();
			RechargeRate.Color = spec.Stats.EnergyRechargeRate > 0 ? NormalColor : ErrorColor;
			Velocity.Color = spec.Stats.EnginePower > 0 ? NormalColor : ErrorColor;
			Velocity.Value.text = spec.Stats.EnginePower.AsDecimal();
			TurnRate.Color = spec.Stats.TurnRate > 0 ? NormalColor : ErrorColor;
			TurnRate.Value.text = spec.Stats.TurnRate.AsDecimal();

			WeaponDamage.gameObject.SetActive(spec.Stats.WeaponDamageMultiplier.HasValue);
			WeaponDamage.Value.text = spec.Stats.WeaponDamageMultiplier.ToString();
			WeaponFireRate.gameObject.SetActive(spec.Stats.WeaponFireRateMultiplier.HasValue);
			WeaponFireRate.Value.text = spec.Stats.WeaponFireRateMultiplier.ToString();
			WeaponRange.gameObject.SetActive(spec.Stats.WeaponRangeMultiplier.HasValue);
			WeaponRange.Value.text = spec.Stats.WeaponRangeMultiplier.ToString();
			WeaponEnergyConsumption.gameObject.SetActive(spec.Stats.WeaponEnergyCostMultiplier.HasValue);
			WeaponEnergyConsumption.Value.text = spec.Stats.WeaponEnergyCostMultiplier.ToString();
			WeaponsBlock.gameObject.SetActive(WeaponsBlock.transform.Cast<Transform>().Count(item => item.gameObject.activeSelf) > 1);

			DroneDamageModifier.gameObject.SetActive(spec.Stats.DroneDamageMultiplier.HasValue);
			DroneDamageModifier.Value.text = spec.Stats.DroneDamageMultiplier.ToString();
            DroneDefenseModifier.gameObject.SetActive(spec.Stats.DroneDefenseMultiplier.HasValue);
            DroneDefenseModifier.Value.text = spec.Stats.DroneDefenseMultiplier.ToString();
            DroneRangeModifier.gameObject.SetActive(spec.Stats.DroneRangeMultiplier.HasValue);
			DroneRangeModifier.Value.text = spec.Stats.DroneRangeMultiplier.ToString();
			DroneSpeedModifier.gameObject.SetActive(spec.Stats.DroneSpeedMultiplier.HasValue);
			DroneSpeedModifier.Value.text = spec.Stats.DroneSpeedMultiplier.ToString();
			DroneTimeModifier.gameObject.SetActive(spec.Stats.DroneBuildSpeed > 0);
			DroneTimeModifier.Value.text = spec.Stats.DroneBuildTime.AsDecimal();
			DronesBlock.gameObject.SetActive(DronesBlock.transform.Cast<Transform>().Count(item => item.gameObject.activeSelf) > 1);

			RamDamage.gameObject.SetActive(!Mathf.Approximately(spec.Stats.RammingDamage, 0));
			RamDamage.Value.text = spec.Stats.RammingDamageMultiplier.AsPercent();
			DamageAbsorption.gameObject.SetActive(spec.Stats.EnergyAbsorption > 0);
			DamageAbsorption.Value.text = spec.Stats.EnergyAbsorptionPercentage.AsPercent();

			KineticDamageResistance.gameObject.SetActive(!Mathf.Approximately(spec.Stats.KineticResistance, 0));
			KineticDamageResistance.Value.text = $"{spec.Stats.KineticResistance.AsDecimal()} ( {spec.Stats.KineticResistancePercentage.AsPercent()})";
			HeatDamageResistance.gameObject.SetActive(!Mathf.Approximately(spec.Stats.ThermalResistance, 0));
			HeatDamageResistance.Value.text = $"{spec.Stats.ThermalResistance.AsDecimal()} ( {spec.Stats.ThermalResistancePercentage.AsPercent()})";
			EnergyDamageResistance.gameObject.SetActive(!Mathf.Approximately(spec.Stats.EnergyResistance, 0));
			EnergyDamageResistance.Value.text = $"{spec.Stats.EnergyResistance.AsDecimal()} ( {spec.Stats.EnergyResistancePercentage.AsPercent()})";
			ResistanceBlock.gameObject.SetActive(ResistanceBlock.transform.Cast<Transform>().Count(item => item.gameObject.activeSelf) > 1);
		}
    }
}
