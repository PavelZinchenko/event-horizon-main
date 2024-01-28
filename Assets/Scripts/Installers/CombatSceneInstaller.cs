using Combat;
using Combat.Ai;
using Combat.Background;
using Combat.Collision.Manager;
using Combat.Factory;
using Combat.Manager;
using Combat.Scene;
using Combat.Services;
using Gui.Combat;
using Services.ObjectPool;
using Services.Settings;
using Services.Messenger;
using GameServices.SceneManager;
using UnityEngine;
using Zenject;

namespace Installers
{
    public class CombatSceneInstaller : MonoInstaller<CombatSceneInstaller>
    {
        [SerializeField] private ShipStatsPanel _playerStatsPanel;
        [SerializeField] private ShipStatsPanel _enemyStatsPanel;
        [SerializeField] private ShipControlsPanel _shipControlsPanel;
        [SerializeField] private ShipSelectionPanel _shipSelectionPanel;
        [SerializeField] private RadarPanel _radarPanel;
        [SerializeField] private CombatMenu _combatMenu;
        [SerializeField] private TimerPanel _timerPanel;
        [SerializeField] private Settings _settings;
        [SerializeField] private CombatBackground _background;
        [SerializeField] private TrailRendererPool _trailRendererPool;
        [SerializeField] private Camera _camera;

        [Inject] private readonly IGameSettings _gameSettings;

        public override void InstallBindings()
        {
			var settings = new ShipFactory.Settings()
			{
				NoDamageIndicator = !_gameSettings.ShowDamage,
				NoEnemyMessages = !_gameSettings.ShowEnemyMessages,
			};

			Container.BindInterfacesTo<Messenger>().AsSingle().WithArguments(GameScene.Combat);

			Container.BindInterfacesAndSelfTo<CombatManager>().AsSingle().NonLazy();
            Container.BindInterfacesTo<ViewRect>().AsTransient();
            Container.BindInterfacesTo<Scene>().AsSingle().WithArguments(new SceneSettings { AreaWidth = 200, AreaHeight = 200 }).NonLazy();
            Container.BindInterfacesTo<CollisionManager>().AsSingle();
            Container.BindInterfacesTo<AiManager>().AsSingle().NonLazy();
			Container.Bind<Combat.Ai.BehaviorTree.BehaviorTreeBuilder>().AsSingle();
			Container.BindInterfacesAndSelfTo<CombatTimer>().AsSingle().NonLazy();
            Container.Bind<ShipStatsPanel>().FromInstance(_playerStatsPanel).When(context => context.MemberName.Contains("player"));
            Container.Bind<ShipStatsPanel>().FromInstance(_enemyStatsPanel).When(context => context.MemberName.Contains("enemy"));
            Container.Bind<ShipControlsPanel>().FromInstance(_shipControlsPanel);
            Container.Bind<ShipSelectionPanel>().FromInstance(_shipSelectionPanel);
            Container.Bind<RadarPanel>().FromInstance(_radarPanel);
            Container.Bind<CombatBackground>().FromInstance(_background);
            Container.Bind<CombatMenu>().FromInstance(_combatMenu);
            Container.Bind<TimerPanel>().FromInstance(_timerPanel);
            Container.Bind<Settings>().FromInstance(_settings);
            Container.Bind<WeaponFactory>().AsSingle();
            Container.Bind<ShipFactory>().AsSingle().WithArguments(settings);
            Container.Bind<SpaceObjectFactory>().AsSingle();
            Container.Bind<DeviceFactory>().AsSingle();
            Container.Bind<DroneBayFactory>().AsSingle();
            Container.Bind<SatelliteFactory>().AsSingle();
			Container.Bind<EffectFactory>().AsSingle();
			Container.BindInterfacesAndSelfTo<Combat.Helpers.RadioTransmitter>().AsSingle();
			Container.BindInterfacesAndSelfTo<GameObjectPool>().AsSingle();
            Container.Bind<TrailRendererPool>().FromInstance(_trailRendererPool);
            Container.Bind<IGameObjectFactory>().To<GameObjectFactory>().AsCached();
            Container.BindInterfacesTo<InputSystemMouse>().AsSingle().WithArguments(_camera);
            Container.BindInterfacesTo<InputSystemKeyboard>().AsSingle();
        }
    }
}
