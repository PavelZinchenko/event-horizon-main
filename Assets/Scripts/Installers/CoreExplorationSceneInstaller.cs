using Combat.Ai;
using Combat.Integration.Core;
using Game.Exploration;
using GameServices.SceneManager;
using Gui.Combat;
using Services.Messenger;
using UnityEngine;
using Zenject;

namespace Installers
{
    // The exploration scene on the shared combat core (exploration plan, step 9). Binds only the new runtime and the
    // exploration HUD; no legacy scene manager, scene, AI or factories. ExplorationState binds the Planet.
    public class CoreExplorationSceneInstaller : MonoInstaller<CoreExplorationSceneInstaller>
    {
        [SerializeField] private ShipStatsPanel _playerStatsPanel;
        [SerializeField] private ShipStatsPanel _enemyStatsPanel;
        [SerializeField] private ShipControlsPanel _shipControlsPanel;
        [SerializeField] private RadarPanel _radarPanel;
        [SerializeField] private Camera _camera;

        public override void InstallBindings()
        {
            Container.BindInterfacesTo<Messenger>().AsSingle().WithArguments(GameScene.CoreExploration);

            Container.BindInterfacesAndSelfTo<ExplorationData>().AsSingle();
            Container.BindInterfacesAndSelfTo<CoreExplorationAdapter>().AsSingle().WithArguments(gameObject.scene).NonLazy();
            Container.BindInterfacesTo<CoreExplorationHudPresenter>().AsSingle().NonLazy();

            Container.Bind<ShipStatsPanel>().FromInstance(_playerStatsPanel).When(context => context.MemberName.Contains("player"));
            Container.Bind<ShipStatsPanel>().FromInstance(_enemyStatsPanel).When(context => context.MemberName.Contains("enemy"));
            Container.Bind<ShipControlsPanel>().FromInstance(_shipControlsPanel);
            Container.Bind<RadarPanel>().FromInstance(_radarPanel);

            Container.BindInterfacesTo<InputSystemMouse>().AsSingle().WithArguments(_camera);
            Container.BindInterfacesTo<InputSystemKeyboard>().AsSingle();

            // After the mouse reader and before the adapter, so the frame's input reaches the frame's steps.
            Container.BindInterfacesTo<CoreKeyboardDriver>().AsSingle();
            Container.BindExecutionOrder<InputSystemMouse>(-2);
            Container.BindExecutionOrder<CoreKeyboardDriver>(-1);
        }
    }
}
