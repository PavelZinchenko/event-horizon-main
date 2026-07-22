using System.Linq;
using Domain.Quests;
using Economy.ItemType;
using Economy.Products;
using Galaxy;
using GameDatabase.DataModel;
using UnityEngine;
using UnityEngine.UI;
using GameModel.Quests;
using GameServices.Player;
using GameServices.Quests;
using GameStateMachine.States;
using Services.Localization;
using Services.Messenger;
using Session;
using Zenject;

namespace ViewModel
{
	public class FactionPanelViewModel : MonoBehaviour
	{
        [Inject] private readonly ItemTypeFactory _factory;
        [Inject] private readonly IMessenger _messenger;
	    [Inject] private readonly MotherShip _motherShip;
	    [Inject] private readonly IQuestManager _questManager;
	    [Inject] private readonly OpenShopSignal.Trigger _openShopTrigger;
	    [Inject] private readonly OpenWorkshopSignal.Trigger _openWorkshopTrigger;
	    [Inject] private readonly OpenShipyardSignal.Trigger _openShipyardTrigger;
	    [Inject] private readonly InventoryFactory _inventoryFactory;
	    [Inject] private readonly ISessionData _session;
	    [Inject] private readonly ILocalization _localization;
        [Inject] private readonly QuestEventSignal.Trigger _questEventTrigger;
	    [Inject] private readonly StartBattleSignal.Trigger _startBattleTrigger;

		[SerializeField] private GameObject CaptureButton;
	    [SerializeField] private GameObject CaptureDescription;
	    [SerializeField] private GameObject MilitaryPowerPanel;
	    [SerializeField] private GameObject ReputationPanel;
        [SerializeField] private GameObject ShopButton;
	    [SerializeField] private GameObject CraftButton;
	    [SerializeField] private GameObject ShipyardButton;
	    [SerializeField] private GameObject MissionButton;
	    [SerializeField] private Text FactionName;
	    [SerializeField] private Text PowerText;
	    [SerializeField] private Text ReputationText;

        public void OpenStore()
		{
            _openShopTrigger.Fire(_inventoryFactory.CreateFactionInventory(_motherShip.CurrentStar.Region), _inventoryFactory.CreatePlayerInventory());
        }

		public void CaptureBase()
		{
			UnityEngine.Debug.Log("FactionPanelViewModel.CaptureBase");

			_motherShip.CurrentStar.CaptureBase();
		}

        public void PeacefullyTransferBase()
        {
            if (_motherShip.CurrentStar.PeacefulTransferBase())
                OnEnable();
        }

        public void DefendBase()
        {
            _motherShip.CurrentStar.DefendBase();
        }

        public static bool IncludeStarshipEarthAllies { get; private set; }

        public bool MissionsAvailable
        {
            get
            {
                if (_motherShip.CurrentStar.Region.Faction.NoMissions) return false;
                return !_questManager.Quests.Any(item => item.IsFactionMission(_motherShip.CurrentStar.Id));
            }
        }

        public void TakeMission()
	    {
            MissionButton.gameObject.SetActive(false);
	        _questEventTrigger.Fire(new StarEventData(QuestEventType.FactionMissionAccepted, _motherShip.CurrentStar.Region.HomeStar));
        }

		private void OnEnable()
		{
            ConfigurePreview4Layout();
			BindFacilityButtons();
			var region = _motherShip.CurrentStar.Region;

		    FactionName.text = _localization.GetString(region.Faction.Name);
            FactionName.color = region.Faction.Color;

		    var reputation = _session.Quests.GetFactionRelations(region.HomeStar);

            if (region.IsCaptured)
		    {
                SetJointControlsVisible(false);
		        SetPeacefulTransferVisible(false);
		        SetDefenseVisible(true);
		        CaptureButton.gameObject.SetActive(false);
		        CaptureDescription.gameObject.SetActive(false);
		        MilitaryPowerPanel.gameObject.SetActive(false);
		        ReputationPanel.gameObject.SetActive(false);
		        ShopButton.SetActive(true);
		        CraftButton.SetActive(true);
		        ShipyardButton.SetActive(true);
		        MissionButton.gameObject.SetActive(false);
                return;
		    }

            CaptureButton.gameObject.SetActive(true);
            SetDefenseVisible(false);
            SetJointControlsVisible(true);
		    CaptureDescription.gameObject.SetActive(true);
		    MilitaryPowerPanel.gameObject.SetActive(true);
		    ReputationPanel.gameObject.SetActive(region.Faction != Faction.Empty);
            var relationState = reputation > 25 ? "友好" : reputation < -25 ? "敌对" : "中立";
		    ReputationText.text = $"{reputation:+0;-0;0}  {relationState}";
		    SetPeacefulTransferVisible(reputation > 25);
		    PowerText.text = region.BaseDefensePower + "%";

            MissionButton.gameObject.SetActive(MissionsAvailable);

			ShopButton.SetActive(reputation >= 5);
			CraftButton.SetActive(reputation >= 60);
            ShipyardButton.SetActive(reputation >= 90);
		}

