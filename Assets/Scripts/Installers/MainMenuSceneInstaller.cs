using Services.Messenger;
using GameServices.SceneManager;
using Zenject;

namespace Installers
{
    public class MainMenuSceneInstaller : MonoInstaller<MainMenuSceneInstaller>
    {
        public override void InstallBindings()
        {
			Container.BindAllInterfaces<Messenger>().To<Messenger>().AsSingle().WithArguments(GameScene.MainMenu);
		}
	}
}
