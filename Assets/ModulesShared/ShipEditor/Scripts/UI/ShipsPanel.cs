using System.Collections.Generic;
using System.Linq;
using Constructor.Ships;
using GameDatabase;
using GameDatabase.Enums;
using Services.Localization;
using ShipEditor.Model;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace ShipEditor.UI
{
    public class ShipsPanel : MonoBehaviour
    {
        [Inject] private readonly IShipEditorModel _shipEditor;
        [Inject] private readonly ILocalization _localization;
        [Inject] private readonly IDatabase _database;

        [SerializeField] private ShipListContentFiller _contentFiller;
        [SerializeField] private ListScrollRect _shipList;

        private void Start()
        {
            _allShips.AddRange(_shipEditor.Inventory.Ships);
            CreateClassFilter();
            ApplyClassFilter();
        }

        private void CreateClassFilter()
        {
            var root = transform as RectTransform;
            if (root == null)
                return;

            var go = new GameObject("ShipClassFilter", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(root, false);
            rect.anchorMin = rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.anchoredPosition = new Vector2(-18f, -14f);
            rect.sizeDelta = new Vector2(190f, 46f);
            go.GetComponent<Image>().color = _database.UiSettings.ButtonColor;
            go.GetComponent<Button>().onClick.AddListener(CycleClassFilter);

            var textObject = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            var textRect = textObject.GetComponent<RectTransform>();
            textRect.SetParent(rect, false);
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = textRect.offsetMax = Vector2.zero;
            _filterLabel = textObject.GetComponent<Text>();
            _filterLabel.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _filterLabel.fontSize = 20;
            _filterLabel.alignment = TextAnchor.MiddleCenter;
            _filterLabel.color = _database.UiSettings.ButtonTextColor;
            _filterLabel.raycastTarget = false;
        }

        private void CycleClassFilter()
        {
            _filterIndex++;
            if (_filterIndex > (int)SizeClass.Starbase + 1)
                _filterIndex = 0;
            ApplyClassFilter();
        }

        private void ApplyClassFilter()
        {
            var selectedClass = _filterIndex == 0 ? (SizeClass?)null : (SizeClass)(_filterIndex - 1);
            IEnumerable<IShip> ships = selectedClass.HasValue
                ? _allShips.Where(ship => ship.Model.SizeClass == selectedClass.Value)
                : _allShips;
            _contentFiller.Initialize(ships);
            _shipList.RefreshContent();
            if (_filterLabel != null)
                _filterLabel.text = selectedClass.HasValue
                    ? "等级：" + selectedClass.Value.ToString(_localization)
                    : "等级：全部";
        }

        public void OnShipClicked(ShipItem item)
        {
            _shipEditor.SelectShip(item.Ship);
        }

        public bool Visible
        {
            get => gameObject.activeSelf;
            set => gameObject.SetActive(value);
        }

        private readonly List<IShip> _allShips = new();
        private Text _filterLabel;
        private int _filterIndex;
    }
}
