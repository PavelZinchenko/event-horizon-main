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
                _allyText.alignment = TextAnchor.MiddleCenter;
                _allyText.fontSize = Mathf.Max(12, _countText.fontSize - 3);
                var rect = _allyText.rectTransform;
                rect.anchoredPosition = _countText.rectTransform.anchoredPosition + new Vector2(0f, -24f);
                rect.sizeDelta = new Vector2(Mathf.Max(70f, _countText.rectTransform.sizeDelta.x * 1.8f), rect.sizeDelta.y);
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
