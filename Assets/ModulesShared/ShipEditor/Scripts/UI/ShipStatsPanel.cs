using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using CommonComponents.Utils;
using Zenject;
using ShipEditor.Model;
using Constructor.Model;
using GameDatabase;
using Gui.Theme;
using System.Collections.Generic;
using Constructor.Component;
using GameDatabase.Enums;

namespace ShipEditor.UI
{
	public class ShipStatsPanel : MonoBehaviour
	{
		[Inject] private readonly IShipEditorModel _shipEditor;
		[Inject] private readonly IDatabase _database;

		[SerializeField] private GameObject _panelFull;
		[SerializeField] private GameObject _panelMini;

		[SerializeField] private NameValueItem _armorPoints;
		[SerializeField] private NameValueItem _repairRate;
		[SerializeField] private NameValueItem _repairCooldown;
		[SerializeField] private NameValueItem _energy;
		[SerializeField] private NameValueItem _rechargeRate;
		[SerializeField] private NameValueItem _rechargeCooldown;
		[SerializeField] private NameValueItem _shield;
		[SerializeField] private NameValueItem _shieldRechargeRate;
		[SerializeField] private NameValueItem _shieldRechargeCooldown;
		[SerializeField] private NameValueItem _weight;
		[SerializeField] private NameValueItem _ramDamage;
		[SerializeField] private NameValueItem _damageAbsorption;
        [SerializeField] private NameValueItem _velocity;
        [SerializeField] private NameValueItem _turnRate;
        [SerializeField] private NameValueItem _forwardAccel;
        [SerializeField] private NameValueItem _angularAccel;
        [SerializeField] private NameValueItem _weaponDamage;
		[SerializeField] private NameValueItem _weaponFireRate;
		[SerializeField] private NameValueItem _weaponRange;
		[SerializeField] private NameValueItem _weaponEnergyConsumption;
		[SerializeField] private NameValueItem _droneDamageModifier;
		[SerializeField] private NameValueItem _droneDefenseModifier;
		[SerializeField] private NameValueItem _droneRangeModifier;
		[SerializeField] private NameValueItem _droneSpeedModifier;
		[SerializeField] private NameValueItem _droneTimeModifier;
		[SerializeField] private NameValueItem _energyDamageResistance;
		[SerializeField] private NameValueItem _kineticDamageResistance;
		[SerializeField] private NameValueItem _heatDamageResistance;
		[SerializeField] private GameObject _weaponsBlock;
		[SerializeField] private GameObject _dronesBlock;
		[SerializeField] private GameObject _resistanceBlock;

        [SerializeField] private ThemeColor _normalColor;
        [SerializeField] private ThemeColor _errorColor;

        [SerializeField] private Text _hitPointsSummaryText;
		[SerializeField] private Text _energySummaryText;
		[SerializeField] private Text _velocitySummaryText;
		[SerializeField] private Text _turnRateSummaryText;

		[SerializeField] private GameObject[] _hiddenForStarbases;

        private List<IComponent> _energyDependentComponents = new();
        private List<IComponent> _energyIndependentComponents = new();

        private Color NormalColor => UiTheme.Current.GetColor(_normalColor);
        private Color ErrorColor => UiTheme.Current.GetColor(_errorColor);

        private void OnEnable()
		{
			_shipEditor.Events.ComponentAdded += OnComponentAdded;
			_shipEditor.Events.ComponentRemoved += OnComponentRemoved;
			_shipEditor.Events.MultipleComponentsChanged += UpdateStats;
			_shipEditor.Events.ShipChanged += OnShipChanged;
		}

		private void OnDisable()
		{
			_shipEditor.Events.ComponentAdded -= OnComponentAdded;
			_shipEditor.Events.ComponentRemoved -= OnComponentRemoved;
			_shipEditor.Events.MultipleComponentsChanged -= UpdateStats;
			_shipEditor.Events.ShipChanged -= OnShipChanged;
		}

		private void Start()
		{
			if (_radarRange == null && _velocity != null)
			{
				_radarRange = Instantiate(_velocity, _velocity.transform.parent);
				_radarRange.name = "Preview5RadarRange";
				_radarRange.transform.SetSiblingIndex(_velocity.transform.GetSiblingIndex());
				_radarRange.Label.text = "雷达范围";
			}
			UpdateStats();
		}

		public void ShowFullStats(bool show)
		{
			_panelFull.SetActive(show);
			_panelMini.SetActive(!show);
		}

		private void OnComponentAdded(IComponentModel component) => UpdateStats();
		private void OnComponentRemoved(IComponentModel component) => UpdateStats();
		private void OnShipChanged(Constructor.Ships.IShip ship) => UpdateStats();

