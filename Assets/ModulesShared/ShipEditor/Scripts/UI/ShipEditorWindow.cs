using System.Linq;
using System.Collections;
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
using GameDatabase;

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
            BuildList,
        }

        [Inject] private readonly IShipEditorModel _shipEditor;
		[Inject] private readonly IGuiManager _guiManager;
		[Inject] private readonly ILocalization _localization;
		[Inject] private readonly CommandList _commandList;
		[Inject] private readonly ISoundPlayer _soundPlayer;
		[Inject] private readonly IResourceLocator _resourceLocator;
		[Inject] private readonly IDatabase _database;
		[InjectOptional] private readonly CloseEditorSignal.Trigger _closeEditorTrigger;

		[SerializeField] private ShipView _shipView;

        [SerializeField] private RectTransform _editorWindow;
		[SerializeField] private ComponentsPanel _componentListPanel;
		[SerializeField] private ShipsPanel _shipListPanel;
		[SerializeField] private SatellitesPanel _satelliteListPanel;
		[SerializeField] private ComponentPanel _componentPanel;
        [SerializeField] private BuildsPanel _buildsPanel;
        [SerializeField] private DraggableComponent _draggableComponent;
		[SerializeField] private UnityEngine.UI.Button _undoButton;
        [SerializeField] private UnityEngine.UI.Button _backButton;
        [SerializeField] private UnityEngine.UI.InputField _shipNameInputField;
        [SerializeField] private GameObject _shipsButton;
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
		private UnityEngine.UI.Button _paintButton;
		private UnityEngine.UI.Button _restoreArtworkButton;
		private GameObject _artworkToolbar;

		public int CurrentShipId => _shipEditor?.Ship?.Model?.OriginalShip?.Id.Value ?? 0;
		public GameDatabase.DataModel.UiSettings UiSettings => _database?.UiSettings;
		public Sprite OriginalShipSprite
		{
			get
			{
				if (_shipEditor?.Ship == null) return null;
				var sprite = _resourceLocator.GetSprite(_shipEditor.Ship.Model.ModelImage);
				if (sprite != null) return sprite;
				return _resourceLocator.GetSprite(_shipEditor.Ship.Model.OriginalShip.IconImage);
			}
		}
		public Sprite CurrentShipSprite
		{
			get
			{
				if (_shipEditor == null || _shipEditor.Ship == null)
					return null;
				var fallback = OriginalShipSprite;
				return PlayerShipTextureOverrides.Get(CurrentShipId, fallback);
			}
		}

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

		private IEnumerator Start()
		{
            _shipsButton.SetActive(_shipEditor.Inventory.Ships.Any());
            _shipNameInputField.interactable = _shipEditor.IsShipNameEditable;
            UpdateBackButton();
			EnsureArtworkButtons();

            yield return new WaitForEndOfFrame();

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

        public void OpenBuildList()
        {
            ShowPanel(PanelType.BuildList);
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
			_shipView.ShowSelection(position, item);
		}

		public void DropComponent(DraggableComponent.Content item, Vector2 position)
		{
			_shipView.ClearSelection();

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

            _shipEditor.SaveShip();
			_closeEditorTrigger?.Fire();
		}

		public void OpenPaintCustomization() => OpenTextureCustomization();

		public void RestoreCustomArtwork()
		{
			if (CurrentShipId <= 0) return;
			PlayerShipTextureOverrides.Restore(CurrentShipId);
			RefreshShipArtwork();
		}

		public void RefreshShipArtwork()
		{
			if (_shipEditor?.Ship == null) return;
			var fallback = OriginalShipSprite;
			var sprite = PlayerShipTextureOverrides.Get(CurrentShipId, fallback);
			_shipView.InitializeShip(_shipEditor.Layout(ShipElementType.Ship), sprite);
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

			if (TrySelectComponent(position, out var component))
				OpenEditComponentPanel(component);
		}

		public void OnMove(Vector2 offset)
		{
			var cameraPosition = _camera.Position - offset;
            var shipPosition = _shipView.Position;
            var boundingRect = GetShipBoundingRect();
            var position = RotationHelpers.Transform(cameraPosition - shipPosition, -_camera.Rotation);
            position.x = Mathf.Clamp(position.x, -boundingRect.x/2, boundingRect.x/2);
            position.y = Mathf.Clamp(position.y, -boundingRect.y/2, boundingRect.y/2);
            _camera.Position = RotationHelpers.Transform(position, _camera.Rotation) + shipPosition;
		}

		public void OnDrag(UnityEngine.EventSystems.PointerEventData eventData)
		{
			if (_overviewMode) return;

			var position = Camera.main.ScreenToWorldPoint(eventData.pressPosition);
			if (!TrySelectComponent(position, out var component)) return;
			if (component.Locked) return;

			var command = new RemoveComponentCommand(_shipEditor, component);
			if (_commandList.TryExecute(command))
				_draggableComponent.Initialize(new DraggableComponent.Content(component.Info, component.KeyBinding, component.Behaviour, component.PersistedBarrelId, component.Rotation), eventData);
		}

		public void OnZoom(float zoom)
		{
			var cameraZoomMax = _overviewMode ? GetBestCameraZoom() : _cameraZoomMax * _shipView.Scale;
			_camera.OrthographicSize = Mathf.Clamp(_camera.OrthographicSize * zoom, _cameraZoomMin * _shipView.Scale, cameraZoomMax);
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
            _camera.Position = _shipView.transform.localPosition;
            ZoomToShip();

			if (_overviewMode)
				_overviewModeEnabled?.Invoke();
			else
				_overviewModeDisabled?.Invoke();
		}

        private void UpdateBackButton()
        {
            if (!_componentListPanel.Visible)
                _backButton.interactable = true;
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
            _buildsPanel.Visible = panel == PanelType.BuildList;
            UpdateBackButton();
		}

		private void OpenTextureCustomization()
		{
			if (_shipEditor?.Ship == null || CurrentShipId <= 0 || CurrentShipSprite == null)
				return;
			if (!PlayerShipTextureOverrides.HasConsent)
			{
				ShipTextureDisclaimerPanel.Open(this, () =>
				{
					PlayerShipTextureOverrides.HasConsent = true;
					ShipTextureCustomizationPanel.Open(this);
				});
				return;
			}

			ShipTextureCustomizationPanel.Open(this);
		}

		private void EnsureArtworkButtons()
		{
			if (_shipEditor?.Ship == null || _paintButton != null)
				return;

			// The serialized _editorWindow is an animated child which can be
			// hidden while the build/component panels are switched.  Put the
			// toolbar on the ShipEditorWindow root so it is always visible and
			// remains above the normal panel hierarchy.
			var parent = transform as RectTransform;
			if (parent == null) return;

			_artworkToolbar = new GameObject("ArtworkToolbar", typeof(RectTransform), typeof(UnityEngine.UI.Image));
			_artworkToolbar.transform.SetParent(parent, false);
			_artworkToolbar.transform.SetAsLastSibling();
			var toolbarRect = (RectTransform)_artworkToolbar.transform;
			toolbarRect.anchorMin = new Vector2(0.5f, 0f);
			toolbarRect.anchorMax = new Vector2(0.5f, 0f);
			toolbarRect.pivot = new Vector2(0.5f, 0f);
			toolbarRect.anchoredPosition = new Vector2(0f, 18f);
			toolbarRect.sizeDelta = new Vector2(302f, 64f);
			var toolbarColor = (Color)_database.UiSettings.BackgroundDark;
			toolbarColor.a = 0.92f;
			_artworkToolbar.GetComponent<UnityEngine.UI.Image>().color = toolbarColor;

			_paintButton = CreateArtworkButton(_artworkToolbar.transform, "涂装", OpenPaintCustomization, 0);
			_restoreArtworkButton = CreateArtworkButton(_artworkToolbar.transform, "还原贴图", RestoreCustomArtwork, 1);
		}

		private UnityEngine.UI.Button CreateArtworkButton(Transform parent, string text,
			UnityEngine.Events.UnityAction action, int index)
		{
			var buttonObject = new GameObject("Artwork" + index, typeof(RectTransform),
				typeof(UnityEngine.UI.Image), typeof(UnityEngine.UI.Button));
			buttonObject.transform.SetParent(parent, false);
			buttonObject.name = "Artwork" + index;
			var button = buttonObject.GetComponent<UnityEngine.UI.Button>();
			button.onClick.AddListener(action);
			buttonObject.GetComponent<UnityEngine.UI.Image>().color = _database.UiSettings.ButtonColor;
			var rect = (RectTransform)buttonObject.transform;
			rect.anchorMin = new Vector2(0f, 0.5f);
			rect.anchorMax = new Vector2(0f, 0.5f);
			rect.pivot = new Vector2(0f, 0.5f);
			rect.anchoredPosition = new Vector2(8f + index * 148f, 0f);
			rect.sizeDelta = new Vector2(136f, 48f);

			var labelObject = new GameObject("Label", typeof(RectTransform), typeof(UnityEngine.UI.Text));
			labelObject.transform.SetParent(buttonObject.transform, false);
			var label = labelObject.GetComponent<UnityEngine.UI.Text>();
			label.text = text;
			label.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
			label.fontSize = 20;
			label.alignment = TextAnchor.MiddleCenter;
			label.color = _database.UiSettings.ButtonTextColor;
			var labelRect = (RectTransform)labelObject.transform;
			labelRect.anchorMin = Vector2.zero;
			labelRect.anchorMax = Vector2.one;
			labelRect.offsetMin = Vector2.zero;
			labelRect.offsetMax = Vector2.zero;
			return button;
		}

		private void OnShipChanged(IShip ship)
		{
            if (!_shipEditor.ShipDataProvider.TryGet(ship, out var data))
                data = _shipEditor.ShipDataProvider.Default;

			var fallback = data.HasImage ? null : _resourceLocator.GetSprite(ship.Model.ModelImage);
			var sprite = data.HasImage ? null : PlayerShipTextureOverrides.Get(ship.Model.OriginalShip.Id.Value, fallback);

            _shipView.InitializeShip(_shipEditor.Layout(ShipElementType.Ship), sprite);
			_shipInitialName = _localization.GetString(_shipEditor.ShipName);
			_shipNameInputField.text = _shipInitialName;

            _shipView.Position = data.Position;
            _shipView.Rotation = data.Rotation;
            _shipView.Scale = data.Size > 0 ? data.Size / ship.Model.Layout.Size : 1.0f;

            _commandList.Clear();
			_camera.Position = data.Position;
			_camera.Rotation = data.Rotation;
			ZoomToShip();
			EnsureArtworkButtons();
		}

		private void ZoomToShip()
		{
			var cameraZoom = GetBestCameraZoom();
			var cameraZoomMax = _overviewMode ? cameraZoom : _cameraZoomMax * _shipView.Scale;
			_camera.OrthographicSize = Mathf.Clamp(cameraZoom, _cameraZoomMin * _shipView.Scale, cameraZoomMax);
		}

		private float GetBestCameraZoom()
		{
            var boundingRect = GetShipBoundingRect();
            var zoom = Mathf.Max(boundingRect.y, boundingRect.x / _camera.AspectFromFocus) / 2;
			return zoom + zoom * _cameraMargins;
		}

        private Vector2 GetShipBoundingRect()
        {
            var scale = _shipView.Scale;
            return RotationHelpers.BoundingRect(_shipView.Width * scale, _shipView.Height * scale, _shipView.Rotation - _camera.Rotation);
        }

        private void OnComponentAdded(IComponentModel component)
		{
			_shipView.AddComponent(component);
			_soundPlayer.Play(_installSound);
		}

		private void OnComponentRemoved(IComponentModel component)
		{
			_shipView.RemoveComponent(component);
		}

		private void OnComponentModified(IComponentModel component)
		{
			_shipView.UpdateComponent(component);
		}

		private void OnSatelliteChanged(SatelliteLocation location)
		{
			var layout = _shipEditor.Layout(location);
			if (layout == null)
				_shipView.RemoveSatellite(location);
			else
			{
				var satellite = _shipEditor.Satellite(location);
				var imageScale = satellite.Id.Value == 950 || satellite.Id.Value == 951 ? 0.45f : 1f;
				_shipView.InitializeSatellite(location, layout, _resourceLocator.GetSprite(satellite.ModelImage), imageScale);
			}

			_commandList.Clear(location.ToShipElement());
		}

		private void OnMultipleComponentsChanged()
		{
			_shipView.ReloadAllComponents(ShipElementType.Ship);
			_shipView.ReloadAllComponents(ShipElementType.SatelliteL);
			_shipView.ReloadAllComponents(ShipElementType.SatelliteR);
			_commandList.Clear();
		}

		private bool TrySelectComponent(Vector2 position, out IComponentModel component)
		{
			return TrySelectComponent(position, ShipElementType.Ship, out component) ||
				TrySelectComponent(position, ShipElementType.SatelliteL, out component) ||
				TrySelectComponent(position, ShipElementType.SatelliteR, out component);
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
			var settings = new ComponentSettings(item.KeyBinding, item.Behaviour, false, item.PersistedBarrelId, item.Rotation);
			return new InstallComponentCommand(_shipEditor, shipElementType, cell, item.Component, settings);
		}
	}
}
