using Combat;
using Combat.Ai;
using Combat.Collision.Manager;
using Combat.Factory;
using Combat.Manager;
using Combat.Scene;
using Combat.Services;
using Services.ObjectPool;
using Services.Messenger;
using GameServices.SceneManager;
using UnityEngine;
using Zenject;

namespace Installers
{
    public class EhopediaSceneInstaller : MonoInstaller<EhopediaSceneInstaller>
    {
        [SerializeField] private Settings _settings;
        [SerializeField] private TrailRendererPool _trailRendererPool;

        public override void InstallBindings()
        {
			Container.BindInterfacesTo<Messenger>().AsSingle().WithArguments(GameScene.Ehopedia);

			Container.Bind<Settings>().FromInstance(_settings);
            Container.BindInterfacesAndSelfTo<EhopediaSceneManager>().AsSingle().NonLazy();
            Container.BindInterfacesTo<ViewRect>().AsTransient();
            Container.BindInterfacesTo<Scene>().AsSingle().WithArguments(new SceneSettings { AreaWidth = 200, AreaHeight = 200 }).NonLazy();
            Container.BindInterfacesTo<CollisionManager>().AsSingle();
            Container.BindInterfacesTo<AiManager>().AsSingle().NonLazy();
            Container.Bind<WeaponFactory>().AsSingle();
            Container.Bind<ShipFactory>().AsSingle().WithArguments(new ShipFactory.Settings());
            Container.Bind<SpaceObjectFactory>().AsSingle();
            Container.Bind<DeviceFactory>().AsSingle();
            Container.Bind<DroneBayFactory>().AsSingle();
            Container.Bind<SatelliteFactory>().AsSingle();
            Container.Bind<EffectFactory>().AsSingle();
			Container.BindInterfacesAndSelfTo<GameObjectPool>().AsSingle();
			Container.Bind<TrailRendererPool>().FromInstance(_trailRendererPool);
            Container.Bind<GameObjectFactory>().AsCached();
        }
    }
}
