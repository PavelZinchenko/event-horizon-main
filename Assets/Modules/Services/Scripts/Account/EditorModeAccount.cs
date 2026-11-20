using Cysharp.Threading.Tasks;
using GameDatabase.Model;
using Services.Resources;
using UnityEngine;
using Zenject;

namespace Services.Account
{
    public class EditorModeAccount : IAccount
    {
        [Inject] private readonly IResourceLocator _resourceLocator;
        [Inject] private readonly AccountStatusChangedSignal.Trigger _accountStatusChangedTrigger;

        public void SignIn()
        {
            Status = Status.Connecting;
            UniTask.Delay(System.TimeSpan.FromSeconds(1)).ContinueWith(() => Status = Status.Connected).Forget();
        }

        public void SignOut()
        {
            Status = Status.NotConnected;
        }

        public bool CanSignOut => true;
        public bool CanSignIn => true;

        public Status Status
        {
            get { return _status; }
            set
            {
                if (_status == value)
                    return;

                _status = value;
                _accountStatusChangedTrigger.Fire(_status);
            }
        }

        public string DisplayName { get { return "Cheater"; } }
        public string Id { get { return "Cheater"; } }

        public async UniTask<Texture2D> LoadUserIcon()
        {
            await UniTask.Delay(System.TimeSpan.FromSeconds(1));
            var sprite = _resourceLocator.GetSprite(new SpriteId("M_07", SpriteId.Type.AvatarIcon));
            return sprite.texture;
        }

        private Status _status = Status.NotConnected;
    }
}
