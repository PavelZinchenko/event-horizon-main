using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using Constructor;
using Economy;
using Services.Localization;
using Zenject;
using CommonComponents;
using ShipEditor.Model;
using UnityEngine.Events;
using Services.Gui;
using Gui.Utils;
using GameDatabase;
using GameDatabase.DataModel;
using GameDatabase.Enums;
using GameDatabase.Model;

namespace ShipEditor.UI
{
	public class ComponentPanel : MonoBehaviour
	{
	    [Inject] private readonly ILocalization _localization;
		[Inject] private readonly IShipEditorModel _shipEditor;
		[Inject] private readonly IGuiManager _guiManager;
		[Inject] private readonly CommandList _commandList;
		[Inject] private readonly IDatabase _database;

		[SerializeField] private ComponentItem _componentItem;
		[SerializeField] private ControlsPanel _controlsPanel;
		[SerializeField] private DragHandler _dragHandler;
		[SerializeField] private DraggableComponent _draggableComponent;
		[SerializeField] private ComponentActionPanel _actionPanel;

		[SerializeField] private UnityEvent _closeRequested;

		private IComponentModel _componentModel;
		private ComponentInfo _componentInfo;
		private Button _creativeWorkshopButton;
		private Text _creativeWorkshopLabel;
		private GameObject _creativeWorkshopModal;
		private Button _modificationButton;
		private Text _modificationLabel;
		private GameObject _modificationModal;
		private Button _rotationButton;
		private int _inventoryRotation;

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
			var keyBinding = _componentModel != null ? _componentModel.KeyBinding : _controlsPanel.KeyBinding;
			var behaviour = _componentModel != null ? _componentModel.Behaviour : _controlsPanel.ComponentMode;
			var persistedBarrelId = _componentModel?.PersistedBarrelId ?? int.MinValue;
			var rotation = _componentModel?.Rotation ?? _inventoryRotation;
			if (_componentModel == null && IsCreativeWorkshop)
				ThreeBodyContentRules.TryGetCreativeWorkshopSelectionSettings(_database, out persistedBarrelId, out behaviour);

			var content = new DraggableComponent.Content(_componentInfo, keyBinding, behaviour, persistedBarrelId, rotation);
			_draggableComponent.Initialize(content, eventData);
		}

		public void RotateComponent()
		{
			if (!CanRotateCurrentComponent)
				return;

			if (_componentModel != null)
			{
				_shipEditor.TryRotateComponent(_componentModel);
				return;
			}

			_inventoryRotation = (_inventoryRotation + 1) & 3;
			_componentItem.Initialize(_componentInfo, _inventoryRotation);
			UpdateExtensionControls();
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
			_componentItem.Initialize(model.Info, model.Rotation);

			var component = _componentInfo.Data;
			_controlsPanel.Initialize(component, model.KeyBinding, _shipEditor.CompatibilityChecker.GetDefaultKey(component), model.Behaviour);
			UpdateExtensionControls();

            var canInstall = _shipEditor.CompatibilityChecker.IsCompatible(component) && _shipEditor.Inventory.GetQuantity(_componentInfo) > 0;
            _dragHandler.gameObject.SetActive(canInstall);

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
			_inventoryRotation = 0;

			_componentItem.Initialize(info, _inventoryRotation);

			var component = _componentInfo.Data;
			_controlsPanel.Initialize(component, -1, _shipEditor.CompatibilityChecker.GetDefaultKey(component), 0);
			UpdateExtensionControls();

			var canInstall = _shipEditor.CompatibilityChecker.IsCompatible(component);
			var alreadyInstalled = !canInstall && _shipEditor.CompatibilityChecker.ComponentLimitReached(component);

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
			if (_shipEditor.Inventory.GetQuantity(_componentInfo) == 0)
				_closeRequested?.Invoke();
		}

		private void OnComponentRemoved(IComponentModel model)
		{
			HideCreativeWorkshopSelector();
			HideModificationSelector();
			HideRotationButton();
			_closeRequested?.Invoke();
		}

