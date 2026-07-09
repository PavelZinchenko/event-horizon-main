using Game;
using UnityEngine;
using UnityEngine.UI;
using GameServices.Player;
using GameStateMachine.States;
using Services.Messenger;
using Gui.Windows;
using Services.Gui;
using Zenject;
using System.Linq;
using GameDatabase;
using Combat.Component.Unit.Classification;
using Services.Localization;
using Session;

namespace Gui.StarMap
{
    public class GameMenu : MonoBehaviour
    {
        [Inject] private readonly MotherShip _motherShip;
        [Inject] private readonly ExitSignal.Trigger _exitTrigger;
        [Inject] private readonly Galaxy.StarMap _starMap;
        [Inject] private readonly HolidayManager _holidayManager;
        [Inject] private readonly IMessenger _messenger;
        [Inject] private readonly IDatabase _database;
        [Inject] private readonly ILocalization _localization;
        [Inject] private readonly ISessionData _session;
        [Inject] private readonly GameModel.RegionMap _regionMap;

        public AnimatedWindow InformationPanel;
        public AnimatedWindow CargoHoldPanel;
        public AnimatedWindow FleetPanel;
        public AnimatedWindow ResearchPanel;

        public AnimatedWindow SurvivalPanel;
        public AnimatedWindow ArenaPanel;
        public AnimatedWindow RuinsPanel;
        public AnimatedWindow XmasPanel;
        public AnimatedWindow MilitaryPanel;
        public AnimatedWindow PlanetPanel;
        public AnimatedWindow BossPanel;
        public AnimatedWindow FactionPanel;
        public AnimatedWindow WormholePanel;
        public AnimatedWindow BlackMarketPanel;
        public AnimatedWindow ChallengePanel;
        public AnimatedWindow IapStoreWindow;
        public AnimatedWindow QuestLogWindow;

        [SerializeField] private Button StarViewButton;
        [SerializeField] private Button GalaxyViewButton;
        [SerializeField] private GameObject GalaxyButtonsGroup;
        [SerializeField] private GameObject FiltersGroup;
        [SerializeField] private Toggle BookmarkFilterToggle;
        [SerializeField] private Toggle BossFilterToggle;
        [SerializeField] private Toggle ShopFilterToggle;
        [SerializeField] private Toggle ArenaFilterToggle;
        [SerializeField] private Toggle XmasFilterToggle;
        
        public void ShowInformation() { InformationPanel.Open(); }
        public void ShowCargoHold() { CargoHoldPanel.Open(); }
        public void ShowFleet() { FleetPanel.Open(); }
        public void ShowResearch() { ResearchPanel.Open(); }
        public void ShowSurvival() { SurvivalPanel.Open(); }
        public void ShowArena() { ArenaPanel.Open(); }
        public void ShowRuins() { RuinsPanel.Open(); }
        public void ShowXmas() { XmasPanel.Open(); }
        public void ShowMilitaryBase() { MilitaryPanel.Open(); }
        public void ShowPandemic() { PlanetPanel.Open(new WindowArgs(Game.Exploration.Planet.InfectedPlanetId)); }
        public void ShowPlanet(int id) { PlanetPanel.Open(new WindowArgs(id)); }
        public void ShowBoss() { BossPanel.Open(); }
        public void ShowFaction() { FactionPanel.Open(); }
        public void ShowWormhole() { WormholePanel.Open(); }
        public void ShowBlackMarket() { BlackMarketPanel.Open(); }
        public void ShowChallenge() { ChallengePanel.Open(); }
        public void ShowIapStore() { IapStoreWindow.Open(); }
        public void ShowQuestLog() { QuestLogWindow.Open(); }

        public void ExitToMainMenu()
        {
            _exitTrigger.Fire();
        }

        public void OnFiltersChanged()
        {
            _starMap.ShowBosses = BossFilterToggle.isOn;
            _starMap.ShowStores = ShopFilterToggle.isOn;
            _starMap.ShowBookmarks = BookmarkFilterToggle.isOn;
            _starMap.ShowArenas = ArenaFilterToggle.isOn;
            _starMap.ShowXmas = XmasFilterToggle.isOn && _holidayManager.IsChristmas;
            _messenger.Broadcast(EventType.StarMapContentChanged);
        }