		private void UpdateStats()
		{
			var ship = _shipEditor.Ship.Model;
			var stats = new ShipStatsCalculator(ship.OriginalShip, _database.ShipSettings);
			stats.BaseStats = ship.Stats;

			var isStarbase = ship.ShipType == GameDatabase.Enums.ShipType.Starbase;
			foreach (var item in _hiddenForStarbases)
				item.gameObject.SetActive(!isStarbase);

            _energyDependentComponents.Clear();
            _energyIndependentComponents.Clear();
            foreach (var item in _shipEditor.InstalledComponents)
			{
				var component = item.Info.CreateComponent(ship.Layout.CellCount);
                component.Upgrades = _shipEditor.UpgradesProvider.GetComponentUpgrades(item.Info.Data);
                if (component.RequiresEnergyToInstall())
                    _energyDependentComponents.Add(component);
                else
                    _energyIndependentComponents.Add(component);
			}

            foreach (var component in _energyIndependentComponents)
                stats.EquipmentStats.AddStats(component.GetStats());
            foreach (var component in _energyDependentComponents)
            {
                var componentStats = component.GetStats();
                if (stats.EnergyRechargeRate < componentStats.EnergyConsumption)
                    stats.EquipmentStats.AddNegativeStatsOnly(componentStats);
                else
                    stats.EquipmentStats.AddStats(componentStats);
            }

            UpdateSummary(stats);
			UpdateStats(stats);
		}

		private void UpdateSummary(IShipStats stats)
		{
			_hitPointsSummaryText.text = stats.ArmorPoints.AsInteger();
			_hitPointsSummaryText.color = stats.ArmorPoints > 0 ? NormalColor : ErrorColor;
			_energySummaryText.text = stats.EnergyPoints.AsInteger() + " [" + stats.EnergyRechargeRate.AsSignedInteger() + "]";
			_energySummaryText.color = stats.EnergyRechargeRate > 0 ? NormalColor : ErrorColor;

            var engine = new EngineStats(stats.EnginePower, stats.TurnRate, stats.Weight, stats.Layout.CellCount, _database.ShipSettings);
			_velocitySummaryText.text = engine.Velocity.ToString("N1");
			_velocitySummaryText.color = stats.EnginePower > 0 ? NormalColor : ErrorColor;
			_turnRateSummaryText.text = engine.AngularVelocityInUnits.ToString("N1");
			_turnRateSummaryText.color = stats.TurnRate > 0 ? NormalColor : ErrorColor;
		}