		private void OnComponentModified(IComponentModel model)
		{
			if (model == _componentModel)
				SetInstalledComponent(model);
		}

		private bool IsCreativeWorkshop =>
			_componentInfo &&
			_componentInfo.Data.Id.Value == ThreeBodyContentRules.CreativeWorkshopComponentId;

		private bool CanRotateCurrentComponent =>
			_componentInfo &&
			_componentInfo.Data.CellType != CellType.Weapon &&
			_componentInfo.Data.CellType != CellType.Engine;

		private void UpdateExtensionControls()
		{
			UpdateCreativeWorkshopSelector();
			UpdateModificationButton();
			UpdateRotationButton();
		}

		private void UpdateCreativeWorkshopSelector()
		{
			if (!IsCreativeWorkshop)
			{
				HideCreativeWorkshopSelector();
				return;
			}

			EnsureCreativeWorkshopSelector();
			_creativeWorkshopButton.gameObject.SetActive(true);
			if (ThreeBodyContentRules.TryGetSelectedCreativeWorkshopDrone(_database, out var build))
				_creativeWorkshopLabel.text = "无人机配置：" + _localization.GetString(build.Ship.Name) + " #" + build.Id.Value;
			else if (_componentModel != null &&
				ThreeBodyContentRules.TryGetCreativeWorkshopDrone(_database, _componentModel.PersistedBarrelId,
					_componentModel.Behaviour, out build))
				_creativeWorkshopLabel.text = "无人机配置：" + _localization.GetString(build.Ship.Name) + " #" + build.Id.Value;
			else
				_creativeWorkshopLabel.text = "无人机配置：未选择";
		}

		private void HideCreativeWorkshopSelector()
		{
			if (_creativeWorkshopButton != null)
				_creativeWorkshopButton.gameObject.SetActive(false);
			if (_creativeWorkshopModal != null)
				_creativeWorkshopModal.SetActive(false);
		}

