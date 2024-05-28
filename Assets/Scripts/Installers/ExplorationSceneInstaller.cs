using Combat;
using Combat.Ai;
using Combat.Collision.Manager;
using Services.Messenger;
using GameServices.SceneManager;
using Combat.Factory;
using Combat.Manager;
using Combat.Scene;
using Combat.Services;
using Game.Exploration;
using Gui.Combat;
using Services.ObjectPool;
using UnityEngine;
using Zenject;

namespace Installers
{
    public class ExplorationSceneInstaller : MonoInstaller<ExplorationSceneInstaller>
    {
        [SerializeField] private ShipStatsPanel _playerStatsPanel;
        [SerializeField] private ShipStatsPanel _enemyStatsPanel;
        [SerializeField] private ShipControlsPanel _shipControlsPanel;
        [SerializeField] private RadarPanel _radarPanel;
        [SerializeField] private Settings _settings;
        [SerializeField] private TrailRendererPool _trailRendererPool;
        [SerializeField] private Camera _camera;
        [SerializeField] private MaterialCache _materialCache;

        public override void InstallBindings()
        {
			Container.BindInterfacesTo<Messenger>().AsSingle().WithArguments(GameScene.Exploration);
			
			Container.BindInterfacesAndSelfTo<ExplorationData>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<ExplorationSceneManager>().AsSingle().NonLazy();
            Container.BindInterfacesTo<ExplorationViewRect>().AsTransient();
            Container.BindInterfacesTo<Scene>().AsSingle().WithArguments(new SceneSettings { AreaWidth = 1000, AreaHeight = 1000, PlayerAlwaysInCenter = true }).NonLazy();
            Container.BindInterfacesTo<CollisionManager>().AsSingle();
            Container.BindInterfacesTo<AiManager>().AsSingle().NonLazy();
            Container.Bind<ShipStatsPanel>().FromInstance(_playerStatsPanel).When(context => context.MemberName.Contains("player"));
            Container.Bind<ShipStatsPanel>().FromInstance(_enemyStatsPanel).When(context => context.MemberName.Contains("enemy"));
            Container.Bind<ShipControlsPanel>().FromInstance(_shipControlsPanel);
            Container.Bind<RadarPanel>().FromInstance(_radarPanel);
            Container.Bind<Settings>().FromInstance(_settings);
            Container.Bind<WeaponFactory>().AsSingle();
            Container.Bind<ShipFactory>().AsSingle().WithArguments(new ShipFactory.Settings { Shadows = true, StaticWrecks = true });
            Container.Bind<ControllerFactory>().AsSingle();
            Container.Bind<SpaceObjectFactory>().AsSingle();
            Container.Bind<DeviceFactory>().AsSingle();
            Container.Bind<DroneBayFactory>().AsSingle();
            Container.Bind<SatelliteFactory>().AsSingle();
            Container.Bind<EffectFactory>().AsSingle();
			Container.BindInterfacesAndSelfTo<GameObjectPool>().AsSingle();
			Container.Bind<TrailRendererPool>().FromInstance(_trailRendererPool);
			Container.Bind<IGameObjectFactory>().To<GameObjectFactory>().AsCached();
			Container.BindInterfacesTo<InputSystemMouse>().AsSingle().WithArguments(_camera);
            Container.BindInterfacesTo<InputSystemKeyboard>().AsSingle();
			Container.Bind<Combat.Ai.BehaviorTree.BehaviorTreeBuilder>().AsSingle();
			Container.BindInterfacesAndSelfTo<Combat.Helpers.RadioTransmitter>().AsSingle();
            Container.BindInterfacesTo<GameServicesProvider>().AsSingle();
            Container.BindInterfacesAndSelfTo<MaterialCache>().FromInstance(_materialCache);
		}
	}
}
