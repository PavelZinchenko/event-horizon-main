using Zenject;
using Services.ObjectPool;

namespace Installers
{
    public class ObjectFactoryInstaller : MonoInstaller<ObjectFactoryInstaller>
    {
        public override void InstallBindings()
        {
            Container.Bind<IGameObjectFactory>().To<GameObjectFactory>().AsCached();
        }
	}
}
