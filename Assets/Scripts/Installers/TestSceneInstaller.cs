using UnityEngine;
using Zenject;
using Services.Resources;
using GameDatabase;
using Services.Localization;
using CommonComponents.Signals;
using Services.Audio;
using Services.Gui;
using GameServices.SceneManager;
using Services.ObjectPool;
using Services.Unity;

namespace Installers
{
    public class TestSceneInstaller : MonoInstaller<TestSceneInstaller>
    {
		[SerializeField] private ResourceLocator _resourceLocator;

        public override void InstallBindings()
        {
			Container.Bind<IGameObjectFactory>().To<GameObjectFactory>().AsCached();
			Container.Bind<IDatabase>().FromMethod(LoadDatabase).AsSingle();
			Container.Bind<IResourceLocator>().FromInstance(_resourceLocator);
			Container.Bind<ILocalization>().FromMethod(LoadLocalization).AsSingle();
			Container.BindInterfacesTo<ShaderTimeProvider>().AsSingle().NonLazy();
			Container.BindInterfacesTo<SoundPlayerStub>().AsCached();
			Container.BindInterfacesTo<MusicPlayerStub>().AsCached();
			Container.BindInterfacesTo<GuiManager>().AsSingle().NonLazy();

			BindSignals();
		}

		private static ILocalization LoadLocalization(InjectContext context)
		{
			var localization = context.Container.Instantiate<LocalizationManager>();
			localization.Initialize("English", context.Container.Resolve<IDatabase>());
			return localization;
		}

		private static IDatabase LoadDatabase(InjectContext context)
		{
			var database = new GameDatabase.Database();
			database.LoadDefault();
			return database;
		}

		private void BindSignals()
		{
			Container.BindSignal<LocalizationChangedSignal>();
			Container.BindTrigger<LocalizationChangedSignal.Trigger>();

			Container.BindSignal<WindowOpenedSignal>();
			Container.BindTrigger<WindowOpenedSignal.Trigger>();
			Container.BindSignal<WindowClosedSignal>();
			Container.BindTrigger<WindowClosedSignal.Trigger>();
			Container.BindSignal<WindowDestroyedSignal>();
			Container.BindTrigger<WindowDestroyedSignal.Trigger>();
			Container.BindSignal<EscapeKeyPressedSignal>();
			Container.BindTrigger<EscapeKeyPressedSignal.Trigger>();

			Container.BindSignal<SceneBeforeUnloadSignal>();
			Container.BindTrigger<SceneBeforeUnloadSignal.Trigger>();
			Container.BindSignal<SceneLoadedSignal>();
			Container.BindTrigger<SceneLoadedSignal.Trigger>();
			Container.BindSignal<SceneManagerStateChangedSignal>();
			Container.BindTrigger<SceneManagerStateChangedSignal.Trigger>();
		}
	}
}
