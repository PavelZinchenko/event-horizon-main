using Combat.Ai;
using Combat.Background;
using Combat.Integration.Core;
using GameServices.SceneManager;
using Gui.Combat;
using Services.Messenger;
using UnityEngine;
using Zenject;

namespace Installers
{
    // The combat scene on the shared combat core (migration phase 3c). Binds only the new runtime and the
    // HUD that reads the integration contracts; no legacy combat manager, scene, AI or factories.
    public class CoreCombatSceneInstaller : MonoInstaller<CoreCombatSceneInstaller>
    {
        [SerializeField] private ShipStatsPanel _playerStatsPanel;
        [SerializeField] private ShipStatsPanel _enemyStatsPanel;
        [SerializeField] private ShipControlsPanel _shipControlsPanel;
        [SerializeField] private ShipSelectionPanel _shipSelectionPanel;
        [SerializeField] private RadarPanel _radarPanel;
        [SerializeField] private CombatMenu _combatMenu;
        [SerializeField] private TimerPanel _timerPanel;
        [SerializeField] private CombatBackground _background;
        [SerializeField] private Camera _camera;

        public override void InstallBindings()
        {
            Container.BindInterfacesTo<Messenger>().AsSingle().WithArguments(GameScene.CoreCombat);

            Container.BindInterfacesAndSelfTo<CoreCombatAdapter>().AsSingle().WithArguments(gameObject.scene).NonLazy();
            Container.BindInterfacesTo<CoreScenePresentation>().AsSingle();
            Container.BindInterfacesTo<CombatTimerPresenter>().AsSingle().NonLazy();
            Container.BindExecutionOrder<CombatTimerPresenter>(1);
            Container.BindInterfacesTo<CombatHudPresenter>().AsSingle().NonLazy();

            Container.Bind<ShipStatsPanel>().FromInstance(_playerStatsPanel).When(context => context.MemberName.Contains("player"));
            Container.Bind<ShipStatsPanel>().FromInstance(_enemyStatsPanel).When(context => context.MemberName.Contains("enemy"));
            Container.Bind<ShipControlsPanel>().FromInstance(_shipControlsPanel);
            Container.Bind<ShipSelectionPanel>().FromInstance(_shipSelectionPanel);
            Container.Bind<RadarPanel>().FromInstance(_radarPanel);
            Container.Bind<CombatBackground>().FromInstance(_background);
            Container.Bind<CombatMenu>().FromInstance(_combatMenu);
            Container.Bind<TimerPanel>().FromInstance(_timerPanel);

            Container.BindInterfacesTo<InputSystemMouse>().AsSingle().WithArguments(_camera);
            Container.BindInterfacesTo<InputSystemKeyboard>().AsSingle();

            // After the mouse reader and before the adapter, so the frame's input reaches the frame's steps.
            Container.BindInterfacesTo<CoreKeyboardDriver>().AsSingle();
            Container.BindExecutionOrder<InputSystemMouse>(-2);
            Container.BindExecutionOrder<CoreKeyboardDriver>(-1);
        }
    }
}
