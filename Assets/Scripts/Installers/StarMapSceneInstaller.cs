using Services.Messenger;
using GameServices.SceneManager;
using Zenject;
using Services.ObjectPool;

namespace Installers
{
    public class StarMapSceneInstaller : MonoInstaller<StarMapSceneInstaller>
    {
        public override void InstallBindings()
        {
			Container.BindAllInterfaces<Messenger>().To<Messenger>().AsSingle().WithArguments(GameScene.StarMap);
			Container.BindAllInterfacesAndSelf<GameObjectPool>().To<GameObjectPool>().AsSingle();
			Container.Bind<GameObjectFactory>();
		}
	}
}