		private void EnsureCreativeWorkshopSelector()
		{
			if (_creativeWorkshopButton != null)
				return;

			var buttonObject = new GameObject("CreativeWorkshopSelector", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
			buttonObject.layer = gameObject.layer;
			buttonObject.transform.SetParent(transform, false);
			var rect = buttonObject.GetComponent<RectTransform>();
			rect.anchorMin = new Vector2(0.08f, 0.18f);
			rect.anchorMax = new Vector2(0.92f, 0.28f);
			rect.offsetMin = rect.offsetMax = Vector2.zero;
			buttonObject.GetComponent<Image>().color = _database.UiSettings.ButtonColor;
			_creativeWorkshopButton = buttonObject.GetComponent<Button>();
			_creativeWorkshopButton.onClick.AddListener(OpenCreativeWorkshopSelector);

			var labelObject = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
			labelObject.layer = buttonObject.layer;
			labelObject.transform.SetParent(buttonObject.transform, false);
			_creativeWorkshopLabel = labelObject.GetComponent<Text>();
			_creativeWorkshopLabel.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
			_creativeWorkshopLabel.fontSize = 21;
			_creativeWorkshopLabel.alignment = TextAnchor.MiddleCenter;
			_creativeWorkshopLabel.color = _database.UiSettings.ButtonTextColor;
			var labelRect = _creativeWorkshopLabel.rectTransform;
			labelRect.anchorMin = Vector2.zero;
			labelRect.anchorMax = Vector2.one;
			labelRect.offsetMin = labelRect.offsetMax = Vector2.zero;
		}

		private void OpenCreativeWorkshopSelector()
		{
			if (!IsCreativeWorkshop)
				return;

			if (_creativeWorkshopModal != null)
			{
				_creativeWorkshopModal.SetActive(true);
				return;
			}

			var canvas = GetComponentInParent<Canvas>() ?? FindObjectOfType<Canvas>();
			if (canvas == null)
				return;

			_creativeWorkshopModal = new GameObject("CreativeWorkshopBuildSelector", typeof(RectTransform), typeof(Canvas), typeof(CanvasRenderer), typeof(Image), typeof(GraphicRaycaster));
			_creativeWorkshopModal.layer = canvas.gameObject.layer;
			var root = _creativeWorkshopModal.GetComponent<RectTransform>();
			root.SetParent(canvas.transform, false);
			root.anchorMin = new Vector2(0.06f, 0.05f);
			root.anchorMax = new Vector2(0.94f, 0.95f);
			root.offsetMin = root.offsetMax = Vector2.zero;
			var workshopBackground = (Color)_database.UiSettings.BackgroundDark;
			workshopBackground.a = 0.99f;
			_creativeWorkshopModal.GetComponent<Image>().color = workshopBackground;
			var overlay = _creativeWorkshopModal.GetComponent<Canvas>();
			overlay.overrideSorting = true;
			overlay.sortingOrder = canvas.sortingOrder + 200;

			var title = CreateSelectorText(root, "Title", "创意工坊：选择无人机配置", 28, TextAnchor.MiddleCenter);
			title.rectTransform.anchorMin = new Vector2(0.02f, 0.9f);
			title.rectTransform.anchorMax = new Vector2(0.98f, 0.99f);
			title.rectTransform.offsetMin = title.rectTransform.offsetMax = Vector2.zero;

			var viewportObject = new GameObject("Viewport", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Mask), typeof(ScrollRect));
			viewportObject.layer = _creativeWorkshopModal.layer;
			var viewport = viewportObject.GetComponent<RectTransform>();
			viewport.SetParent(root, false);
			viewport.anchorMin = new Vector2(0.03f, 0.13f);
			viewport.anchorMax = new Vector2(0.97f, 0.88f);
			viewport.offsetMin = viewport.offsetMax = Vector2.zero;
			var workshopViewport = (Color)_database.UiSettings.WindowColor;
			workshopViewport.a = 0.98f;
			viewportObject.GetComponent<Image>().color = workshopViewport;
			viewportObject.GetComponent<Mask>().showMaskGraphic = true;

			var contentObject = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
			contentObject.layer = _creativeWorkshopModal.layer;
			var content = contentObject.GetComponent<RectTransform>();
			content.SetParent(viewport, false);
			content.anchorMin = new Vector2(0f, 1f);
			content.anchorMax = Vector2.one;
			content.pivot = new Vector2(0.5f, 1f);
			content.offsetMin = content.offsetMax = Vector2.zero;
			var group = contentObject.GetComponent<VerticalLayoutGroup>();
			group.spacing = 5f;
			group.padding = new RectOffset(8, 8, 8, 8);
			group.childForceExpandHeight = false;
			contentObject.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
			var scroll = viewportObject.GetComponent<ScrollRect>();
			scroll.viewport = viewport;
			scroll.content = content;
			scroll.horizontal = false;
			scroll.vertical = true;

			foreach (var build in ThreeBodyContentRules.GetCreativeWorkshopBuilds(_database))
				CreateCreativeWorkshopBuildRow(content, build);

			var close = CreateSelectorButton(root, "Close", "关闭", new Vector2(0.33f, 0.025f), new Vector2(0.67f, 0.105f));
			close.onClick.AddListener(() => _creativeWorkshopModal.SetActive(false));
		}

