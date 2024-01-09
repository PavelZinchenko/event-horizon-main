using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using ShipEditor.Model;
using Zenject;
using Constructor.Ships;
using Constructor;
using Services.Gui;
using Gui.Utils;
using Services.Audio;
using Services.Localization;
using Services.Resources;

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
		[Inject] private readonly IResourceLocator _resourceLocator;
		[InjectOptional] private readonly CloseEditorSignal.Trigger _closeEditorTrigger;

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

		[SerializeField] private float _cameraZoomMin = 3;
		[SerializeField] private float _cameraZoomMax = 25;
		[SerializeField] private float _cameraMargins = 0.1f;
		[SerializeField] private CameraController _camera;
		[SerializeField] private RectTransform _cameraFocusDefault;
		[SerializeField] private RectTransform _cameraFocusCenter;
		[SerializeField] private UnityEvent _overviewModeEnabled;
		[SerializeField] private UnityEvent _overviewModeDisabled;

		private string _shipInitialName;
		private bool _overviewMode;

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
			ZoomToShip();
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
			if (_overviewMode)
			{
				SetOverviewMode(false);
				return;
			}

			_closeEditorTrigger?.Fire();
		}

		public void OnUndoListChanged()
		{
			_undoButton.interactable = !_commandList.IsEmpty;
		}

		public void RemoveAll()
		{
			if (_shipEditor.InstalledComponents.Any(item => !item.Locked))
				_guiManager.ShowConfirmationDialog(_localization.GetString("$RemoveAllConfirmation"), RemoveAllCompoents);
		}

		public void OnClick(Vector2 position)
		{
			if (_overviewMode) return;

			if (TrySelectComponent(position, out var component, out var elementType))
				OpenEditComponentPanel(component);
		}

		public void OnMove(Vector2 offset)
		{
			var position = _camera.Position - offset;
			position.x = Mathf.Clamp(position.x, -_shipView.Width / 2, _shipView.Width / 2);
			position.y = Mathf.Clamp(position.y, -_shipView.Height / 2, _shipView.Height / 2);
			_camera.Position = position;
		}

		public void OnDrag(UnityEngine.EventSystems.PointerEventData eventData)
		{
			if (_overviewMode) return;

			var position = Camera.main.ScreenToWorldPoint(eventData.pressPosition);
			if (!TrySelectComponent(position, out var component, out var elementType)) return;
			if (component.Locked) return;

			var command = new RemoveComponentCommand(_shipEditor, component);
			if (_commandList.TryExecute(command))
				_draggableComponent.Initialize(new DraggableComponent.Content(component.Info, component.KeyBinding, component.Behaviour), eventData);
		}

		public void OnZoom(float zoom)
		{
			var cameraZoomMax = _overviewMode ? GetBestCameraZoom() : _cameraZoomMax;
			_camera.OrthographicSize = Mathf.Clamp(_camera.OrthographicSize * zoom, _cameraZoomMin, cameraZoomMax);
		}

		public void OnNameChanged(string name)
		{
			_shipEditor.ShipName = name != _shipInitialName ? name : null;
		}

		public void SetOverviewMode(bool enabled)
		{
			if (_overviewMode == enabled) return;
			_overviewMode = enabled;

			_camera.Focus = _overviewMode ? _cameraFocusCenter : _cameraFocusDefault;
			_camera.Position = Vector2.zero;
			ZoomToShip();

			if (_overviewMode)
				_overviewModeEnabled?.Invoke();
			else
				_overviewModeDisabled?.Invoke();
		}

		private void RemoveAllCompoents()
		{
			_shipEditor.RemoveAllComponents();

			if (_componentPanel.Visible || _satelliteListPanel.Visible)
				ShowPanel(PanelType.ComponentList);
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
			var sprite = _resourceLocator.GetSprite(ship.Model.ModelImage);
			_shipView.InitializeShip(_shipEditor.Layout(ShipElementType.Ship), sprite);
			_shipInitialName = _localization.GetString(_shipEditor.ShipName);
			_shipNameInputField.text = _shipInitialName;
			_commandList.Clear();
			_camera.Position = Vector2.zero;
			ZoomToShip();
		}

		private void ZoomToShip()
		{
			var cameraZoom = GetBestCameraZoom();
			var cameraZoomMax = _overviewMode ? cameraZoom : _cameraZoomMax;
			_camera.OrthographicSize = Mathf.Clamp(cameraZoom, _cameraZoomMin, cameraZoomMax);
		}

		private float GetBestCameraZoom()
		{
			var zoom = Mathf.Max(_shipView.Height, _shipView.Width / _camera.AspectFromFocus) / 2;
			return zoom + zoom * _cameraMargins;
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
			if (layout == null)
				_shipView.RemoveSatellite(location);
			else
				_shipView.InitializeSatellite(location, layout, _resourceLocator.GetSprite(_shipEditor.Satellite(location).ModelImage));

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
