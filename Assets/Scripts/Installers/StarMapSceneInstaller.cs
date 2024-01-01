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
			Container.BindInterfacesTo<Messenger>().AsSingle().WithArguments(GameScene.StarMap);
			Container.BindInterfacesAndSelfTo<GameObjectPool>().AsSingle();
			Container.Bind<GameObjectFactory>().AsCached();
		}
	}
}
