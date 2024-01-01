using Services.Messenger;
using GameServices.SceneManager;
using Zenject;

namespace Installers
{
    public class CommonGuiSceneInstaller : MonoInstaller<CommonGuiSceneInstaller>
    {
        public override void InstallBindings()
        {
			Container.BindInterfacesTo<Messenger>().AsSingle().WithArguments(GameScene.CommonGui);
		}
	}
}