		private void CreateCreativeWorkshopBuildRow(RectTransform parent, ShipBuild build)
		{
			var rowObject = new GameObject("Build_" + build.Id.Value, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(LayoutElement));
			rowObject.layer = parent.gameObject.layer;
			rowObject.transform.SetParent(parent, false);
			var rowColor = (Color)_database.UiSettings.SelectionColor;
			rowColor.a = 0.96f;
			rowObject.GetComponent<Image>().color = rowColor;
			rowObject.GetComponent<LayoutElement>().preferredHeight = 58f;
			var text = CreateSelectorText(rowObject.transform, "Label", _localization.GetString(build.Ship.Name) + "  ·  配置 #" + build.Id.Value, 20, TextAnchor.MiddleLeft);
			text.rectTransform.anchorMin = new Vector2(0.04f, 0f);
			text.rectTransform.anchorMax = new Vector2(0.96f, 1f);
			text.rectTransform.offsetMin = text.rectTransform.offsetMax = Vector2.zero;
			rowObject.GetComponent<Button>().onClick.AddListener(() => SelectCreativeWorkshopBuild(build));
		}

		private void SelectCreativeWorkshopBuild(ShipBuild build)
		{
			if (!ThreeBodyContentRules.TryEncodeCreativeWorkshopDrone(_database, build, out var persistedBarrelId, out var behaviour))
				return;

			ThreeBodyContentRules.SetSelectedCreativeWorkshopDrone(build);
			foreach (var item in _shipEditor.InstalledComponents
				.Where(item => item.Data.Id.Value == ThreeBodyContentRules.CreativeWorkshopComponentId)
				.ToArray())
			{
				_shipEditor.SetComponentPersistedBarrelId(item, persistedBarrelId);
				_shipEditor.SetComponentBehaviour(item, behaviour);
			}

			if (_creativeWorkshopModal != null)
				_creativeWorkshopModal.SetActive(false);
			UpdateExtensionControls();
		}

		private bool TryGetModificationCategory(out ComponentCategory category)
		{
			category = _componentInfo
				? _componentInfo.Data.DisplayCategory
				: ComponentCategory.Undefined;
			return _componentInfo && ThreeBodyComponentModifications.GetOptions(_componentInfo.Data).Count > 1;
		}

		private void UpdateModificationButton()
		{
			if (!TryGetModificationCategory(out var category))
			{
				HideModificationSelector();
				return;
			}

			EnsureModificationButton();
			_modificationButton.gameObject.SetActive(true);
			_modificationLabel.text = "改装 · " + GetModificationCategoryName(category) + "：" +
				ThreeBodyComponentModifications.GetName(_componentInfo.ModificationType.Id.Value);
		}

		private void HideModificationSelector()
		{
			if (_modificationButton != null)
				_modificationButton.gameObject.SetActive(false);
			if (_modificationModal != null)
				_modificationModal.SetActive(false);
		}

		private void EnsureModificationButton()
		{
			if (_modificationButton != null)
				return;

			var buttonObject = new GameObject("ComponentModificationSelector", typeof(RectTransform),
				typeof(CanvasRenderer), typeof(Image), typeof(Button));
			buttonObject.layer = gameObject.layer;
			buttonObject.transform.SetParent(transform, false);
			var rect = buttonObject.GetComponent<RectTransform>();
			// Share the former Creative Workshop location. The two controls are
			// mutually exclusive because a workshop is not a weapon/energy/
			// defense/engine component.
			rect.anchorMin = new Vector2(0.08f, 0.18f);
			rect.anchorMax = new Vector2(0.92f, 0.28f);
			rect.offsetMin = rect.offsetMax = Vector2.zero;
			buttonObject.GetComponent<Image>().color = _database.UiSettings.ButtonColor;
			_modificationButton = buttonObject.GetComponent<Button>();
			_modificationButton.onClick.AddListener(OpenModificationSelector);

			var labelObject = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
			labelObject.layer = buttonObject.layer;
			labelObject.transform.SetParent(buttonObject.transform, false);
			_modificationLabel = labelObject.GetComponent<Text>();
			_modificationLabel.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
			_modificationLabel.fontSize = 21;
			_modificationLabel.alignment = TextAnchor.MiddleCenter;
			_modificationLabel.color = _database.UiSettings.ButtonTextColor;
			var labelRect = _modificationLabel.rectTransform;
			labelRect.anchorMin = Vector2.zero;
			labelRect.anchorMax = Vector2.one;
			labelRect.offsetMin = labelRect.offsetMax = Vector2.zero;
		}

