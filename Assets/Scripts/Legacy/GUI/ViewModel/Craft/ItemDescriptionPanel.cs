using System.Collections.Generic;
using System.Linq;
using Constructor;
using Economy.ItemType;
using Economy.Products;
using UnityEngine;
using UnityEngine.UI;
using Services.Localization;
using Services.ObjectPool;
using Constructor.Ships;
using GameDatabase;
using GameDatabase.DataModel;
using GameDatabase.Enums;
using GameDatabase.Serializable;
using Services.Resources;
using Zenject;
using Gui.Theme;

namespace ViewModel.Craft
{
    public class ItemDescriptionPanel : MonoBehaviour
    {
        [Inject] private readonly ILocalization _localization;
        [Inject] private readonly IGameObjectFactory _factory;
        [Inject] private readonly IResourceLocator _resourceLocator;
        [Inject] private readonly IDatabase _database;

        [SerializeField] Image _icon;
        [SerializeField] Text _name;
        [SerializeField] Text _description;
        [SerializeField] Text _modification;
        [SerializeField] LayoutGroup _stats;
        [SerializeField] LayoutGroup _weaponSlots;

        [SerializeField] Sprite _emptyIcon;

        public void Initialize(IProduct item)
        {
            if (item == null)
            {
                CreateEmpty();
            }
            else if (item.Type is ComponentItem)
            {
                CreateComponent(((ComponentItem)item.Type).Component);
            }
            else if (item.Type is ShipItemBase)
            {
                CreateShip(((ShipItemBase)item.Type).Ship);
            }
            else if (item.Type is SatelliteItem)
            {
                CreateSatellite(((SatelliteItem)item.Type).Satellite);
            }
            else
            {
                CreateDefault(item.Type);
            }
        }

        private void CreateShip(IShip ship)
        {
            _icon.sprite = _resourceLocator.GetSprite(ship.Model.ModelImage);
            _icon.color = Color.white;
            _name.text = _localization.GetString(ship.Name);
            _name.color = Gui.Theme.UiTheme.Current.GetQualityColor(ship.Model.Quality());
            _description.gameObject.SetActive(false);

            _modification.gameObject.SetActive(ship.Model.Modifications.Any());
            _modification.text = string.Join("\n", ship.Model.Modifications.Select(item => item.GetDescription(_localization)).ToArray());

            _stats.gameObject.SetActive(true);
            _stats.transform.InitializeElements<TextFieldViewModel, KeyValuePair<string, string>>(GetShipDescription(ship, _localization), UpdateTextField);
			UpdateWeaponSlots(ship.Model.Barrels);
        }

        private void CreateSatellite(Satellite satellite)
        {
            _icon.sprite = _resourceLocator.GetSprite(satellite.ModelImage);
            _icon.color = Color.white;
            _name.text = _localization.GetString(satellite.Name);
            _name.color = Gui.Theme.UiTheme.Current.GetQualityColor(ItemQuality.Common);
            _description.gameObject.SetActive(false);

            _modification.gameObject.SetActive(false);

            _stats.gameObject.SetActive(true);
            _stats.transform.InitializeElements<TextFieldViewModel, KeyValuePair<string, string>>(GetSatelliteDescription(satellite), UpdateTextField);
			UpdateWeaponSlots(satellite.Barrels);
        }

        private void CreateComponent(ComponentInfo info)
        {
            _icon.sprite = _resourceLocator.GetSprite(info.Data.Icon);
            _icon.color = info.Data.Color;
            _name.text = _localization.GetString(info.Data.Name);
            _name.color = UiTheme.Current.GetQualityColor(info.ItemQuality);
            _description.gameObject.SetActive(false);

            var component = info.CreateComponent(100);

            var modification = component.Modification ?? Constructor.Modification.EmptyModification.Instance;
            _modification.gameObject.SetActive(!string.IsNullOrEmpty(_modification.text = modification.GetDescription(_localization)));
            _modification.color = UiTheme.Current.GetQualityColor(info.ItemQuality);

            _stats.gameObject.SetActive(true);
            _stats.transform.InitializeElements<TextFieldViewModel, KeyValuePair<string, string>>(ShipEditor.UI.ComponentItem.GetDescription(component, _localization), UpdateTextField, _factory);
            _weaponSlots.gameObject.SetActive(false);
        }

        private void CreateEmpty()
        {
            _icon.sprite = _emptyIcon;
            _icon.color = Color.white;
            _name.text = string.Empty;
            _description.gameObject.SetActive(false);
            _stats.gameObject.SetActive(false);
            _weaponSlots.gameObject.SetActive(false);
            _modification.gameObject.SetActive(false);
        }

        private void CreateDefault(IItemType item)
        {
            _icon.sprite = _resourceLocator.GetSprite(item.Icon);
            _icon.color = item.Color;
            _name.text = item.Name;
            _name.color = UiTheme.Current.GetQualityColor(item.Quality);

            var description = item.Description;
            _description.gameObject.SetActive(!string.IsNullOrEmpty(description));
            _description.text = item.Description;

            _stats.gameObject.SetActive(false);
            _weaponSlots.gameObject.SetActive(false);
            _modification.gameObject.SetActive(false);
        }

