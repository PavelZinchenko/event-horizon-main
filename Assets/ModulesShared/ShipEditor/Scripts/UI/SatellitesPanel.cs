using UnityEngine;
using UnityEngine.UI;
using Gui.ComponentList;
using Zenject;
using Services.Localization;
using Services.Resources;
using GameDatabase.DataModel;
using ShipEditor.Model;
using Constructor.Satellites;

namespace ShipEditor.UI
{
	public class SatellitesPanel : MonoBehaviour
    {
		[Inject] private readonly ILocalization _localization;
		[Inject] private readonly IResourceLocator _resourceLocator;
		[Inject] private readonly IShipEditorModel _shipEditor;
		
		[SerializeField] private GroupListItem _backButton;
		[SerializeField] private LayoutGroup _itemsLayoutGroup;
		[SerializeField] private GameObject _removeSatelliteItem;

		private SatelliteLocation _location;

		private void OnEnable()
		{
			_shipEditor.Events.SatelliteChanged += OnSatelliteChanged;
			_shipEditor.Events.ShipChanged += OnShipChanged;

			UpdateContent();
		}

		private void OnDisable()
		{
			_shipEditor.Events.SatelliteChanged -= OnSatelliteChanged;
			_shipEditor.Events.ShipChanged -= OnShipChanged;
		}

		public SatelliteLocation Location
		{
			get => _location;
			set
			{
				if (_location == value) return;
				_location = value;
			}
		}

		public bool Visible
		{
			get => gameObject.activeSelf;
			set => gameObject.SetActive(value);
		}

		public void Start()
        {
			var node = new RootNode(null);
			_backButton.Initialize(node, node);
		}

		public void RemoveSatellite()
		{
			_shipEditor.RemoveSatellite(_location);
		}

		public void InstallSatellite(SatelliteItem item)
		{
			if (item.SatelliteBuild != null)
				_shipEditor.InstallSatellite(_location, item.SatelliteBuild);
			else
				_shipEditor.TryInstallSatellite(_location, item.Satellite);
		}

		private void OnShipChanged(Constructor.Ships.IShip ship)
		{
			UpdateContent();
		}

		private void OnSatelliteChanged(SatelliteLocation location)
		{
			if (location == _location)
				UpdateContent();
		}

		private void UpdateContent()
		{
			var isInstalled = _shipEditor.HasSatellite(_location);
			_removeSatelliteItem.SetActive(isInstalled);

			if (_shipEditor.Inventory.SatelliteBuilds.Count > 0)
				_itemsLayoutGroup.transform.InitializeElements<SatelliteItem, ISatellite>(
					_shipEditor.Inventory.SatelliteBuilds, UpdateSatellite);
			else
				_itemsLayoutGroup.transform.InitializeElements<SatelliteItem, Satellite>(
					_shipEditor.Inventory.Satellites, UpdateSatellite);
		}

		private void UpdateSatellite(SatelliteItem item, ISatellite satellite)
		{
			var canBeInstalled = _shipEditor.CompatibilityChecker.IsCompatible(satellite.Information);
			item.Initialize(satellite, canBeInstalled, _resourceLocator, _localization);
		}

		private void UpdateSatellite(SatelliteItem item, Satellite satellite)
		{
            var quantity = _shipEditor.Inventory.GetQuantity(satellite);
			var canBeInstalled = _shipEditor.CompatibilityChecker.IsCompatible(satellite);
			item.Initialize(satellite, quantity, canBeInstalled, _resourceLocator, _localization);
		}
	}
}
