using Services.Messenger;
using GameServices.SceneManager;
using Zenject;

namespace Installers
{
    public class ShipEditorSceneInstaller : MonoInstaller<ShipEditorSceneInstaller>
    {
        public override void InstallBindings()
        {
			Container.BindAllInterfaces<Messenger>().To<Messenger>().AsSingle().WithArguments(GameScene.ShipEditor);
		}
	}
}