		private bool _facilityButtonsBound;

		private void BindFacilityButtons()
		{
			if (_facilityButtonsBound)
				return;

			var workshopButton = CraftButton != null ? CraftButton.GetComponent<Button>() : null;
			var shipyardButton = ShipyardButton != null ? ShipyardButton.GetComponent<Button>() : null;
			if (workshopButton != null)
				workshopButton.onClick.AddListener(OpenWorkshop);
			if (shipyardButton != null)
				shipyardButton.onClick.AddListener(OpenShipyard);
			_facilityButtonsBound = workshopButton != null || shipyardButton != null;
		}

		private void OpenWorkshop()
		{
			var region = _motherShip.CurrentStar.Region;
			_openWorkshopTrigger.Fire(region.Faction, Mathf.Max(1, region.HomeStarLevel));
		}

		private void OpenShipyard()
		{
			var region = _motherShip.CurrentStar.Region;
			_openShipyardTrigger.Fire(region.Faction, Mathf.Max(1, region.HomeStarLevel));
		}

        private void ConfigurePreview4Layout()
        {
            foreach (var text in GetComponentsInChildren<Text>(true))
            {
                text.resizeTextForBestFit = true;
                text.resizeTextMinSize = 14;
                text.resizeTextMaxSize = Mathf.Min(text.fontSize, 26);
            }

            var captureRect = CaptureButton.GetComponent<RectTransform>();
            if (captureRect == null)
                return;

            var buttonsRect = CaptureButton.transform.parent.GetComponent<RectTransform>();
            if (buttonsRect != null)
                buttonsRect.sizeDelta = new Vector2(Mathf.Max(430f, buttonsRect.sizeDelta.x), Mathf.Max(440f, buttonsRect.sizeDelta.y));
            var captureLayout = CaptureButton.GetComponent<LayoutElement>() ?? CaptureButton.AddComponent<LayoutElement>();
            captureLayout.minWidth = 410f;
            captureLayout.preferredWidth = 410f;
            captureLayout.minHeight = 78f;
            captureLayout.preferredHeight = 78f;
            captureLayout.flexibleWidth = 0f;
            captureRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 410f);
            captureRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 78f);
            foreach (var captureLabel in CaptureButton.GetComponentsInChildren<Text>(true))
            {
                captureLabel.resizeTextForBestFit = true;
                captureLabel.resizeTextMinSize = 10;
                captureLabel.resizeTextMaxSize = Mathf.Max(18, captureLabel.fontSize);
                captureLabel.horizontalOverflow = HorizontalWrapMode.Wrap;
            }

            var emblem = transform.Find("Body/Left/Faction") ?? transform.Find("Faction");
            if (emblem != null)
                emblem.localScale = Vector3.one * 0.72f;

            var rootCanvas = GetComponentInParent<Canvas>()?.rootCanvas;
            _alliedAttackPanel = (rootCanvas != null ? rootCanvas.GetComponentsInChildren<Transform>(true) : GetComponentsInChildren<Transform>(true))
                .FirstOrDefault(item => item.name == "Preview7AlliedAttackDialog")?.gameObject;
            _jointAttackButton = GetComponentsInChildren<Transform>(true)
                .FirstOrDefault(item => item.name == "Preview5JointAttackButton")?.gameObject;
			EnsurePeacefulTransferButton();
			EnsureDefenseButton();
            if (_alliedAttackPanel != null && _jointAttackButton != null)
                return;

