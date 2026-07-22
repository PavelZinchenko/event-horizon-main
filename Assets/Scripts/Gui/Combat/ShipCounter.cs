using Services.Messenger;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Combat.Manager;

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

            if (visible && _allyOrderText != null)
                _allyOrderText.text = GetAllyOrderText();
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
            panelRect.anchoredPosition = new Vector2(-210f, -82f);
            panelRect.sizeDelta = new Vector2(180f, 96f);

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
            textRect.anchorMin = new Vector2(0f, 0.5f);
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(4f, 0f);
            textRect.offsetMax = new Vector2(-4f, 0f);

            var orderButton = new GameObject("AllyOrderButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            orderButton.layer = canvas.gameObject.layer;
            orderButton.transform.SetParent(panel.transform, false);
            var orderRect = orderButton.GetComponent<RectTransform>();
            orderRect.anchorMin = Vector2.zero;
            orderRect.anchorMax = new Vector2(1f, 0.5f);
            orderRect.offsetMin = new Vector2(6f, 4f);
            orderRect.offsetMax = new Vector2(-6f, -3f);
            var orderBackground = orderButton.GetComponent<Image>();
            orderBackground.color = new Color(0.08f, 0.38f, 0.54f, 0.94f);
            // Clone the scene's localized counter text so the button inherits
            // the game's Chinese-capable font. LegacyRuntime.ttf has no CJK
            // glyphs on Android and produced a visually empty button.
            var orderText = Instantiate(_countText, orderRect);
            orderText.name = "Label";
            orderText.transform.SetParent(orderRect, false);
            var orderTextRect = orderText.rectTransform;
            orderTextRect.anchorMin = Vector2.zero;
            orderTextRect.anchorMax = Vector2.one;
            orderTextRect.offsetMin = orderTextRect.offsetMax = Vector2.zero;
            orderText.fontSize = Mathf.Max(13, _countText.fontSize - 4);
            orderText.resizeTextForBestFit = true;
            orderText.resizeTextMinSize = 11;
            orderText.resizeTextMaxSize = Mathf.Max(13, _countText.fontSize - 2);
            orderText.alignment = TextAnchor.MiddleCenter;
            orderText.color = Color.white;
            orderText.raycastTarget = false;
            orderText.text = GetAllyOrderText();
            orderButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                _manager.CycleAllyOrder();
                orderText.text = GetAllyOrderText();
            });
            _allyOrderText = orderText;
        }

        private string GetAllyOrderText()
        {
            return _manager == null ? "友军指令：自由" : "友军指令：" + _manager.AllyOrderName;
        }

        private global::Combat.Manager.CombatManager _manager;
        private Text _allyText;
        private Text _allyOrderText;
    }
}
