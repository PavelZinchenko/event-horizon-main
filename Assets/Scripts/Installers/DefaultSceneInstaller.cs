using UnityEngine;
using Zenject;
using Services.Messenger;
using GameServices.SceneManager;

namespace Installers
{
    public class DefaultSceneInstaller : MonoInstaller<DefaultSceneInstaller>
    {
		[SerializeField] private GameScene _scene;

        public override void InstallBindings()
        {
			Container.BindInterfacesTo<Messenger>().AsSingle().WithArguments(_scene);
		}
	}
}
