using UnityEngine;
using Constructor;
using Economy;
using Services.Localization;
using Zenject;
using CommonComponents;
using ShipEditor.Model;
using UnityEngine.Events;
using Services.Gui;
using Gui.Utils;

namespace ShipEditor.UI
{
	public class ComponentPanel : MonoBehaviour
	{
	    [Inject] private readonly ILocalization _localization;
		[Inject] private readonly IShipEditorModel _shipEditor;
		[Inject] private readonly IGuiManager _guiManager;
		[Inject] private readonly CommandList _commandList;

		[SerializeField] private ComponentItem _componentItem;
		[SerializeField] private ControlsPanel _controlsPanel;
		[SerializeField] private DragHandler _dragHandler;
		[SerializeField] private DraggableComponent _draggableComponent;
		[SerializeField] private ComponentActionPanel _actionPanel;

		[SerializeField] private UnityEvent _closeRequested;

		private IComponentModel _componentModel;
		private ComponentInfo _componentInfo;

		private void OnEnable()
		{
			_shipEditor.Events.ComponentAdded += OnComponentAdded;
			_shipEditor.Events.ComponentRemoved += OnComponentRemoved;
			_shipEditor.Events.ComponentModified += OnComponentModified;
		}

		private void OnDisable()
		{
			_shipEditor.Events.ComponentAdded -= OnComponentAdded;
			_shipEditor.Events.ComponentRemoved -= OnComponentRemoved;
			_shipEditor.Events.ComponentModified -= OnComponentModified;
		}

		public bool Visible
		{
			get => gameObject.activeSelf;
			set => gameObject.SetActive(value);
		}

		public void OnKeyBindingChanged()
		{
			if (_componentModel != null)
			{
				_shipEditor.SetComponentKeyBinding(_componentModel, _controlsPanel.KeyBinding);
				_shipEditor.SetComponentBehaviour(_componentModel, _controlsPanel.ComponentMode);
			}
		}

		public void OnDragStarted(UnityEngine.EventSystems.PointerEventData eventData)
		{
			var content = new DraggableComponent.Content(_componentInfo, _controlsPanel.KeyBinding, _controlsPanel.ComponentMode);
			_draggableComponent.Initialize(content, eventData);
		}

		public void RemoveComponent()
		{
			_commandList.TryExecute(new RemoveComponentCommand(_shipEditor, _componentModel));
		}

		public void UnlockComponent()
		{
			_guiManager.ShowBuyConfirmationDialog(_localization.GetString("$UnlockConfirmation"), 
				_shipEditor.Inventory.GetUnlockPrice(_componentInfo), () => _shipEditor.UnlockComponent(_componentModel));
		}

		public void UnlockAllComponents()
		{
			Money totalPrice = 0;

			foreach (var item in _shipEditor.InstalledComponents)
				if (_shipEditor.CanBeUnlocked(item))
					totalPrice += _shipEditor.Inventory.GetUnlockPrice(item.Info).Amount;

			var price = Price.Common(totalPrice);
			_guiManager.ShowBuyConfirmationDialog(_localization.GetString("$UnlockAllConfirmation"), price, UnlockAllComponentsInternal);
		}

		public void SetInstalledComponent(IComponentModel model)
		{
			_componentModel = model;
			_componentInfo = model.Info;
			_componentItem.Initialize(model.Info);

			var component = _componentInfo.Data;
			_controlsPanel.Initialize(component, model.KeyBinding, _shipEditor.CompatibilityChecker.GetDefaultKey(component), model.Behaviour);
			_dragHandler.gameObject.SetActive(false);

			if (!model.Locked)
				_actionPanel.Show(ComponentActionPanel.Status.CanRemove);
			else if (_shipEditor.CanBeUnlocked(_componentModel))
				_actionPanel.Show(ComponentActionPanel.Status.Locked);
			else
				_actionPanel.Show(ComponentActionPanel.Status.None);
		}

		public void SetInventoryComponent(ComponentInfo info)
		{
			_componentModel = null;
			_componentInfo = info;

			_componentItem.Initialize(info);

			var component = _componentInfo.Data;
			_controlsPanel.Initialize(component, -1, _shipEditor.CompatibilityChecker.GetDefaultKey(component), 0);

			var canInstall = _shipEditor.CompatibilityChecker.IsCompatible(component);
			var alreadyInstalled = !canInstall && _shipEditor.CompatibilityChecker.UniqueComponentInstalled(component);

			_dragHandler.gameObject.SetActive(canInstall);

			if (canInstall)
				_actionPanel.Show(ComponentActionPanel.Status.CanInstall);
			else if (alreadyInstalled)
				_actionPanel.Show(ComponentActionPanel.Status.AlreadyInstalled);
			else
				_actionPanel.Show(ComponentActionPanel.Status.NotCompatible);
		}

		private void UnlockAllComponentsInternal()
		{
			foreach (var item in _shipEditor.InstalledComponents)
				if (_shipEditor.CanBeUnlocked(item))
					_shipEditor.UnlockComponent(item);
		}

		private void OnComponentAdded(IComponentModel model)
		{
			if (_shipEditor.Inventory.Components.GetQuantity(_componentInfo) == 0)
				_closeRequested?.Invoke();
		}

		private void OnComponentRemoved(IComponentModel model)
		{
			_closeRequested?.Invoke();
		}

		private void OnComponentModified(IComponentModel model)
		{
			if (model == _componentModel)
				SetInstalledComponent(model);
		}
	}
}
