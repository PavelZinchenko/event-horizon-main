using UnityEngine;
using Constructor;
using Gui.ComponentList;
using UnityEngine.Events;
using Constructor.Ships;
using ShipEditor.Model;
using Zenject;

namespace ShipEditor.UI
{
    public class ComponentsPanel : MonoBehaviour
    {
		[Inject] private readonly IShipEditorModel _shipEditor;

		[SerializeField] private ComponentContentFiller _contentFiller;
        [SerializeField] private ListScrollRect _componentList;
        [SerializeField] private GameObject _noItemsText;

		[SerializeField] private UnityEvent _leftSatelliteGroupSelected;
		[SerializeField] private UnityEvent _rightSatelliteGroupSelected;
		[SerializeField] private UnityEvent<ComponentInfo> _componentSelected;

		private RootNode _rootNode;
		private SatellitesNode _leftSatelliteNode;
		private SatellitesNode _rightSatelliteNode;
		private IComponentTreeNode _selectedNode;

		private void Start()
		{
			var components = _shipEditor.Inventory.Components;
			_rootNode = new RootNode(new ComponentQuantityProvider(components));
			_rootNode.Assign(components);
			_rootNode.IsVisible = true;
			_leftSatelliteNode = new SatellitesNode(_rootNode, SatellitesNode.Location.Left);
			_rightSatelliteNode = new SatellitesNode(_rootNode, SatellitesNode.Location.Right);
			_rootNode.AddNode(_leftSatelliteNode, true);
			_rootNode.AddNode(_rightSatelliteNode, true);
			_selectedNode = _rootNode;
			_noItemsText.SetActive(components.Count == 0);
			UpdateSatelliteGroups();
			_contentFiller.InitializeItems(_selectedNode);
			_componentList.RefreshContent();
		}

		private void OnEnable()
		{
			_shipEditor.Events.ShipChanged += OnShipSelected;
			_shipEditor.Events.SatelliteChanged += OnSatelliteChanged;
			_shipEditor.Events.ComponentAdded += OnComponentAddedOrRemoved;
			_shipEditor.Events.ComponentRemoved += OnComponentAddedOrRemoved;
			_shipEditor.Events.MultipleComponentsChanged += RefreshList;
		}

		private void OnDisable()
		{
			_shipEditor.Events.ShipChanged -= OnShipSelected;
			_shipEditor.Events.SatelliteChanged -= OnSatelliteChanged;
			_shipEditor.Events.ComponentAdded -= OnComponentAddedOrRemoved;
			_shipEditor.Events.ComponentRemoved -= OnComponentAddedOrRemoved;
			_shipEditor.Events.MultipleComponentsChanged -= RefreshList;
		}

		private void OnComponentAddedOrRemoved(IComponentModel component, ShipElementType elementType) => RefreshList();

		public bool Visible
		{
			get => gameObject.activeSelf;
			set 
			{
				if (Visible == value) return;
				gameObject.SetActive(value);
				if (value) RefreshList();
			}
		}

		public void GoBack()
		{
			_selectedNode = _selectedNode.GetVisibleParent() ?? _rootNode;
			RefreshList();
		}

		public void ShowAll()
		{
			_selectedNode = _rootNode;
			RefreshList();
		}

		public void OnItemSelected(ComponentListItem item)
		{
			_componentSelected?.Invoke(item.Component);
		}

		public void OnGroupSelected(GroupListItem item)
        {
            if (item.Node == _leftSatelliteNode)
            {
                _leftSatelliteGroupSelected?.Invoke();
                return;
            }

			if (item.Node == _rightSatelliteNode)
			{
				_rightSatelliteGroupSelected?.Invoke();
				return;
			}

            if (_selectedNode == item.Node)
                _selectedNode = _selectedNode.GetVisibleParent() ?? _rootNode;
            else
                _selectedNode = item.Node;

            RefreshList();
        }

        private void RefreshList()
        {
			if (_rootNode == null) return;
            _rootNode.Assign(_shipEditor.Inventory.Components);

			while (_selectedNode != _rootNode && _selectedNode.ItemCount == 0)
                _selectedNode = _selectedNode.GetVisibleParent() ?? _rootNode;

			UpdateSatelliteGroups();
			_contentFiller.InitializeItems(_selectedNode);
            _componentList.RefreshContent();
		}

		private void OnShipSelected(IShip ship) => UpdateSatelliteGroups();

		private void OnSatelliteChanged(SatelliteLocation location) => UpdateSatelliteGroups();

		private void UpdateSatelliteGroups()
		{
			var isStarbase = _shipEditor.Ship.Model.ShipType == GameDatabase.Enums.ShipType.Starbase;
			var haveSatellites = _shipEditor.Inventory.Satellites.Count > 0;

			_leftSatelliteNode.IsVisible = !isStarbase && (haveSatellites || _shipEditor.HasSatellite(SatelliteLocation.Left));
			_rightSatelliteNode.IsVisible = !isStarbase && (haveSatellites || _shipEditor.HasSatellite(SatelliteLocation.Left));
		}
	}
}
