using System;
using DataModel.Technology;
using Economy.Products;
using GameServices.Player;
using GameServices.Research;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Zenject;

namespace Gui.Craft
{
    public class CraftPanel : MonoBehaviour
    {
        [Inject] private readonly MotherShip _motherShip;
        [Inject] private readonly PlayerResources _resources;
        [Inject] private readonly PlayerSkills _playerSkills;
        [Inject] private readonly Research _research;
        [Inject] private readonly Session.ISessionData _session;

        [SerializeField] private ItemCreatedEvent _itemCreatedEvent = new ItemCreatedEvent();

        [SerializeField] private CraftItemQuality _itemQuality;
        [SerializeField] private Button _createButton;
        [SerializeField] private CraftPricePanel _craftPricePanel;

        [SerializeField] private Text _levelText;
        [SerializeField] private Color _enoughColor;
        [SerializeField] private Color _notEnoughColor;
        [SerializeField] private GameObject _notAvailablePanel;

        [Serializable]
        public class ItemCreatedEvent : UnityEvent<IProduct> {}

        public void Initialize(ITechnology tech, int level)
        {
            EnsureQuantityControls();
			if (_itemQuality != CraftItemQuality.Common && !CanCraftImprovedItem(tech))
            {
                Cleanup();
                _notAvailablePanel.gameObject.SetActive(true);
                return;
            }

            WorkshopLevel = level;
            _technology = tech;

            var price = tech.GetCraftPrice(_itemQuality)*_playerSkills.CraftingPriceScale;

            var requiredLevel = RequiredLevel;

            _notAvailablePanel.gameObject.SetActive(false);

            _levelText.text = requiredLevel.ToString();
            _levelText.color = requiredLevel <= level ? _enoughColor : _notEnoughColor;

            _craftPricePanel.gameObject.SetActive(true);
            _craftPricePanel.Initialize(price, tech.Faction);

            _createButton.interactable = requiredLevel <= level && _craftPricePanel.HaveEnoughResources;
        }

        public void Cleanup()
        {
            _technology = null;
            _craftPricePanel.gameObject.SetActive(false);
            _levelText.color = _enoughColor;
            _levelText.text = "0";
            _createButton.interactable = false;
            _notAvailablePanel.gameObject.SetActive(false);
        }

        public void CreateButtonClicked()
        {
            if (!TryConsumeResources(_quantity))
                return;

            IProduct lastItem = null;
            var seed = _session.Game.Seed + _session.Game.Counter + (int)_resources.Money + _motherShip.CurrentStar.Id;
            for (var i = 0; i < _quantity; i++)
            {
                lastItem = _technology.CreateItem(_itemQuality, new System.Random(seed + i * 7919));
                lastItem.Consume(1);
            }
            if (lastItem != null)
                _itemCreatedEvent.Invoke(lastItem);
        }

        private bool TryConsumeResources(int quantity)
        {
            if (RequiredLevel > WorkshopLevel)
                return false;

            var price = _technology.GetCraftPrice(_itemQuality)*(_playerSkills.CraftingPriceScale * quantity);
            if (price.Credits > _resources.Money || price.Stars > _resources.Stars)
                return false;
            if (price.Techs > 0 && _research.GetAvailablePoints(_technology.Faction) < price.Techs)
                return false;

            _resources.Money -= price.Credits;
			_resources.Stars -= price.Stars;
            _research.AddResearchPoints(_technology.Faction, -price.Techs);

            return true;
        }

        private void EnsureQuantityControls()
        {
            if (_quantityText != null) return;
            var row = new GameObject("Preview5Quantity", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            var rect = row.GetComponent<RectTransform>();
            rect.SetParent(_createButton.transform.parent, false);
            rect.SetSiblingIndex(_createButton.transform.GetSiblingIndex() + 1);
            rect.sizeDelta = new Vector2(150f, 36f);
            var layout = row.GetComponent<HorizontalLayoutGroup>();
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;

            CreateQuantityButton(rect, "-", () => SetQuantity(_quantity - 1));
            var label = new GameObject("Quantity", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            label.transform.SetParent(rect, false);
            _quantityText = label.GetComponent<Text>();
            _quantityText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _quantityText.alignment = TextAnchor.MiddleCenter;
            _quantityText.color = Color.white;
            CreateQuantityButton(rect, "+", () => SetQuantity(_quantity + 1));
            SetQuantity(1);
        }

        private void CreateQuantityButton(Transform parent, string label, UnityAction action)
        {
            var go = new GameObject(label, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = new Color(0.08f, 0.25f, 0.32f, 0.95f);
            go.GetComponent<Button>().onClick.AddListener(action);
            var textObject = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            var rect = textObject.GetComponent<RectTransform>();
            rect.SetParent(go.transform, false);
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            var text = textObject.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.text = label;
        }

        private void SetQuantity(int value)
        {
            _quantity = Mathf.Clamp(value, 1, 99);
            _quantityText.text = _quantity.ToString();
        }

		private static bool CanCraftImprovedItem(ITechnology tech)
		{
			switch (tech)
			{
				case SatelliteTechnology:
					return false;
				case ComponentTechnology componentTech:
					return componentTech.Component.PossibleModifications.Count > 0;
				default:
					return true;
			}
		}

		private int WorkshopLevel { get; set; }
        private int RequiredLevel { get { return Math.Max(0, _itemQuality.GetWorkshopLevel(_technology.GetWorkshopLevel()) + _playerSkills.CraftingLevelModifier); } }

		private ITechnology _technology;
        private int _quantity = 1;
        private Text _quantityText;
    }
}
