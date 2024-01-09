using Services.Localization;
using Services.Resources;
using UnityEngine;
using UnityEngine.UI;
using GameDatabase.DataModel;
using Constructor.Satellites;

namespace ShipEditor.UI
{
    public class SatelliteItem : MonoBehaviour
    {
		[SerializeField] private Image _icon;
		[SerializeField] private Text _name;
		[SerializeField] private Text _quantity;
		[SerializeField] private Text _size;
		[SerializeField] private Text _weapons;
		[SerializeField] private Selectable _button;

		public ISatellite SatelliteBuild { get; private set; }
		public Satellite Satellite { get; private set; }

		public void Initialize(ISatellite satellite, bool canBeInstalled, IResourceLocator resourceLocator, ILocalization localization)
		{
			Initialize(satellite.Information, 1, canBeInstalled, resourceLocator, localization);
			SatelliteBuild = satellite;
			_name.text = localization.Localize(satellite.Name);
			_quantity.text = string.Empty;
		}

		public void Initialize(Satellite satellite, int quantity, bool canBeInstalled, IResourceLocator resourceLocator, ILocalization localization)
        {
			Satellite = satellite;
			_icon.sprite = resourceLocator.GetSprite(satellite.ModelImage);
			_icon.color = canBeInstalled ? Color.white : Color.gray;
			_name.text = localization.Localize(satellite.Name);
			_quantity.text = quantity.ToString();
			_size.text = satellite.Layout.CellCount.ToString();
			_weapons.text = satellite.Barrels.Count > 0 ? satellite.Barrels[0].WeaponClass : "-";
			_button.interactable = canBeInstalled;
		}
	}
}
