using Diagnostics;
using GameDatabase;
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
using Services.Reources;
using Services.Storage;
using Services.Unity;
using Services.Assets;
using Services.GameApplication;
using UnityEngine;
using Zenject;
using GameServices.LevelManager;
using GameServices.Settings;
using GameServices.Gui;

namespace Installers
{
    public class ServicesInstaller : MonoInstaller<ServicesInstaller>
    {
        [SerializeField] private MusicPlayer _musicPlayer;
        [SerializeField] private SoundPlayer _soundPlayer;
        [SerializeField] private SystemFontLocator _systemFontLoader;

        public override void InstallBindings()
        {
            BindSignals();

            Container.BindAllInterfacesAndSelf<GameSettings>().To<GameSettings>().AsSingle().NonLazy();

            Container.Bind<IResourceLocator>().FromPrefabResource("ResourceLocator").AsSingle().NonLazy();

            Container.BindAllInterfaces<GameDatabase.Database>().To<GameDatabase.Database>().AsSingle().NonLazy();
            Container.BindAllInterfacesAndSelf<DatabaseStatistics>().To<DatabaseStatistics>().AsSingle().NonLazy();
            Container.BindAllInterfaces<DebugManager>().To<DebugManager>().AsSingle();

            Container.Bind<IMessenger>().To<Messenger>().AsSingle();

            Container.Bind<ILocalization>().To<LocalizationManager>().AsSingle();
            Container.BindAllInterfaces<KeyNameLocalizer>().To<KeyNameLocalizer>().AsSingle();
            Container.Bind<SystemFontLocator>().FromInstance(_systemFontLoader);

//#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
//            Container.BindAllInterfaces<DiscordController>().To<DiscordController>().AsSingle().NonLazy();
//#endif

            Container.Bind<ICoroutineManager>().To<CoroutineManager>().FromGameObject().AsSingle();
            Container.BindAllInterfaces<ShaderTimeProvider>().To<ShaderTimeProvider>().AsSingle().NonLazy();

            Container.BindAllInterfaces<GuiManager>().To<GuiManager>().AsSingle().NonLazy();
            Container.BindSignal<WindowOpenedSignal>();
            Container.BindTrigger<WindowOpenedSignal.Trigger>();
            Container.BindSignal<WindowClosedSignal>();
            Container.BindTrigger<WindowClosedSignal.Trigger>();
            Container.BindSignal<EscapeKeyPressedSignal>();
            Container.BindTrigger<EscapeKeyPressedSignal.Trigger>();

            Container.BindAllInterfacesAndSelf<IObjectPool>().To<GameObjectPool>().FromGameObject().AsSingle();
            Container.Bind<GameObjectFactory>();

            Container.Bind<IMusicPlayer>().To<MusicPlayer>().FromInstance(_musicPlayer);
            Container.Bind<ISoundPlayer>().To<SoundPlayer>().FromInstance(_soundPlayer);

#if UNITY_WEBGL
            Container.Bind<IApplication>().To<WebGlApplication>().FromGameObject().AsSingle().NonLazy();
#elif UNITY_STANDALONE
            Container.Bind<IApplication>().To<StandaloneApplication>().FromGameObject().AsSingle().NonLazy();
#elif UNITY_IPHONE || UNITY_ANDROID
            Container.Bind<IApplication>().To<MobileApplication>().FromGameObject().AsSingle().NonLazy();
#endif

            Container.Bind<PrefabCache>().To<PrefabCache>().FromGameObject().AsSingle();

            Container.BindAllInterfacesAndSelf<InternetTimeService>().To<InternetTimeService>().AsSingle().NonLazy();
            Container.BindSignal<ServerTimeReceivedSignal>();
            Container.BindTrigger<ServerTimeReceivedSignal.Trigger>();

#if LICENSE_OPENSOURCE
            Container.BindAllInterfaces<EmptyAccount>().To<EmptyAccount>().AsSingle();
#elif UNITY_EDITOR
            Container.BindAllInterfaces<EditorModeAccount>().To<EditorModeAccount>().AsSingle();
#elif UNITY_ANDROID && !NO_GPGS
            Container.BindAllInterfaces<GoogleAccount>().To<GoogleAccount>().AsSingle();
#elif UNITY_IPHONE
            Container.BindAllInterfaces<GameCenterAccount>().To<GameCenterAccount>().AsSingle();
#elif UNITY_STANDALONE
            Container.BindAllInterfaces<SteamAccount>().To<SteamAccount>().AsSingle();
#else
            Container.BindAllInterfaces<EmptyAccount>().To<EmptyAccount>().AsSingle();
#endif

#if LICENSE_OPENSOURCE
            Container.BindAllInterfaces<PlayerPrefsStorage>().To<PlayerPrefsStorage>().AsSingle();
#elif UNITY_WEBGL && !UNITY_EDITOR
            Container.BindAllInterfaces<PlayerPrefsStorage>().To<PlayerPrefsStorage>().AsSingle();
#elif UNITY_ANDROID && !UNITY_EDITOR
            Container.BindAllInterfaces<AndroidLocalStorage>().To<AndroidLocalStorage>().AsSingle();
#elif UNITY_STANDALONE_OSX && !UNITY_EDITOR
            Container.BindAllInterfaces<MacLocalStorage>().To<MacLocalStorage>().AsSingle();
#elif UNITY_STANDALONE_WIN && !UNITY_EDITOR
            Container.BindAllInterfaces<WindowsLocalStorage>().To<WindowsLocalStorage>().AsSingle();
#else
            Container.BindAllInterfaces<LocalStorage>().To<LocalStorage>().AsSingle();
#endif

#if LICENSE_OPENSOURCE
            Container.BindAllInterfaces<EmptyCloudStorage>().To<EmptyCloudStorage>().AsSingle();
#elif UNITY_ANDROID && !UNITY_EDITOR && !NO_GPGS
            Container.BindAllInterfaces<Services.GooglePlay.GooglePlayServices>().To<Services.GooglePlay.GooglePlayServices>().AsSingle().NonLazy();
            Container.BindAllInterfaces<GoogleCloudStorage>().To<GoogleCloudStorage>().AsSingle().NonLazy();
#elif UNITY_IPHONE && !UNITY_EDITOR
            Container.BindAllInterfaces<AppleCloudStorage>().To<AppleCloudStorage>().AsSingle();
#else
            Container.BindAllInterfaces<EmptyCloudStorage>().To<EmptyCloudStorage>().AsSingle();
#endif

#if ADS_DISABLED || LICENSE_OPENSOURCE
            Container.BindAllInterfaces<AdsManagerStub>().To<AdsManagerStub>().AsSingle().NonLazy();
#elif ADS_GOOGLE
            Container.BindAllInterfaces<GoogleAdsManager>().To<GoogleAdsManager>().AsSingle().NonLazy();
#elif ADS_UNITY
            Container.BindAllInterfaces<UnityAdsManager>().To<UnityAdsManager>().AsSingle().NonLazy();
#elif ADS_APPODEAL
            Container.BindAllInterfaces<AppodealAdsManager>().To<AppodealAdsManager>().AsSingle().NonLazy();
#endif

#if NO_INTERNET
            Container.BindAllInterfaces<NullRateGameService>().To<NullRateGameService>().AsSingle();
#elif UNITY_IOS
            Container.BindAllInterfaces<IosRateGameService>().To<IosRateGameService>().AsSingle();
#elif UNITY_ANDROID
            Container.BindAllInterfaces<AndroidRateGameService>().To<AndroidRateGameService>().AsSingle();
#else
            Container.BindAllInterfaces<NullRateGameService>().To<NullRateGameService>().AsSingle();
#endif

#if LICENSE_OPENSOURCE
            Container.BindAllInterfaces<EmptyStorage>().To<EmptyStorage>().AsSingle();
#elif UNITY_IOS && !UNITY_EDITOR
            Container.BindAllInterfaces<KeyChainStorage>().To<KeyChainStorage>().AsSingle();
#elif NO_INTERNET || IAP_DISABLED
            Container.BindAllInterfaces<EmptyStorage>().To<EmptyStorage>().AsSingle();
#else
            Container.BindAllInterfaces<WebStorage>().To<WebStorage>().AsSingle();
#endif

            Container.Bind<ILevelLoader>().To<LevelLoader>().AsSingle();

#if LICENSE_COMMERCIAL
            Container.BindAllInterfaces<AssetLoader>().To<AssetLoader>().AsSingle();
#else
            Container.BindAllInterfaces<LocalAssetLoader>().To<LocalAssetLoader>().AsSingle();
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
