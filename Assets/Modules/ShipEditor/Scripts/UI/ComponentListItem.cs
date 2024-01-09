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

namespace ShipEditor.UI
{
	public class ComponentListItem : ComponentListItemBase
	{
	    [Inject] private readonly ILocalization _localization;
	    [Inject] private readonly IResourceLocator _resourceLocator;
		[Inject] private readonly IShipEditorModel _shipEditor;

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
			var canInstall = model.IsSuitable(ship);

			_button.interactable = canInstall;
			_lockIcon.gameObject.SetActive(!canInstall);

			UpdateDescription(model);
		}

        public override ComponentInfo Component => _component;
		public override bool Selected { get; set; }

		public void OnDragStarted(PointerEventData eventData)
		{
			_draggableComponent.Initialize(new DraggableComponent.Content(_component), eventData);
		}

		private void UpdateDescription(IComponent component)
		{
			_name.text = _component.GetName(_localization);
		    _name.color = _component.ItemQuality.ToColor();

			_icon.sprite = _resourceLocator.GetSprite(_component.Data.Icon);
			_icon.color = _component.Data.Color;

		    var modification = component.Modification ?? EmptyModification.Instance;
		    _modification.gameObject.SetActive(!string.IsNullOrEmpty(_modification.text = modification.GetDescription(_localization)));
		    //_modification.color = _component.ItemQuality.ToColor();
		}
	}
}