            var panel = new GameObject("Preview7AlliedAttackDialog", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            var rect = panel.GetComponent<RectTransform>();
            rect.SetParent(rootCanvas != null ? rootCanvas.transform : transform, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            panel.GetComponent<Image>().color = new Color(0f, 0.02f, 0.05f, 0.78f);
            panel.SetActive(false);
            _alliedAttackPanel = panel;

            var dismissButton = panel.AddComponent<Button>();
            dismissButton.targetGraphic = panel.GetComponent<Image>();
            dismissButton.onClick.AddListener(() => panel.SetActive(false));

            var card = new GameObject("Card", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Outline));
            var cardRect = card.GetComponent<RectTransform>();
            cardRect.SetParent(rect, false);
            cardRect.anchorMin = cardRect.anchorMax = new Vector2(0.5f, 0.5f);
            cardRect.pivot = new Vector2(0.5f, 0.5f);
            cardRect.sizeDelta = new Vector2(540f, 300f);
            card.GetComponent<Image>().color = new Color(0.015f, 0.09f, 0.15f, 0.98f);
            var cardOutline = card.GetComponent<Outline>();
            cardOutline.effectColor = new Color(0.12f, 0.72f, 1f, 0.95f);
            cardOutline.effectDistance = new Vector2(2f, -2f);

            var title = new GameObject("Title", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            var titleRect = title.GetComponent<RectTransform>();
            titleRect.SetParent(cardRect, false);
            titleRect.anchorMin = new Vector2(0f, 1f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0f, -18f);
            titleRect.sizeDelta = new Vector2(-36f, 56f);
            var titleText = title.GetComponent<Text>();
            titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            titleText.fontSize = 28;
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.color = new Color(0.5f, 0.9f, 1f);
            titleText.text = "选择联合进攻舰队";

            var toggleObject = new GameObject("StarshipEarth", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Toggle));
            var toggleRect = toggleObject.GetComponent<RectTransform>();
            toggleRect.SetParent(cardRect, false);
            toggleRect.anchorMin = toggleRect.anchorMax = new Vector2(0.5f, 0.5f);
            toggleRect.pivot = new Vector2(0.5f, 0.5f);
            toggleRect.anchoredPosition = new Vector2(0f, 20f);
            toggleRect.sizeDelta = new Vector2(460f, 72f);
            toggleObject.GetComponent<Image>().color = new Color(0.035f, 0.18f, 0.25f, 1f);

            var checkBackground = new GameObject("CheckBackground", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            var checkBackgroundRect = checkBackground.GetComponent<RectTransform>();
            checkBackgroundRect.SetParent(toggleRect, false);
            checkBackgroundRect.anchorMin = checkBackgroundRect.anchorMax = new Vector2(0f, 0.5f);
            checkBackgroundRect.pivot = new Vector2(0f, 0.5f);
            checkBackgroundRect.sizeDelta = new Vector2(38f, 38f);
            checkBackgroundRect.anchoredPosition = new Vector2(18f, 0f);
            checkBackground.GetComponent<Image>().color = new Color(0.02f, 0.05f, 0.08f, 1f);

            var check = new GameObject("Checkmark", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            var checkRect = check.GetComponent<RectTransform>();
            checkRect.SetParent(checkBackgroundRect, false);
            checkRect.anchorMin = new Vector2(0.2f, 0.2f);
            checkRect.anchorMax = new Vector2(0.8f, 0.8f);
            checkRect.offsetMin = Vector2.zero;
            checkRect.offsetMax = Vector2.zero;
            check.GetComponent<Image>().color = new Color(0.12f, 0.75f, 1f, 1f);

            var label = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            var labelRect = label.GetComponent<RectTransform>();
            labelRect.SetParent(toggleRect, false);
            labelRect.anchorMin = new Vector2(0f, 0f);
            labelRect.anchorMax = new Vector2(1f, 1f);
            labelRect.offsetMin = new Vector2(72f, 0f);
            labelRect.offsetMax = new Vector2(-12f, 0f);
            var labelText = label.GetComponent<Text>();
            labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            labelText.fontSize = 22;
            labelText.alignment = TextAnchor.MiddleLeft;
            labelText.color = Color.white;
            labelText.text = "星舰地球支援舰队";

            var toggle = toggleObject.GetComponent<Toggle>();
            toggle.graphic = check.GetComponent<Image>();
            toggle.targetGraphic = toggleObject.GetComponent<Image>();
            toggle.isOn = IncludeStarshipEarthAllies;

            var confirmObject = new GameObject("Confirm", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(Outline));
            var confirmRect = confirmObject.GetComponent<RectTransform>();
            confirmRect.SetParent(cardRect, false);
            confirmRect.anchorMin = confirmRect.anchorMax = new Vector2(0.5f, 0f);
            confirmRect.pivot = new Vector2(0.5f, 0f);
            confirmRect.anchoredPosition = new Vector2(0f, 22f);
            confirmRect.sizeDelta = new Vector2(300f, 62f);
            confirmObject.GetComponent<Image>().color = new Color(0.04f, 0.46f, 0.68f, 1f);
            confirmObject.GetComponent<Outline>().effectColor = new Color(0.3f, 0.9f, 1f, 0.8f);
            var confirmText = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            var confirmTextRect = confirmText.GetComponent<RectTransform>();
            confirmTextRect.SetParent(confirmRect, false);
            confirmTextRect.anchorMin = Vector2.zero;
            confirmTextRect.anchorMax = Vector2.one;
            confirmTextRect.offsetMin = Vector2.zero;
            confirmTextRect.offsetMax = Vector2.zero;
            var confirmLabel = confirmText.GetComponent<Text>();
            confirmLabel.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            confirmLabel.fontSize = 24;
            confirmLabel.alignment = TextAnchor.MiddleCenter;
            confirmLabel.color = Color.white;
            confirmLabel.text = "确认选择";
            confirmObject.GetComponent<Button>().onClick.AddListener(() =>
            {
                IncludeStarshipEarthAllies = toggle.isOn;
                panel.SetActive(false);
            });

            var jointObject = new GameObject("Preview5JointAttackButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            var jointRect = jointObject.GetComponent<RectTransform>();
            jointRect.SetParent(CaptureButton.transform.parent, false);
            jointObject.transform.SetAsLastSibling();
            jointRect.sizeDelta = new Vector2(410f, 78f);
            var jointLayout = jointObject.AddComponent<LayoutElement>();
            jointLayout.minWidth = 410f;
            jointLayout.preferredWidth = 410f;
            jointLayout.minHeight = 78f;
            jointLayout.preferredHeight = 78f;
            jointLayout.flexibleWidth = 0f;
            var captureImage = CaptureButton.GetComponent<Image>();
            var jointImage = jointObject.GetComponent<Image>();
            jointImage.color = new Color(0.025f, 0.32f, 0.48f, 1f);
            var jointOutline = jointObject.AddComponent<Outline>();
            jointOutline.effectColor = new Color(0.2f, 0.8f, 1f, 0.85f);
            jointOutline.effectDistance = new Vector2(2f, -2f);
            jointObject.GetComponent<Button>().onClick.AddListener(() =>
            {
                toggle.isOn = IncludeStarshipEarthAllies;
                panel.SetActive(true);
                panel.transform.SetAsLastSibling();
            });

            var jointLabel = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            var jointLabelRect = jointLabel.GetComponent<RectTransform>();
            jointLabelRect.SetParent(jointRect, false);
            jointLabelRect.anchorMin = Vector2.zero;
            jointLabelRect.anchorMax = Vector2.one;
            jointLabelRect.offsetMin = new Vector2(8f, 4f);
            jointLabelRect.offsetMax = new Vector2(-8f, -4f);
            var jointText = jointLabel.GetComponent<Text>();
            jointText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            jointText.fontSize = 22;
            jointText.resizeTextForBestFit = true;
            jointText.resizeTextMinSize = 14;
            jointText.alignment = TextAnchor.MiddleCenter;
            jointText.color = Color.white;
            jointText.text = "◇  联合进攻";
            _jointAttackButton = jointObject;
        }

        private void EnsurePeacefulTransferButton()
        {
            _peacefulTransferButton = GetComponentsInChildren<Transform>(true)
                .FirstOrDefault(item => item.name == "PeacefulTransferButton")?.gameObject;
            if (_peacefulTransferButton != null)
                return;

            var buttonObject = new GameObject("PeacefulTransferButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(LayoutElement), typeof(Outline));
            var rect = buttonObject.GetComponent<RectTransform>();
            rect.SetParent(CaptureButton.transform.parent, false);
            rect.sizeDelta = new Vector2(410f, 78f);
            var layout = buttonObject.GetComponent<LayoutElement>();
            layout.minWidth = layout.preferredWidth = 410f;
            layout.minHeight = layout.preferredHeight = 78f;
            layout.flexibleWidth = 0f;
            buttonObject.GetComponent<Image>().color = new Color(0.04f, 0.42f, 0.34f, 1f);
            var outline = buttonObject.GetComponent<Outline>();
            outline.effectColor = new Color(0.35f, 1f, 0.72f, 0.9f);
            outline.effectDistance = new Vector2(2f, -2f);
            buttonObject.GetComponent<Button>().onClick.AddListener(PeacefullyTransferBase);

            var textTemplate = CaptureButton.GetComponentInChildren<Text>(true);
            Text label;
            if (textTemplate != null)
            {
                label = Instantiate(textTemplate, rect);
                label.name = "Label";
            }
            else
            {
                label = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text)).GetComponent<Text>();
                label.transform.SetParent(rect, false);
                label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }

            var labelRect = label.rectTransform;
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(8f, 4f);
            labelRect.offsetMax = new Vector2(-8f, -4f);
            label.text = "和平交接";
            label.alignment = TextAnchor.MiddleCenter;
            label.resizeTextForBestFit = true;
            label.resizeTextMinSize = 14;
            label.resizeTextMaxSize = 24;
            label.color = Color.white;
            _peacefulTransferButton = buttonObject;
        }

        private void SetPeacefulTransferVisible(bool visible)
        {
            if (_peacefulTransferButton != null)
                _peacefulTransferButton.SetActive(visible);
        }

        private void EnsureDefenseButton()
        {
            _defenseButton = GetComponentsInChildren<Transform>(true)
                .FirstOrDefault(item => item.name == "StarbaseDefenseButton")?.gameObject;
            if (_defenseButton != null)
                return;

            var buttonObject = new GameObject("StarbaseDefenseButton", typeof(RectTransform), typeof(CanvasRenderer),
                typeof(Image), typeof(Button), typeof(LayoutElement), typeof(Outline));
            var rect = buttonObject.GetComponent<RectTransform>();
            rect.SetParent(CaptureButton.transform.parent, false);
            rect.sizeDelta = new Vector2(410f, 78f);
            var layout = buttonObject.GetComponent<LayoutElement>();
            layout.minWidth = layout.preferredWidth = 410f;
            layout.minHeight = layout.preferredHeight = 78f;
            layout.flexibleWidth = 0f;
            buttonObject.GetComponent<Image>().color = new Color(0.05f, 0.31f, 0.58f, 1f);
            var outline = buttonObject.GetComponent<Outline>();
            outline.effectColor = new Color(0.32f, 0.78f, 1f, 0.95f);
            outline.effectDistance = new Vector2(2f, -2f);
            buttonObject.GetComponent<Button>().onClick.AddListener(DefendBase);

            var template = CaptureButton.GetComponentInChildren<Text>(true);
            var label = template != null
                ? Instantiate(template, rect)
                : new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text)).GetComponent<Text>();
            if (template == null)
            {
                label.transform.SetParent(rect, false);
                label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }
            label.name = "Label";
            label.rectTransform.anchorMin = Vector2.zero;
            label.rectTransform.anchorMax = Vector2.one;
            label.rectTransform.offsetMin = new Vector2(8f, 4f);
            label.rectTransform.offsetMax = new Vector2(-8f, -4f);
            label.text = "防卫";
            label.alignment = TextAnchor.MiddleCenter;
            label.resizeTextForBestFit = true;
            label.resizeTextMinSize = 14;
            label.resizeTextMaxSize = 26;
            label.color = Color.white;
            _defenseButton = buttonObject;
        }

        private void SetDefenseVisible(bool visible)
        {
            if (_defenseButton != null)
                _defenseButton.SetActive(visible);
        }

        private void SetJointControlsVisible(bool visible)
        {
            if (_jointAttackButton != null)
                _jointAttackButton.SetActive(visible);
            if (!visible && _alliedAttackPanel != null)
                _alliedAttackPanel.SetActive(false);
        }

        private GameObject _jointAttackButton;
        private GameObject _alliedAttackPanel;
        private GameObject _peacefulTransferButton;
        private GameObject _defenseButton;
	}
}