        private void Start()
        {
            ApplyPreview4FactionIcon();
            CreateRelationsButton();
            _messenger.AddListener<int>(EventType.PlayerPositionChanged, OnPlayerPositionChanged);
            _messenger.AddListener<ViewMode>(EventType.ViewModeChanged, OnMapStateChanged);
            _messenger.AddListener<Galaxy.StarObjectType>(EventType.ArrivedToObject, OnArrivedToObject);
            _messenger.AddListener<int>(EventType.ArrivedToPlanet, OnArrivedToPlanet);

            XmasFilterToggle.gameObject.SetActive(_holidayManager.IsChristmas);

            InitButtons();
            OnFiltersChanged();
        }

        private void CreateRelationsButton()
        {
            if (transform.Find("Preview5RelationsButton") != null) return;
            var exit = GetComponentsInChildren<Button>(true).FirstOrDefault(button =>
            {
                for (var i = 0; i < button.onClick.GetPersistentEventCount(); i++)
                    if (button.onClick.GetPersistentMethodName(i) == nameof(ExitToMainMenu)) return true;
                return false;
            });
            if (exit == null) return;

            var buttonObject = new GameObject("Preview5RelationsButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(LayoutElement));
            buttonObject.transform.SetParent(exit.transform.parent, false);
            buttonObject.name = "Preview5RelationsButton";
            var rect = buttonObject.GetComponent<RectTransform>();
            var exitRect = exit.GetComponent<RectTransform>();
            rect.anchorMin = exitRect.anchorMin;
            rect.anchorMax = exitRect.anchorMax;
            rect.pivot = exitRect.pivot;
            rect.sizeDelta = exitRect.sizeDelta;
            rect.anchoredPosition = exitRect.anchoredPosition + new Vector2(exitRect.rect.width + 12f, 0f);
            var sourceLayout = exit.GetComponent<LayoutElement>();
            var buttonLayout = buttonObject.GetComponent<LayoutElement>();
            if (sourceLayout != null)
            {
                buttonLayout.minWidth = sourceLayout.minWidth;
                buttonLayout.minHeight = sourceLayout.minHeight;
                buttonLayout.preferredWidth = sourceLayout.preferredWidth;
                buttonLayout.preferredHeight = sourceLayout.preferredHeight;
                buttonLayout.flexibleWidth = sourceLayout.flexibleWidth;
                buttonLayout.flexibleHeight = sourceLayout.flexibleHeight;
                buttonLayout.layoutPriority = sourceLayout.layoutPriority;
            }
            else
            {
                buttonLayout.preferredWidth = 120f;
                buttonLayout.preferredHeight = 120f;
            }
            var sourceImage = exit.GetComponent<Image>();
            var image = buttonObject.GetComponent<Image>();
            if (sourceImage != null)
            {
                image.sprite = sourceImage.sprite;
                image.type = sourceImage.type;
                image.color = sourceImage.color;
            }
            var button = buttonObject.GetComponent<Button>();
            button.onClick.AddListener(ToggleRelationsPanel);
            var iconObject = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            var iconRect = iconObject.GetComponent<RectTransform>();
            iconRect.SetParent(buttonObject.transform, false);
            iconRect.anchorMin = Vector2.zero;
            iconRect.anchorMax = Vector2.one;
            iconRect.offsetMin = new Vector2(10f, 10f);
            iconRect.offsetMax = new Vector2(-10f, -10f);
            var iconImage = iconObject.GetComponent<Image>();
            iconImage.sprite = Resources.Load<Sprite>("Textures/UI/faction_relations_preview4");
            iconImage.preserveAspect = true;
            iconImage.raycastTarget = false;
        }

        private void ToggleRelationsPanel()
        {
            if (_relationsPanel != null)
            {
                _relationsPanel.SetActive(!_relationsPanel.activeSelf);
                return;
            }

            _relationsPanel = new GameObject("Preview5RelationsPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            var rect = _relationsPanel.GetComponent<RectTransform>();
            rect.SetParent(transform.root, false);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(760f, 520f);
            _relationsPanel.GetComponent<Image>().color = new Color(0.015f, 0.04f, 0.08f, 0.97f);
            _relationsPanel.transform.SetAsLastSibling();

            var layout = _relationsPanel.AddComponent<GridLayoutGroup>();
            layout.padding = new RectOffset(24, 24, 24, 24);
            layout.spacing = new Vector2(8f, 4f);
            layout.cellSize = new Vector2(350f, 28f);
            layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            layout.constraintCount = 2;
            var title = NewRelationText(rect, "玩家势力关系", 26);
            title.color = new Color(0.3f, 0.8f, 1f);
            foreach (var faction in _database.FactionList.OrderBy(item => item.Id.Value))
            {
                var reputation = GetFactionReputation(faction);
                var state = reputation > 25 ? "友好" : reputation < -25 ? "敌对" : "中立";
                var row = NewRelationText(rect,
                    $"{faction.Id.Value:00}  {_localization.GetString(faction.Name)}    {reputation:+0;-0;0}  {state}", 18);
                row.color = reputation > 25
                    ? new Color(0.3f, 0.8f, 1f)
                    : reputation < -25
                        ? new Color(1f, 0.35f, 0.25f)
                        : new Color(0.85f, 0.85f, 0.65f);
            }
        }

        private int GetFactionReputation(GameDatabase.DataModel.Faction faction)
        {
            foreach (var regionId in _session.Regions.Regions)
            {
                var region = _regionMap[regionId];
                if (region != GameModel.Region.Empty && region.Faction.Id == faction.Id)
                    return _session.Quests.GetFactionRelations(region.HomeStar);
            }

            return faction.Id.Value >= GameModel.Region.StarshipEarthFactionId ? 50 : -50;
        }

        private static Text NewRelationText(Transform parent, string value, int size)
        {
            var go = new GameObject("Row", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text), typeof(LayoutElement));
            go.transform.SetParent(parent, false);
            go.GetComponent<LayoutElement>().preferredHeight = 28f;
            var text = go.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = size;
            text.alignment = TextAnchor.MiddleLeft;
            text.text = value;
            return text;
        }

        private void ApplyPreview4FactionIcon()
        {
            var icon = Resources.Load<Sprite>("Textures/UI/faction_relations_preview4");
            if (icon == null)
                return;

            foreach (var button in GetComponentsInChildren<Button>(true))
            {
                var opensFactionPanel = false;
                for (var i = 0; i < button.onClick.GetPersistentEventCount(); i++)
                    opensFactionPanel |= button.onClick.GetPersistentMethodName(i) == nameof(ShowFaction);

                if (!opensFactionPanel)
                    continue;

                var images = button.GetComponentsInChildren<Image>(true);
                var target = images.FirstOrDefault(image => image.gameObject != button.gameObject) ??
                             images.FirstOrDefault();
                if (target != null)
                {
                    target.sprite = icon;
                    target.preserveAspect = true;
                }
            }
        }

        private void OnPlayerPositionChanged(int starId)
        {
            InitButtons();
        }

        private void OnMapStateChanged(ViewMode view)
        {
            InitButtons();
        }

        private void InitButtons()
        {
            var view = _motherShip.ViewMode;

            StarViewButton.gameObject.SetActive(view == ViewMode.StarMap);
            GalaxyViewButton.gameObject.SetActive(view == ViewMode.StarSystem || view == ViewMode.GalaxyMap);
            GalaxyButtonsGroup.SetActive(view == ViewMode.StarMap);
            FiltersGroup.gameObject.SetActive(view == ViewMode.GalaxyMap);
            ShowInformation();
        }

        private void OnArrivedToObject(Galaxy.StarObjectType objectType)
        {
            switch (objectType)
            {
                case Galaxy.StarObjectType.Undefined:
                    ShowInformation();
                    break;
                case Galaxy.StarObjectType.Boss:
                    ShowBoss();
                    break;
                case Galaxy.StarObjectType.StarBase:
                    ShowFaction();
                    break;
                case Galaxy.StarObjectType.Wormhole:
                    ShowWormhole();
                    break;
                case Galaxy.StarObjectType.Military:
                    ShowMilitaryBase();
                    break;
                case Galaxy.StarObjectType.Challenge:
                    ShowChallenge();
                    break;
                case Galaxy.StarObjectType.Arena:
                    ShowArena();
                    break;
                case Galaxy.StarObjectType.Ruins:
                    ShowRuins();
                    break;
                case Galaxy.StarObjectType.Xmas:
                    ShowXmas();
                    break;
                case Galaxy.StarObjectType.Survival:
                    ShowSurvival();
                    break;
                case Galaxy.StarObjectType.BlackMarket:
                    ShowBlackMarket();
                    break;
                case Galaxy.StarObjectType.Hive:
                    ShowPandemic();
                    break;
                case Galaxy.StarObjectType.Event:
                    _motherShip.CurrentStar.LocalEvent.Start();
                    break;
            }
        }

        private void OnArrivedToPlanet(int planetId)
        {
            ShowPlanet(planetId);
        }

        private GameObject _relationsPanel;
    }
}
