using Diagnostics;
using Services.Account;
using Services.Advertisements;
using Services.Audio;
using Services.IapStorage;
using Services.Gui;
using Services.InternetTime;
using Services.Localization;
using Services.Messenger;
using Services.ObjectPool;
using Services.RateGame;
using Services.Resources;
using Services.Storage;
using Services.Unity;
using Services.Assets;
using Services.GameApplication;
using UnityEngine;
using Zenject;
using GameServices.SceneManager;
using GameServices.Settings;
using GameServices.Gui;
using CommonComponents.Signals;

namespace Installers
{
    public class ServicesInstaller : MonoInstaller<ServicesInstaller>
    {
        [SerializeField] private MusicPlayer _musicPlayer;
        [SerializeField] private SoundPlayer _soundPlayer;
		[SerializeField] private ResourceLocator _resourceLocator;
        [SerializeField] private Gui.Theme.UiTheme _defaultUiTheme;

		public override void InstallBindings()
        {
            BindSignals();

            Container.BindInterfacesAndSelfTo<GameSettings>().AsSingle().NonLazy();

            Container.Bind<IResourceLocator>().To<ResourceLocator>().FromInstance(_resourceLocator);

            Container.BindInterfacesTo<GameDatabase.Database>().AsSingle().NonLazy();
            Container.BindInterfacesTo<Gui.DebugConsole.DebugConsoleLogger>().AsSingle();

			Container.Bind<IMessengerContext>().To<MessengerContext>().AsSingle();

			Container.Bind<ILocalization>().To<LocalizationManager>().AsSingle();
            Container.BindInterfacesTo<KeyNameLocalizer>().AsSingle();

            //#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            //            Container.BindAllInterfaces<DiscordController>().AsSingle().NonLazy();
            //#endif

            Container.BindInterfacesTo<Gui.Theme.UiThemeLoader>().AsSingle().WithArguments(_defaultUiTheme);

            Container.Bind<ICoroutineManager>().To<CoroutineManager>().FromNewComponentOnNewGameObject().AsSingle();
            Container.BindInterfacesTo<ShaderTimeProvider>().AsSingle().NonLazy();

            Container.BindInterfacesTo<GuiManager>().AsSingle().NonLazy();
            Container.BindSignal<WindowOpenedSignal>();
            Container.BindTrigger<WindowOpenedSignal.Trigger>();
			Container.BindSignal<WindowClosedSignal>();
			Container.BindTrigger<WindowClosedSignal.Trigger>();
			Container.BindSignal<WindowDestroyedSignal>();
			Container.BindTrigger<WindowDestroyedSignal.Trigger>();
			Container.BindSignal<EscapeKeyPressedSignal>();
            Container.BindTrigger<EscapeKeyPressedSignal.Trigger>();

			Container.Bind<IGameObjectFactory>().To<GameObjectFactory>().AsCached();

			Container.Bind<IMusicPlayer>().To<MusicPlayer>().FromInstance(_musicPlayer);
            Container.Bind<ISoundPlayer>().To<SoundPlayer>().FromInstance(_soundPlayer);

#if UNITY_WEBGL
            Container.Bind<IApplication>().To<WebGlApplication>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
#elif UNITY_STANDALONE
			Container.Bind<IApplication>().To<StandaloneApplication>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
#elif UNITY_IPHONE || UNITY_ANDROID
            Container.Bind<IApplication>().To<MobileApplication>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
#endif

			Container.Bind<PrefabCache>().FromNewComponentOnNewGameObject().AsSingle();

            Container.BindInterfacesAndSelfTo<InternetTimeService>().AsSingle().NonLazy();
            Container.BindSignal<ServerTimeReceivedSignal>();
            Container.BindTrigger<ServerTimeReceivedSignal.Trigger>();

#if LICENSE_OPENSOURCE
            Container.BindInterfacesTo<EmptyAccount>().AsSingle();
#elif UNITY_EDITOR
			Container.BindInterfacesTo<EditorModeAccount>().AsSingle();
#elif UNITY_ANDROID && !NO_GPGS
            Container.BindInterfacesTo<GoogleAccount>().AsSingle();
#elif UNITY_IPHONE
            Container.BindInterfacesTo<GameCenterAccount>().AsSingle();
#elif UNITY_STANDALONE
            Container.BindInterfacesTo<SteamAccount>().AsSingle();
#else
            Container.BindInterfacesTo<EmptyAccount>().AsSingle();
#endif

#if LICENSE_OPENSOURCE
            Container.BindInterfacesTo<PlayerPrefsStorage>().AsSingle();
#elif UNITY_WEBGL && !UNITY_EDITOR
            Container.BindInterfacesTo<PlayerPrefsStorage>().AsSingle();
#elif UNITY_ANDROID && !UNITY_EDITOR
            Container.BindInterfacesTo<AndroidLocalStorage>().AsSingle();
#elif UNITY_STANDALONE_OSX && !UNITY_EDITOR
            Container.BindInterfacesTo<MacLocalStorage>().AsSingle();
#elif UNITY_STANDALONE_WIN && !UNITY_EDITOR
            Container.BindInterfacesTo<WindowsLocalStorage>().AsSingle();
#else
			Container.BindInterfacesTo<LocalStorage>().AsSingle();
#endif

#if LICENSE_OPENSOURCE
            Container.BindInterfacesTo<EmptyCloudStorage>().AsSingle();
#elif UNITY_ANDROID && !UNITY_EDITOR && !NO_GPGS
            Container.BindInterfacesTo<Services.GooglePlay.GooglePlayServices>().AsSingle().NonLazy();
            Container.BindInterfacesTo<GoogleCloudStorage>().AsSingle().NonLazy();
#elif UNITY_IPHONE && !UNITY_EDITOR
            Container.BindInterfacesTo<AppleCloudStorage>().AsSingle();
#else
			Container.BindInterfacesTo<EmptyCloudStorage>().AsSingle();
#endif

#if ADS_DISABLED || LICENSE_OPENSOURCE
            Container.BindInterfacesTo<AdsManagerStub>().AsSingle().NonLazy();
#elif ADS_GOOGLE
			Container.BindInterfacesTo<GoogleAdsManager>().AsSingle().NonLazy();
#elif ADS_UNITY
            Container.BindInterfacesTo<UnityAdsManager>().AsSingle().NonLazy();
#elif ADS_APPODEAL
            Container.BindInterfacesTo<AppodealAdsManager>().AsSingle().NonLazy();
#endif

#if NO_INTERNET
            Container.BindInterfacesTo<NullRateGameService>().AsSingle();
#elif UNITY_IOS
            Container.BindInterfacesTo<IosRateGameService>().AsSingle();
#elif UNITY_ANDROID
			Container.BindInterfacesTo<AndroidRateGameService>().AsSingle();
#else
            Container.BindInterfacesTo<NullRateGameService>().AsSingle();
#endif

#if LICENSE_OPENSOURCE
            Container.BindInterfacesTo<EmptyStorage>().AsSingle();
#elif UNITY_IOS && !UNITY_EDITOR
            Container.BindInterfacesTo<KeyChainStorage>().AsSingle();
#elif NO_INTERNET || IAP_DISABLED
            Container.BindInterfacesTo<EmptyStorage>().AsSingle();
#else
			Container.BindInterfacesTo<WebStorage>().AsSingle();
#endif

			Container.BindInterfacesTo<GameSceneManager>().FromNewComponentOnNewGameObject().AsSingle();

#if LICENSE_COMMERCIAL
			Container.BindInterfacesTo<AssetLoader>().AsSingle();
#else
            Container.BindInterfacesTo<LocalAssetLoader>().AsSingle();
#endif
		}

