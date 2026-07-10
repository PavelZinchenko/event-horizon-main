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

            if (_allyText == null)
            {
                _allyText = Instantiate(_countText, _countText.transform.parent);
                _allyText.name = "AllyShipCount";
                _allyText.alignment = TextAnchor.UpperCenter;
                _allyText.fontSize = Mathf.Max(12, _countText.fontSize - 3);
                _allyText.resizeTextForBestFit = false;
                _allyText.horizontalOverflow = HorizontalWrapMode.Overflow;
                _allyText.verticalOverflow = VerticalWrapMode.Overflow;
                var rect = _allyText.rectTransform;
                var enemyRect = _countText.rectTransform;
                var verticalGap = Mathf.Max(34f, enemyRect.rect.height + 8f);
                rect.anchoredPosition = enemyRect.anchoredPosition + new Vector2(0f, -verticalGap);
                rect.sizeDelta = new Vector2(Mathf.Max(150f, enemyRect.rect.width * 2.4f), Mathf.Max(28f, enemyRect.rect.height));
            }

            var visible = _manager.HasAlliedParticipants;
            _allyText.gameObject.SetActive(visible);
            if (visible)
                _allyText.text = "友军 " + _manager.RemainingAllyCount;
        }

        private global::Combat.Manager.CombatManager _manager;
        private Text _allyText;
    }
}
