using System.Linq;
using UnityEngine;
using ShipEditor.Model;
using Zenject;
using Constructor.Ships;
using Constructor;
using Services.Gui;
using Gui.Utils;
using Services.Audio;
using Services.Localization;

namespace ShipEditor.UI
{
	public class ShipEditorWindow : MonoBehaviour
	{
		public enum PanelType
		{
			ShipList,
			ComponentList,
			SatelliteList,
			Component,
		}

		[Inject] private readonly IShipEditorModel _shipEditor;
		[Inject] private readonly IGuiManager _guiManager;
		[Inject] private readonly ILocalization _localization;
		[Inject] private readonly CommandList _commandList;
		[Inject] private readonly ISoundPlayer _soundPlayer;
		[InjectOptional] private readonly CloseEditorSignal.Trigger _closeEditorTrigger;

		[SerializeField] private float CameraZoomMin = 5;
		[SerializeField] private float CameraZoomMax = 30;

		[SerializeField] private ShipView _shipView;

		[SerializeField] private RectTransform _editorWindow;
		[SerializeField] private ComponentsPanel _componentListPanel;
		[SerializeField] private ShipsPanel _shipListPanel;
		[SerializeField] private SatellitesPanel _satelliteListPanel;
		[SerializeField] private ComponentPanel _componentPanel;
		[SerializeField] private DraggableComponent _draggableComponent;
		[SerializeField] private UnityEngine.UI.Button _undoButton;
		[SerializeField] private UnityEngine.UI.InputField _shipNameInputField;
		[SerializeField] private AudioClip _installSound;

		private string _shipInitialName;

		private void OnEnable()
		{
			_commandList.DataChanged += OnUndoListChanged;
			_shipEditor.Events.ShipChanged += OnShipChanged;
			_shipEditor.Events.SatelliteChanged += OnSatelliteChanged;
			_shipEditor.Events.ComponentAdded += OnComponentAdded;
			_shipEditor.Events.ComponentRemoved += OnComponentRemoved;
			_shipEditor.Events.ComponentModified += OnComponentModified;
			_shipEditor.Events.MultipleComponentsChanged += OnMultipleComponentsChanged;
		}

		private void OnDisable()
		{
			_commandList.DataChanged -= OnUndoListChanged;
			_shipEditor.Events.ShipChanged -= OnShipChanged;
			_shipEditor.Events.SatelliteChanged -= OnSatelliteChanged;
			_shipEditor.Events.ComponentAdded -= OnComponentAdded;
			_shipEditor.Events.ComponentRemoved -= OnComponentRemoved;
			_shipEditor.Events.ComponentModified -= OnComponentModified;
			_shipEditor.Events.MultipleComponentsChanged -= OnMultipleComponentsChanged;
		}

		private void Start()
		{
			OnShipChanged(_shipEditor.Ship);
			OnSatelliteChanged(SatelliteLocation.Left);
			OnSatelliteChanged(SatelliteLocation.Right);
			OpenComponentList();
		}

		public void OpenShipList() => ShowPanel(PanelType.ShipList);

		public void BackButtonPressed()
		{
			if (!_componentListPanel.Visible)
				ShowPanel(PanelType.ComponentList);
			else
				_componentListPanel.GoBack();
		}

		public void OpenComponentList()
		{
			ShowPanel(PanelType.ComponentList);
			_componentListPanel.ShowAll();
		}

		public void OpenComponentPanel(ComponentInfo component)
		{
			_componentPanel.SetInventoryComponent(component);
			ShowPanel(PanelType.Component);
		}

		public void OpenEditComponentPanel(IComponentModel component)
		{
			_componentPanel.SetInstalledComponent(component);
			ShowPanel(PanelType.Component);
		}

		public void OpenLeftSatellitePanel() => OpenSatellitePanel(SatelliteLocation.Left);
		public void OpenRightSatellitePanel() => OpenSatellitePanel(SatelliteLocation.Right);

		public void OpenSatellitePanel(SatelliteLocation satelliteLocation)
		{
			_satelliteListPanel.Location = satelliteLocation;
			ShowPanel(PanelType.SatelliteList);
		}

		public void ShowPlacement(DraggableComponent.Content item, Vector2 position)
		{
			_shipView.ShowSelection(position, item.Component.Data);
		}

		public void DropComponent(DraggableComponent.Content item, Vector2 position)
		{
			_shipView.ShowSelection(Vector2.zero, null);

			if (!_commandList.TryExecute(CreateInstallCommand(ShipElementType.Ship, item, position)))
				if (!_commandList.TryExecute(CreateInstallCommand(ShipElementType.SatelliteL, item, position)))
					if (!_commandList.TryExecute(CreateInstallCommand(ShipElementType.SatelliteR, item, position)))
						return;
		}

		public void Undo()
		{
			_commandList.Undo();
		}

		public void Exit()
		{
			_closeEditorTrigger?.Fire();
		}