		public void BindSignals()
        {
            Container.BindSignal<CloudStorageStatusChangedSignal>();
            Container.BindTrigger<CloudStorageStatusChangedSignal.Trigger>();
            Container.BindSignal<CloudLoadingCompletedSignal>();
            Container.BindTrigger<CloudLoadingCompletedSignal.Trigger>();
            Container.BindSignal<CloudSavingCompletedSignal>();
            Container.BindTrigger<CloudSavingCompletedSignal.Trigger>();
            Container.BindSignal<CloudOperationFailedSignal>();
            Container.BindTrigger<CloudOperationFailedSignal.Trigger>();
            Container.BindSignal<AccountStatusChangedSignal>();
            Container.BindTrigger<AccountStatusChangedSignal.Trigger>();
            Container.BindSignal<CloudSavedGamesReceivedSignal>();
            Container.BindTrigger<CloudSavedGamesReceivedSignal.Trigger>();

            Container.BindSignal<SceneBeforeUnloadSignal>();
            Container.BindTrigger<SceneBeforeUnloadSignal.Trigger>();
            Container.BindSignal<SceneLoadedSignal>();
            Container.BindTrigger<SceneLoadedSignal.Trigger>();
			Container.BindSignal<SceneManagerStateChangedSignal>();
			Container.BindTrigger<SceneManagerStateChangedSignal.Trigger>();
			Container.BindSignal<GamePausedSignal>();
            Container.BindTrigger<GamePausedSignal.Trigger>();
            Container.BindSignal<AppActivatedSignal>();
            Container.BindTrigger<AppActivatedSignal.Trigger>();
            Container.BindSignal<ShowMessageSignal>();
            Container.BindTrigger<ShowMessageSignal.Trigger>();
            Container.BindSignal<ShowDebugMessageSignal>();
            Container.BindTrigger<ShowDebugMessageSignal.Trigger>();
            Container.BindSignal<RewardedVideoCompletedSignal>();
            Container.BindTrigger<RewardedVideoCompletedSignal.Trigger>();
            Container.BindSignal<AdStatusChangedSignal>();
            Container.BindTrigger<AdStatusChangedSignal.Trigger>();
            Container.BindSignal<IapDataSavedSignal>();
            Container.BindTrigger<IapDataSavedSignal.Trigger>();
            Container.BindSignal<AssetLoaderStatusChangedSignal>();
            Container.BindTrigger<AssetLoaderStatusChangedSignal.Trigger>();
            Container.BindSignal<LocalizationChangedSignal>();
            Container.BindTrigger<LocalizationChangedSignal.Trigger>();
        }
    }
}