		private void OpenModificationSelector()
		{
			if (!TryGetModificationCategory(out var category))
				return;

			if (_modificationModal != null)
				Destroy(_modificationModal);

			var canvas = GetComponentInParent<Canvas>() ?? FindObjectOfType<Canvas>();
			if (canvas == null)
				return;

			_modificationModal = new GameObject("ComponentModificationSelectorModal", typeof(RectTransform),
				typeof(Canvas), typeof(CanvasRenderer), typeof(Image), typeof(GraphicRaycaster));
			_modificationModal.layer = canvas.gameObject.layer;
			var root = _modificationModal.GetComponent<RectTransform>();
			root.SetParent(canvas.transform, false);
			root.anchorMin = new Vector2(0.16f, 0.2f);
			root.anchorMax = new Vector2(0.84f, 0.8f);
			root.offsetMin = root.offsetMax = Vector2.zero;
			var modalBackground = (Color)_database.UiSettings.BackgroundDark;
			modalBackground.a = 0.99f;
			_modificationModal.GetComponent<Image>().color = modalBackground;
			var overlay = _modificationModal.GetComponent<Canvas>();
			overlay.overrideSorting = true;
			overlay.sortingOrder = canvas.sortingOrder + 200;

			var title = CreateSelectorText(root, "Title",
				"改装 · " + GetModificationCategoryName(category), 28, TextAnchor.MiddleCenter);
			title.rectTransform.anchorMin = new Vector2(0.04f, 0.82f);
			title.rectTransform.anchorMax = new Vector2(0.84f, 0.96f);
			title.rectTransform.offsetMin = title.rectTransform.offsetMax = Vector2.zero;
			title.color = _database.UiSettings.HeaderTextColor;

			var description = CreateSelectorText(root, "Description",
				"改装会随舰船保存，并在战斗中实际生效。不同组件只显示它可以使用的改装。",
				18, TextAnchor.MiddleCenter);
			description.rectTransform.anchorMin = new Vector2(0.06f, 0.7f);
			description.rectTransform.anchorMax = new Vector2(0.94f, 0.8f);
			description.rectTransform.offsetMin = description.rectTransform.offsetMax = Vector2.zero;
			description.color = _database.UiSettings.PaleTextColor;

			var options = ThreeBodyComponentModifications.GetOptions(_componentInfo.Data);
			const float optionsTop = 0.68f;
			const float optionsBottom = 0.08f;
			const float optionSpacing = 0.012f;
			var optionHeight = options.Count > 0
				? Mathf.Min(0.11f, (optionsTop - optionsBottom - optionSpacing * (options.Count - 1)) / options.Count)
				: 0.11f;
			for (var i = 0; i < options.Count; ++i)
			{
				var option = options[i];
				var isSelected = _componentInfo.ModificationType.Id.Value == option;
				var label = (isSelected ? "已选 · " : string.Empty) + ThreeBodyComponentModifications.GetName(option) +
					"\n" + ThreeBodyComponentModifications.GetDescription(option);
				var optionMaxY = optionsTop - i * (optionHeight + optionSpacing);
				var row = CreateSelectorButton(root, "Option" + i, label,
					new Vector2(0.12f, optionMaxY - optionHeight),
					new Vector2(0.88f, optionMaxY), 20);
				row.onClick.AddListener(() => SelectModification(option));
			}

			var close = CreateSelectorButton(root, "Close", "关闭",
				new Vector2(0.86f, 0.86f), new Vector2(0.97f, 0.96f), 18);
			close.onClick.AddListener(() => _modificationModal.SetActive(false));
		}