		public void OnUndoListChanged()
		{
			_undoButton.interactable = !_commandList.IsEmpty;
		}

		public void RemoveAll()
		{
			if (_shipEditor.InstalledComponents.Any(item => !item.Locked))
				_guiManager.ShowConfirmationDialog(_localization.GetString("$RemoveAllConfirmation"), _shipEditor.RemoveAllComponents);

			if (_componentPanel.Visible || _satelliteListPanel.Visible)
				ShowPanel(PanelType.ComponentList);
		}

		public void OnClick(Vector2 position)
		{
			if (TrySelectComponent(position, out var component, out var elementType))
				OpenEditComponentPanel(component);
		}

		public void OnMove(Vector2 offset)
		{
			var position = _shipView.transform.localPosition + (Vector3)offset;
			position.x = Mathf.Clamp(position.x, -_shipView.Width / 2, _shipView.Width / 2);
			position.y = Mathf.Clamp(position.y, -_shipView.Height / 2, _shipView.Height / 2);
			_shipView.transform.localPosition = position;
		}

		public void OnDrag(UnityEngine.EventSystems.PointerEventData eventData)
		{
			var position = Camera.main.ScreenToWorldPoint(eventData.pressPosition);
			if (!TrySelectComponent(position, out var component, out var elementType)) return;
			if (component.Locked) return;

			var command = new RemoveComponentCommand(_shipEditor, component);
			if (_commandList.TryExecute(command))
				_draggableComponent.Initialize(new DraggableComponent.Content(component.Info, component.KeyBinding, component.Behaviour), eventData);
		}

		public void OnZoom(float zoom)
		{
			Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize * zoom, CameraZoomMin, CameraZoomMax);
		}

		public void OnNameChanged(string name)
		{
			_shipEditor.ShipName = name != _shipInitialName ? name : null;
		}

		private void ShowPanel(PanelType panel)
		{
			_shipListPanel.Visible = panel == PanelType.ShipList;
			_satelliteListPanel.Visible = panel == PanelType.SatelliteList;
			_componentListPanel.Visible = panel == PanelType.ComponentList;
			_componentPanel.Visible = panel == PanelType.Component;
		}

		private void OnShipChanged(IShip ship)
		{
			_shipView.InitializeShip(_shipEditor.Layout(ShipElementType.Ship));
			_shipView.transform.localPosition = new Vector3(0, 0, _shipView.transform.localPosition.z);
			_commandList.Clear();
			_shipInitialName = _localization.GetString(_shipEditor.ShipName);
			_shipNameInputField.text = _shipInitialName;
		}

		private void OnComponentAdded(IComponentModel component, ShipElementType shipElement)
		{
			_shipView.AddComponent(component, shipElement);
			_soundPlayer.Play(_installSound);
		}

		private void OnComponentRemoved(IComponentModel component, ShipElementType shipElement)
		{
			_shipView.RemoveComponent(component, shipElement);
		}

		private void OnComponentModified(IComponentModel component, ShipElementType shipElement)
		{
			_shipView.UpdateComponent(component, shipElement);
		}

		private void OnSatelliteChanged(SatelliteLocation location)
		{
			var layout = _shipEditor.Layout(location);
			_shipView.InitializeSatellite(layout, location);
			_commandList.Clear(location == SatelliteLocation.Left ? ShipElementType.SatelliteL : ShipElementType.SatelliteR);
		}

		private void OnMultipleComponentsChanged()
		{
			_shipView.ReloadAllComponents(ShipElementType.Ship);
			_shipView.ReloadAllComponents(ShipElementType.SatelliteL);
			_shipView.ReloadAllComponents(ShipElementType.SatelliteR);
			_commandList.Clear();
		}

		private bool TrySelectComponent(Vector2 position, out IComponentModel component, out ShipElementType elementType)
		{
			elementType = ShipElementType.Ship;
			if (TrySelectComponent(position, elementType, out component))
				return true;

			elementType = ShipElementType.SatelliteL;
			if (TrySelectComponent(position, elementType, out component))
				return true;

			elementType = ShipElementType.SatelliteR;
			if (TrySelectComponent(position, elementType, out component))
				return true;

			return false;
		}

		private bool TrySelectComponent(Vector2 position, ShipElementType elementType, out IComponentModel component)
		{
			var layout = _shipEditor.Layout(elementType);
			if (layout == null)
			{
				component = null;
				return false;
			}

			var cell = _shipView.WorldToCell(position, elementType, 1);
			return layout.TryGetComponentAt(cell.x, cell.y, out component);
		}

		private ICommand CreateInstallCommand(ShipElementType shipElementType, DraggableComponent.Content item, Vector2 position)
		{
			var cell = _shipView.WorldToCell(position, shipElementType, item.Layout.Size);
			var settings = new ComponentSettings(item.KeyBinding, item.Behaviour, false);
			return new InstallComponentCommand(_shipEditor, shipElementType, cell, item.Component, settings);
		}
	}
}
