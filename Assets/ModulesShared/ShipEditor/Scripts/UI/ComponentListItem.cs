using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Zenject;
using Constructor;
using Constructor.Modification;
using Economy.ItemType;
using Gui.ComponentList;
using Services.Localization;
using Services.Resources;
using Constructor.Component;
using ShipEditor.Model;
using GameDatabase;

namespace ShipEditor.UI
{
	public class ComponentListItem : ComponentListItemBase
	{
	    [Inject] private readonly ILocalization _localization;
		[Inject] private readonly IResourceLocator _resourceLocator;
		[Inject] private readonly IShipEditorModel _shipEditor;
		[Inject] private readonly IDatabase _database;

		[SerializeField] private DraggableComponent _draggableComponent;
        [SerializeField] private Button _button;
		[SerializeField] private Image _icon;
		[SerializeField] private Image _lockIcon;
		[SerializeField] private Text _name;
        [SerializeField] private Text _modification;
        [SerializeField] private Text _quantity;

		private ComponentInfo _component;

		public override void Initialize(ComponentInfo data, int quantity)
		{
            _quantity.gameObject.SetActive(quantity > 0);
			_quantity.text = quantity.ToString();

            if (_component == data)
				return;

			_component = data;
			var ship = _shipEditor.Ship.Model;
            var model = _component.CreateComponent(ship.Layout.CellCount);
            model.Upgrades = _shipEditor.UpgradesProvider.GetComponentUpgrades(_component.Data);
			var canInstall = model.IsSuitable(ship);

			_button.interactable = canInstall;
			_lockIcon.gameObject.SetActive(!canInstall);

			UpdateDescription(model);
		}

        public override ComponentInfo Component => _component;
		public override bool Selected { get; set; }

		public void OnDragStarted(PointerEventData eventData)
		{
			var persistedBarrelId = int.MinValue;
			var behaviour = 0;
			if (_component.Data.Id.Value == ThreeBodyContentRules.CreativeWorkshopComponentId)
				ThreeBodyContentRules.TryGetCreativeWorkshopSelectionSettings(_database, out persistedBarrelId, out behaviour);

			_draggableComponent.Initialize(
				new DraggableComponent.Content(_component, 0, behaviour, persistedBarrelId), eventData);
		}

		private void UpdateDescription(IComponent component)
		{
			_name.text = _component.GetName(_localization);
		    _name.color = Gui.Theme.UiTheme.Current.GetQualityColor(_component.ItemQuality);

			_icon.sprite = _resourceLocator.GetSprite(_component.Data.Icon);
			_icon.color = _component.Data.Color;
			_icon.preserveAspect = true;

		    var modification = component.Modification ?? EmptyModification.Instance;
		    _modification.gameObject.SetActive(!string.IsNullOrEmpty(_modification.text = modification.GetDescription(_localization)));
		    //_modification.color = _component.ItemQuality.ToColor();
		}
	}
}
