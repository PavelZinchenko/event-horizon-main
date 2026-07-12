using Services.Messenger;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Gui.Combat
{
    public class ShipCounter : MonoBehaviour
    {
        public enum Type
        {
            Player,
            Enemy,
        }

        [SerializeField] private Text _countText;
        [SerializeField] private Type _type;

        [Inject]
        private void Initialize(IMessenger messenger, global::Combat.Manager.CombatManager manager)
        {
            _manager = manager;
            messenger.AddListener<int>(_type == Type.Player ? EventType.PlayerShipCountChanged : EventType.EnemyShipCountChanged, OnShipCountChanged);
        }

        private void OnShipCountChanged(int count)
        {
            _countText.text = count.ToString();
            UpdateAllyCount();
        }

        private void Update()
        {
            if (_type == Type.Enemy)
                UpdateAllyCount();
        }

        private void UpdateAllyCount()
        {
            if (_type != Type.Enemy || _manager == null || _countText == null)
                return;

            var visible = _manager.HasAlliedParticipants;
            if (visible && _allyText == null)
                CreateAllyPopup();

            if (_allyText != null)
                _allyText.transform.parent.gameObject.SetActive(visible);

            if (visible && _allyText != null)
                _allyText.text = "友军：" + _manager.RemainingAllyCount;
        }

        private void CreateAllyPopup()
        {
            var canvas = _countText.GetComponentInParent<Canvas>();
            if (canvas == null) return;

            var panel = new GameObject("AllyShipCountPopup", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            panel.layer = canvas.gameObject.layer;
            panel.transform.SetParent(canvas.transform, false);
            panel.transform.SetAsLastSibling();

            var panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 1f);
            panelRect.anchorMax = new Vector2(0.5f, 1f);
            panelRect.pivot = new Vector2(0.5f, 1f);
            // Keep the ally counter in its own top-level popup, well clear of
            // the enemy counter and the right-side target list.
            panelRect.anchoredPosition = new Vector2(-210f, -18f);
            panelRect.sizeDelta = new Vector2(180f, 48f);

            var background = panel.GetComponent<Image>();
            background.color = new Color(0.02f, 0.16f, 0.24f, 0.88f);
            background.raycastTarget = false;

            _allyText = Instantiate(_countText, panel.transform);
            _allyText.name = "AllyShipCount";
            _allyText.alignment = TextAnchor.MiddleCenter;
            _allyText.fontSize = Mathf.Max(14, _countText.fontSize - 2);
            _allyText.resizeTextForBestFit = false;
            _allyText.horizontalOverflow = HorizontalWrapMode.Overflow;
            _allyText.verticalOverflow = VerticalWrapMode.Overflow;
            _allyText.color = new Color(0.4f, 0.85f, 1f, 1f);
            _allyText.raycastTarget = false;
            var textRect = _allyText.rectTransform;
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
        }

        private global::Combat.Manager.CombatManager _manager;
        private Text _allyText;
    }
}