        private void UpdateStats(IShipStats stats)
		{
			_armorPoints.gameObject.SetActive(!Mathf.Approximately(stats.ArmorPoints, 0));
            _armorPoints.Value.text = stats.ArmorPoints.AsInteger();
            _armorPoints.Color = stats.ArmorPoints > 0 ? NormalColor : ErrorColor;

            _repairRate.gameObject.SetActive(stats.ArmorRepairRate > 0);
			_repairRate.Value.text = stats.ArmorRepairRate.AsDecimal();
			_repairCooldown.gameObject.SetActive(stats.ArmorRepairRate > 0);
			_repairCooldown.Value.text = stats.ArmorRepairCooldown.AsDecimal();

			_shield.gameObject.SetActive(stats.ShieldPoints > 0);
            _shield.Value.text = stats.ShieldPoints.AsInteger();
            _shieldRechargeRate.gameObject.SetActive(stats.ShieldPoints > 0);
            _shieldRechargeRate.Value.text = stats.ShieldRechargeRate.AsDecimal();
			_shieldRechargeCooldown.gameObject.SetActive(stats.ShieldPoints > 0 && stats.ShieldRechargeRate > 0);
			_shieldRechargeCooldown.Value.text = stats.ShieldRechargeCooldown.AsDecimal();

			_weight.Value.text = Mathf.RoundToInt(stats.Weight * 1000).ToString();
			_energy.Value.text = stats.EnergyPoints.AsInteger();
			_rechargeRate.Value.text = stats.EnergyRechargeRate.AsDecimal();
			_rechargeRate.Color = stats.EnergyRechargeRate > 0 ? NormalColor : ErrorColor;
			_rechargeCooldown.gameObject.SetActive(stats.EnergyRechargeRate > 0);
			_rechargeCooldown.Value.text = stats.EnergyRechargeCooldown.AsDecimal();

            var engine = new EngineStats(stats.EnginePower, stats.TurnRate, stats.Weight, stats.Layout.CellCount, _database.ShipSettings);
            _velocity.Color = stats.EnginePower > 0 ? NormalColor : ErrorColor;
            _forwardAccel.Color = stats.EnginePower > 0 ? NormalColor : ErrorColor;
            _turnRate.Color = stats.TurnRate > 0 ? NormalColor : ErrorColor;
            _angularAccel.Color = stats.TurnRate > 0 ? NormalColor : ErrorColor;
            _velocity.Value.text = engine.Velocity.AsDecimal();
            if (_radarRange != null)
            {
                var radar = 300f + _shipEditor.InstalledComponents
                    .Where(item => item.Info.Data.Device != null && item.Info.Data.Device.Stats.DeviceClass == DeviceClass.Radar)
                    .Sum(item => item.Info.Data.Device.Stats.Power);
                _radarRange.Value.text = radar.ToString("N0");
            }
            _turnRate.Value.text = engine.AngularVelocityInUnits.AsDecimal();
            _forwardAccel.Value.text = engine.Propulsion.AsDecimal();
            _angularAccel.Value.text = engine.TurnRateInUnits.AsDecimal();

            _weaponDamage.gameObject.SetActive(stats.WeaponDamageMultiplier.HasValue);
			_weaponDamage.Value.text = stats.WeaponDamageMultiplier.ToString();
			_weaponFireRate.gameObject.SetActive(stats.WeaponFireRateMultiplier.HasValue);
			_weaponFireRate.Value.text = stats.WeaponFireRateMultiplier.ToString();
			_weaponRange.gameObject.SetActive(stats.WeaponRangeMultiplier.HasValue);
			_weaponRange.Value.text = stats.WeaponRangeMultiplier.ToString();
			_weaponEnergyConsumption.gameObject.SetActive(stats.WeaponEnergyCostMultiplier.HasValue);
			_weaponEnergyConsumption.Value.text = stats.WeaponEnergyCostMultiplier.ToString();
			_weaponsBlock.gameObject.SetActive(_weaponsBlock.transform.Cast<Transform>().Count(item => item.gameObject.activeSelf) > 1);

			_droneDamageModifier.gameObject.SetActive(stats.DroneDamageMultiplier.HasValue);
			_droneDamageModifier.Value.text = stats.DroneDamageMultiplier.ToString();
            _droneDefenseModifier.gameObject.SetActive(stats.DroneDefenseMultiplier.HasValue);
            _droneDefenseModifier.Value.text = stats.DroneDefenseMultiplier.ToString();
            _droneRangeModifier.gameObject.SetActive(stats.DroneRangeMultiplier.HasValue);
			_droneRangeModifier.Value.text = stats.DroneRangeMultiplier.ToString();
			_droneSpeedModifier.gameObject.SetActive(stats.DroneSpeedMultiplier.HasValue);
			_droneSpeedModifier.Value.text = stats.DroneSpeedMultiplier.ToString();
			_droneTimeModifier.gameObject.SetActive(stats.DroneBuildSpeed > 0);
			_droneTimeModifier.Value.text = stats.DroneBuildTime.AsDecimal();
			_dronesBlock.gameObject.SetActive(_dronesBlock.transform.Cast<Transform>().Count(item => item.gameObject.activeSelf) > 1);

			_ramDamage.gameObject.SetActive(!Mathf.Approximately(stats.RammingDamage, 0));
			_ramDamage.Value.text = stats.RammingDamageMultiplier.AsPercentageChange();
			_damageAbsorption.gameObject.SetActive(stats.EnergyAbsorption > 0);
			_damageAbsorption.Value.text = stats.EnergyAbsorptionPercentage.AsPercentage();

			_kineticDamageResistance.gameObject.SetActive(!Mathf.Approximately(stats.KineticResistance, 0));
			_kineticDamageResistance.Value.text = $"{stats.KineticResistance.AsInteger()} ( {stats.KineticResistancePercentage.AsPercentage()})";
			_heatDamageResistance.gameObject.SetActive(!Mathf.Approximately(stats.ThermalResistance, 0));
			_heatDamageResistance.Value.text = $"{stats.ThermalResistance.AsInteger()} ( {stats.ThermalResistancePercentage.AsPercentage()})";
			_energyDamageResistance.gameObject.SetActive(!Mathf.Approximately(stats.EnergyResistance, 0));
			_energyDamageResistance.Value.text = $"{stats.EnergyResistance.AsInteger()} ( {stats.EnergyResistancePercentage.AsPercentage()})";
			_resistanceBlock.gameObject.SetActive(_resistanceBlock.transform.Cast<Transform>().Count(item => item.gameObject.activeSelf) > 1);
		}

		private NameValueItem _radarRange;
	}
}
