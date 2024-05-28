﻿using Combat;
using Combat.Ai;
using Combat.Collision.Manager;
using Combat.Factory;
using Combat.Scene;
using Combat.Services;
using Gui.Combat;
using Services.ObjectPool;
using Services.Messenger;
using GameServices.SceneManager;
using UnityEngine;
using Zenject;

namespace Installers
{
    public class ConfigSceneInstaller : MonoInstaller<ConfigSceneInstaller>
    {
        [SerializeField] private Settings _settings;
        [SerializeField] private ShipControlsPanel _shipControlsPanel;
        [SerializeField] private TrailRendererPool _trailRendererPool;
        [SerializeField] private Camera _camera;
        [SerializeField] private MaterialCache _materialCache;

        public override void InstallBindings()
        {
			Container.BindInterfacesTo<Messenger>().AsSingle().WithArguments(GameScene.ConfigureControls);

			Container.Bind<Settings>().FromInstance(_settings);
            Container.BindInterfacesTo<ViewRect>().AsTransient();
            Container.BindInterfacesTo<Scene>().AsSingle().WithArguments(new SceneSettings { AreaWidth = 200, AreaHeight = 200 }).NonLazy();
            Container.BindInterfacesTo<CollisionManager>().AsSingle();
            Container.BindInterfacesTo<AiManager>().AsSingle().NonLazy();
			Container.Bind<ShipControlsPanel>().FromInstance(_shipControlsPanel);
            Container.Bind<WeaponFactory>().AsSingle();
            Container.Bind<ShipFactory>().AsSingle().WithArguments(new ShipFactory.Settings());
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