		private void UpdateWeaponSlots(IReadOnlyCollection<Barrel> barrels)
		{
			HashSet<char> types = new(barrels.SelectMany(barrel => string.IsNullOrEmpty(barrel.WeaponClass) ? "•" : barrel.WeaponClass));

			_weaponSlots.gameObject.SetActive(barrels.Count > 0);
			_weaponSlots.transform.InitializeElements<BlockViewModel, char>(types, UpdateWeaponSlot);
		}

		private void UpdateTextField(TextFieldViewModel viewModel, KeyValuePair<string, string> data)
        {
            viewModel.Label.text = _localization.GetString(data.Key);
            viewModel.Value.text = data.Value;
        }

        private static void UpdateWeaponSlot(BlockViewModel viewModel, char type)
        {
            viewModel.Label.text = type.ToString();
        }

        private static IEnumerable<KeyValuePair<string, string>> GetShipDescription(IShip ship, ILocalization localization)
        {
            var size = ship.Model.Layout.CellCount;
            yield return new KeyValuePair<string, string>("$CellCount", size.ToString());
            yield return new KeyValuePair<string, string>("$EngineSize", ship.Model.Layout.Data.Count(value => value == (char)CellType.Engine).ToString());

            var data = ship.Model.OriginalShip;
            var kineticResistance = CalculateResistance(data.Features.KineticResistance);
            var heatResistance = CalculateResistance(data.Features.HeatResistance);
            var energyResistance = CalculateResistance(data.Features.EnergyResistance);
            var baseWeightBonus = data.Features.ShipWeightBonus;
            var equipmentWeightBonus = data.Features.EquipmentWeightBonus;
            var armorBonus = data.Features.ArmorBonus;
            var shieldBonus = data.Features.ShieldBonus;
            var energyBonus = data.Features.EnergyBonus;
            var velocityBonus = data.Features.VelocityBonus;
            var turnRateBonus = data.Features.TurnRateBonus;
            var regeneration = data.Features.Regeneration;

            if (kineticResistance != 0)
                yield return new KeyValuePair<string, string>("$KineticDamageResistance", kineticResistance + "%");
            if (heatResistance != 0)
                yield return new KeyValuePair<string, string>("$ThermalDamageResistance", heatResistance + "%");
            if (energyResistance != 0)
                yield return new KeyValuePair<string, string>("$EnergyDamageResistance", energyResistance + "%");
            if (regeneration)
                yield return new KeyValuePair<string, string>("$RepairRate", localization.GetString("$ValuePerSecond", "1%"));
            if (baseWeightBonus != 0)
                yield return new KeyValuePair<string, string>("$Weight", SignedPercent(baseWeightBonus));
            if (equipmentWeightBonus != 0)
                yield return new KeyValuePair<string, string>("$EquipmentWeight", SignedPercent(equipmentWeightBonus));
            if (velocityBonus != 0)
                yield return new KeyValuePair<string, string>("$Velocity", SignedPercent(velocityBonus));
            if (turnRateBonus != 0)
                yield return new KeyValuePair<string, string>("$TurnRate", SignedPercent(turnRateBonus));
            if (armorBonus != 0)
                yield return new KeyValuePair<string, string>("$Armor", SignedPercent(armorBonus));
            if (shieldBonus != 0)
                yield return new KeyValuePair<string, string>("$Shield", SignedPercent(shieldBonus));
            if (energyBonus != 0)
                yield return new KeyValuePair<string, string>("$Energy", SignedPercent(energyBonus));

            foreach (var item in data.Features.BuiltinDevices)
                yield return new KeyValuePair<string, string>("$Device", GetDeviceName(item.Stats.DeviceClass, localization));
        }

        private static IEnumerable<KeyValuePair<string, string>> GetSatelliteDescription(Satellite satellite)
        {
            var size = satellite.Layout.CellCount;
            yield return new KeyValuePair<string, string>("$CellCount", size.ToString());

            var engineSize = satellite.Layout.Data.Count(value => value == (char)CellType.Engine);
            if (engineSize > 0)
                yield return new KeyValuePair<string, string>("$EngineSize", satellite.Layout.Data.Count(value => value == (char)CellType.Engine).ToString());
        }

        private static int CalculateResistance(float value)
        {
            return Mathf.FloorToInt(100 * value / (value + 1));
        }

        private static string SignedPercent(float value)
        {
            var sb = new System.Text.StringBuilder();
            var percent = Mathf.RoundToInt(value * 100);
            if (percent > 0) sb.Append('+');
            sb.Append(percent);
            sb.Append('%');
            return sb.ToString();
        }

        private static string GetDeviceName(DeviceClass deviceClass, ILocalization localization)
        {
            switch (deviceClass)
            {
                case DeviceClass.TimeMachine:
                    return localization.GetString("$InfinityStone_S");
                case DeviceClass.ToxicWaste:
                    return localization.GetString("$ToxicWaste");
                case DeviceClass.WormTail:
                    return localization.GetString("$Wormtail");
                case DeviceClass.DroneCamouflage:
                    return localization.GetString("$DroneCamouflage");
                case DeviceClass.MissileCamouflage:
                    return localization.GetString("$MissileCamouflage");
                case DeviceClass.RepairBot:
                    return localization.GetString("$RepairBot_M");
                default:
                    return deviceClass.ToString();
            }
        }
    }
}
