using Cysharp.Threading.Tasks;
using Services.Account;
using Services.Messenger;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Gui.MainMenu
{
    public class SettingsAccount : MonoBehaviour
    {
        [SerializeField] RectTransform _signInButton;
        [SerializeField] RectTransform _signOutButton;
        [SerializeField] RawImage _accountIcon;
        [SerializeField] Text _accountName;
        [SerializeField] Image _notConnectedIcon;
        [SerializeField] Text _notConnectedText;
        [SerializeField] private GameObject[] _controls;

        [Inject] private readonly IAccount _account;

        [Inject]
        private void Initialize(IMessenger messenger)
        {
            messenger.AddListener<Status>(EventType.AccountStatusChanged, OnAccountChanged);
        }

        public void SignIn()
        {
            _account.SignIn();
        }

        public void SignOut()
        {
            _account.SignOut();
        }

        private void OnAccountChanged(Status status)
        {
            if (!this) 
                return;
            if (!gameObject.activeSelf)
                return;

            var connected = status == Status.Connected;
            _signInButton.gameObject.SetActive(!connected && _account.CanSignIn);
            _signOutButton.gameObject.SetActive(connected && _account.CanSignOut);

            _accountName.gameObject.SetActive(connected);
            _notConnectedText.gameObject.SetActive(!connected);

            _notConnectedIcon.gameObject.SetActive(!connected);
            _accountIcon.gameObject.SetActive(connected);

            if (connected)
                _accountName.text = _account.DisplayName;

            LoadUserIcon().Forget();
        }

        private async UniTaskVoid LoadUserIcon()
        {
            Texture2D icon;
            try
            {
                icon = await _account.LoadUserIcon();
            }
            catch (System.Exception e)
            {
                Debug.Log("SettingsAccount: failed to load the user icon - " + e.Message);
                return;
            }

            // A destroyed panel compares equal to null.
            if (icon != null && _accountIcon != null)
                _accountIcon.texture = icon;
        }

        private void OnEnable()
        {
            OnAccountChanged(_account.Status);
        }
    }
}
