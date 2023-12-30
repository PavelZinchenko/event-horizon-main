using Services.Messenger;
using GameServices.SceneManager;
using Zenject;

namespace Installers
{
    public class CommonGuiSceneInstaller : MonoInstaller<CommonGuiSceneInstaller>
    {
        public override void InstallBindings()
        {
			Container.BindAllInterfaces<Messenger>().To<Messenger>().AsSingle().WithArguments(GameScene.CommonGui);
		}
	}
}