		private void SelectModification(int option)
		{
			var modification = option == 0
				? ComponentMod.Empty
				: _database.GetComponentMod(ItemId<ComponentMod>.Create(option));
			if (modification == null)
				return;

			if (_componentModel != null)
			{
				if (!_shipEditor.TrySetComponentModification(_componentModel, modification))
					return;
			}
			else
			{
				_componentInfo = new ComponentInfo(_componentInfo.Data, modification,
					modification == ComponentMod.Empty ? ModificationQuality.N3 : ModificationQuality.P3,
					_componentInfo.Level);
				_componentItem.Initialize(_componentInfo, _inventoryRotation);
			}

			if (_modificationModal != null)
				_modificationModal.SetActive(false);
			UpdateModificationButton();
		}

		private void UpdateRotationButton()
		{
			if (!CanRotateCurrentComponent)
			{
				HideRotationButton();
				return;
			}

			EnsureRotationButton();
			_rotationButton.gameObject.SetActive(true);
		}

		private void HideRotationButton()
		{
			if (_rotationButton != null)
				_rotationButton.gameObject.SetActive(false);
		}

		private void EnsureRotationButton()
		{
			if (_rotationButton != null)
				return;

			var buttonObject = new GameObject("RotateComponentButton", typeof(RectTransform),
				typeof(CanvasRenderer), typeof(Image), typeof(Button));
			buttonObject.layer = gameObject.layer;
			buttonObject.transform.SetParent(transform, false);
			var rect = buttonObject.GetComponent<RectTransform>();
			rect.anchorMin = new Vector2(0.68f, 0.31f);
			rect.anchorMax = new Vector2(0.92f, 0.39f);
			rect.offsetMin = rect.offsetMax = Vector2.zero;
			buttonObject.GetComponent<Image>().color = _database.UiSettings.ButtonColor;
			_rotationButton = buttonObject.GetComponent<Button>();
			_rotationButton.onClick.AddListener(RotateComponent);

			var label = CreateSelectorText(rect, "Label", "旋转 90°", 18, TextAnchor.MiddleCenter);
			label.rectTransform.anchorMin = Vector2.zero;
			label.rectTransform.anchorMax = Vector2.one;
			label.rectTransform.offsetMin = label.rectTransform.offsetMax = Vector2.zero;
		}

		private static string GetModificationCategoryName(ComponentCategory category)
		{
			switch (category)
			{
				case ComponentCategory.Weapon: return "武器";
				case ComponentCategory.Energy: return "能源";
				case ComponentCategory.Defense: return "防御";
				case ComponentCategory.Engine: return "引擎";
				default: return "组件";
			}
		}

		private Text CreateSelectorText(Transform parent, string name, string value, int fontSize, TextAnchor alignment)
		{
			var gameObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
			gameObject.layer = parent.gameObject.layer;
			gameObject.transform.SetParent(parent, false);
			var text = gameObject.GetComponent<Text>();
			text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
			text.text = value;
			text.fontSize = fontSize;
			text.alignment = alignment;
			text.color = _database.UiSettings.TextColor;
			return text;
		}

		private Button CreateSelectorButton(RectTransform parent, string name, string label, Vector2 anchorMin, Vector2 anchorMax, int fontSize = 22)
		{
			var gameObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
			gameObject.layer = parent.gameObject.layer;
			var rect = gameObject.GetComponent<RectTransform>();
			rect.SetParent(parent, false);
			rect.anchorMin = anchorMin;
			rect.anchorMax = anchorMax;
			rect.offsetMin = rect.offsetMax = Vector2.zero;
			gameObject.GetComponent<Image>().color = _database.UiSettings.ButtonColor;
			var text = CreateSelectorText(rect, "Label", label, fontSize, TextAnchor.MiddleCenter);
			text.color = _database.UiSettings.ButtonTextColor;
			text.rectTransform.anchorMin = Vector2.zero;
			text.rectTransform.anchorMax = Vector2.one;
			text.rectTransform.offsetMin = text.rectTransform.offsetMax = Vector2.zero;
			return gameObject.GetComponent<Button>();
		}
	}
}
