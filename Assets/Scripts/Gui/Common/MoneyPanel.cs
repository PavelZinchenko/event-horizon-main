using GameServices.Player;
using Services.Messenger;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using CommonComponents;

namespace Gui.Common
{
    public class MoneyPanel : MonoBehaviour
    {
        [SerializeField] private Text _creditsText;

        [Inject]
        private void Initialize(IMessenger messenger, PlayerResources playerResources)
        {
            messenger.AddListener<Money>(EventType.MoneyValueChanged, SetValue);
            SetValue(playerResources.Money);
        }

        private void SetValue(Money value)
        {
            if (_credits == value) return;
            _credits = value;
			_creditsText.text = _credits.ToString();
        }

        private Money _credits = -1;
    }
}
