using System;
using System.Collections.Generic;
using System.Linq;
using Combat.Domain;
using CombatSimulation.Core;
using CombatSimulation.Modes;
using CombatSimulation.Runtime;
using CombatView;
using Constructor;
using Economy.Products;
using Game.Exploration;
using GameDatabase;
using GameServices.Player;
using GameStateMachine.States;
using Services.Audio;
using Services.Gui;
using Services.Localization;
using Services.Messenger;
using Services.Resources;
using Services.Settings;
using UnityEngine;
using Zenject;
using CoreUnitId = CombatSimulation.Core.UnitId;
using PlayerInputSignal = Combat.Ai.PlayerInputSignal;

namespace Combat.Integration.Core
{
    // Main's exploration level on the shared combat core (exploration plan, step 8), on the mode-neutral CoreBattleHost.
    // The legacy reference is ExplorationSceneManager. The mode's events go to the messenger and the windows: a dock
    // opens the scan window, whose Ok completes the objective; a completion writes the planet's mask and shows the loot.
    // The scene's own menu (Gui/Exploration/CombatMenu) sends Surrender and KillAllEnemies through the messenger, so this
    // adapter implements none of the roster contracts (E2).
    public sealed class CoreExplorationAdapter : CoreBattleHost, IScenePresentation
    {
        [Inject]
        public CoreExplorationAdapter(
            ExplorationData exploration,
            IDatabase database,
            IMessenger messenger,
            IResourceLocator resourceLocator,
            IGuiManager guiManager,
            PlayerFleet playerFleet,
            PlayerSkills playerSkills,
            ExitSignal.Trigger exitTrigger,
            ILocalization localization,
            PlayerInputSignal playerInputSignal,
            ISoundPlayer soundPlayer,
            IGameSettings gameSettings,
            UnityEngine.SceneManagement.Scene scene)
            : base(database, resourceLocator, exitTrigger, localization, playerInputSignal, soundPlayer, scene)
        {
            _exploration = exploration;
            _messenger = messenger;
            _guiManager = guiManager;

            var mapper = new CoreShipMapper(database, resourceLocator);
            _playerSpecification = PlayerSpecification(playerFleet, playerSkills, database);

            // The seed is drawn here, outside the core. It does not move the layout, which comes from the planet seed.
            var seed = (ulong)(uint)UnityEngine.Random.Range(int.MinValue, int.MaxValue);
            _setup = CoreExplorationSetup.Create(exploration, database, mapper, _playerSpecification,
                CoreExplorationSetup.CompletedObjectives(exploration), seed);
            LogInertDevices(mapper);

            var torus = _setup.Layout.Torus;
            var trees = CorePilotProvider.CreateTrees(seed, torus, mapper, OnBroadcast);
            var pilots = new CorePilotProvider(database.CombatSettings, trees, mapper.Mappings, PlayerSide, _setup.EnemyAiLevel);
            var runner = new BattleRunner(_setup.Setup, pilots);

            // E10: no shadows until phase 4; static wrecks on, as legacy exploration (ShipFactory.Settings).
            var viewSettings = new CombatViewSettings(damageText: gameSettings.ShowDamage, shadows: false, staticWrecks: true,
                trails: gameSettings.QualityMode >= 0);
            // No asteroids and no combat planet: the scene draws the planet's surface (PlanetBackground).
            Initialize(mapper, pilots, runner, trees, null, seed, torus, null, false, viewSettings);

            _objects = CoreExplorationObjects.Create(_setup, exploration, resourceLocator, scene);

            // E3: a beacon for each objective that is not complete; it closes on ObjectiveCompleted.
            foreach (var objective in _setup.Layout.Objectives)
                if (!exploration.IsCompleted(exploration.Objectives[objective.Id]))
                    _beacons.Add(objective.Id, new Beacon(this, objective.Position));

            ShipAdded += OnShipAdded;
            messenger.AddListener(EventType.Surrender, Surrender);
            messenger.AddListener(EventType.KillAllEnemies, KillAllEnemies);
        }

        // The player's ship and the nearest active enemy ship (legacy Scene.EnemyShip); EntityId.None when there is none.
        public EntityId ControlledShip => _player;
        public EntityId TargetShip => _target;

        // E3: the radar's beacons, by objective ID.
        public IEnumerable<IBeaconTarget> Beacons => _beacons.Values;

        #region IScenePresentation

        public Rect ViewRect => CameraRect;
        public float MapWidth => _setup.Layout.WorldSize;
        public float MapHeight => _setup.Layout.WorldSize;

        #endregion

        #region CoreBattleHost

        protected override EntityId PlayerShip => _player;
        protected override EntityId CameraTarget => _target;

        // Legacy ExplorationSceneInstaller: PlayerAlwaysInCenter.
        protected override bool CameraCentersPlayer => true;

        protected override bool TryDescribeShip(CoreUnitId unit, out CombatSide side, out IShipSpecification specification)
        {
            if (unit == CoreExplorationSetup.PlayerUnit)
            {
                side = CombatSide.Player;
                specification = _playerSpecification;
                return true;
            }

            if (!_setup.Layout.TryFindSpawner(unit, out var spawner))
            {
                side = default;
                specification = null;
                return false;
            }

            side = spawner.Role == ExplorationShipRole.DockableShip ? CombatSide.Player : CombatSide.Enemy;
            specification = _setup.Content.Specification(spawner.Role, spawner.ContentKey, spawner.Ordinal);
            return true;
        }

        protected override void OnUnitTransition(BattleSnapshot snapshot, in BattleEvent item)
        {
            if (item.UnitId != CoreExplorationSetup.PlayerUnit || item.TransitionReason != UnitTransitionReason.Deployed)
                return;

            TryGetEntity(item.EntityId, out _player);
            ZoomCamera();
        }

        protected override void OnShipRemoved(EntityId ship)
        {
            _enemies.Remove(ship);
            if (ship == _player)
                _player = EntityId.None;
            if (ship == _target)
                _target = EntityId.None;
        }

        protected override void OnModeEvent(BattleSnapshot snapshot, in ModeEvent item)
        {
            switch (item.Code)
            {
                case ExplorationMode.Docked:
                    OnDocked((int)item.Value0);
                    break;
                case ExplorationMode.Undocked:
                    // ScanningPanel closes with Cancel.
                    _messenger.Broadcast(EventType.PlayerShipUndocked, (int)item.Value0);
                    break;
                case ExplorationMode.ObjectiveCompleted:
                    OnObjectiveCompleted((int)item.Value0);
                    break;
            }
        }

        // Legacy Scene.UpdateEnemies, once per tick: the nearest active enemy ship.
        protected override void EndSnapshot(BattleSnapshot snapshot)
        {
            _target = EntityId.None;
            if (!TryGetCoreView(_player, out var player))
                return;

            var torus = _setup.Layout.Torus;
            var minDistance = float.MaxValue;
            foreach (var id in _enemies)
            {
                if (!TryGetCoreView(id, out var enemy) || !enemy.IsActive)
                    continue;

                var distance = torus.Delta(player.State.Position, enemy.State.Position).LengthSquared;
                if (distance >= minDistance)
                    continue;

                minDistance = distance;
                _target = id;
            }
        }

        // E12: AllComplete exits 250 ticks later, game time. PlayerOut exits at once: the core already waited 250 ticks
        // after the death (Lives). Main never sends Exit; surrender exits without a result.
        protected override void OnCompleted(BattleResult result)
        {
            if (IsExitScheduled)
                return;

            Debug.Log($"CoreExplorationAdapter: result {result}, {_exploration.Objectives.Count(_exploration.IsCompleted)} " +
                      $"of {_exploration.Objectives.Count} objectives complete");
            ScheduleExit(result.ModeReason == ExplorationMode.AllComplete ? AllCompleteExitSeconds : 0f);
        }

        protected override void RenderMode(Vector2 anchor, float deltaTime) => _objects?.Render(anchor, deltaTime);

        protected override void DisposeMode()
        {
            ShipAdded -= OnShipAdded;
            _messenger.RemoveListener(EventType.Surrender, Surrender);
            _messenger.RemoveListener(EventType.KillAllEnemies, KillAllEnemies);
            _objects?.Dispose();
        }

        #endregion

        // Legacy ExplorationSceneManager.CreatePlayerShip and FleetModel: the exploration ship with the player's skills, and
        // every satellite mount point (satellite plan, phase 7, step 4).
        private static IShipSpecification PlayerSpecification(PlayerFleet playerFleet, PlayerSkills playerSkills,
            IDatabase database)
        {
            var ship = playerFleet.ExplorationShip;
            var builder = ship.CreateBuilder();
            builder.AllMountPoints = ship.Model.Satellites.Count > ShipBuilder.LegacySatelliteSlots;
            builder.ApplyPlayerSkills(playerSkills);
            return builder.Build(database.ShipSettings);
        }

        private void OnShipAdded(EntityId ship, CombatSide side)
        {
            if (side == CombatSide.Enemy)
                _enemies.Add(ship);
        }

        // Legacy OnPlayerDocked: the scan window; its Ok completes the objective. An undock closes it with Cancel.
        private void OnDocked(int objective)
        {
            _messenger.Broadcast(EventType.PlayerShipDocked, objective);
            if (!_beacons.TryGetValue(objective, out var beacon) || beacon.IsComplete)
                return;

            _guiManager.OpenWindow(Gui.Exploration.WindowNames.ScanningPanel, code =>
            {
                if (code == WindowExitCode.Ok)
                    CompleteObjective(objective);
            });
        }

        private void CompleteObjective(int objective)
        {
            if (IsRunning)
                Runner.SubmitModeCommand(ExplorationMode.CompleteObjective, Runner.Tick, objective);
        }

        // E13, legacy OnScanCompleted: the planet's mask, then the loot and its window. A hostile objective's death comes
        // here too.
        private void OnObjectiveCompleted(int objective)
        {
            if (_beacons.TryGetValue(objective, out var beacon))
                beacon.IsComplete = true;

            _exploration.Complete(objective);
            var loot = _exploration.GetLoot(_exploration.Objectives[objective]).ToList();
            loot.Consume();
            _guiManager.OpenWindow(Gui.Exploration.WindowNames.LootPanel, new WindowArgs(loot));
        }

        // Legacy Exit: at once, without a result. The menu pauses the game, and a delay of 0 exits in this frame anyway.
        private void Surrender() => ScheduleExit(0f);

        // Legacy KillThemAll: every active enemy ship vanishes, and a hostile objective's death completes it.
        private void KillAllEnemies()
        {
            if (IsRunning)
                Runner.SubmitModeCommand(ExplorationMode.KillAll, Runner.Tick);
        }

        private bool TryGetCoreView(EntityId id, out CoreShipView view)
        {
            view = null;
            if (!id.IsValid || !TryGetShip(id, out var ship))
                return false;
            view = ship as CoreShipView;
            return view != null;
        }

        // E3: a fixed layout position, drawn at its image nearest to the camera's point.
        private sealed class Beacon : IBeaconTarget
        {
            public Beacon(CoreExplorationAdapter owner, CombatSimulation.Physics.Vec2 position)
            {
                _owner = owner;
                _position = position;
            }

            public Vector2 Position => Placement.NearestImage(_owner._setup.Layout.Torus, _owner.ViewPoint, _position);
            public bool IsActive => !IsComplete;
            public bool IsComplete { get; set; }

            private readonly CoreExplorationAdapter _owner;
            private readonly CombatSimulation.Physics.Vec2 _position;
        }

        // Legacy DelayedExit: 5 s.
        private const float AllCompleteExitSeconds = 5f;

        private EntityId _player = EntityId.None;
        private EntityId _target = EntityId.None;
        private readonly HashSet<EntityId> _enemies = new HashSet<EntityId>();
        private readonly Dictionary<int, Beacon> _beacons = new Dictionary<int, Beacon>();

        private readonly CoreExplorationSetup _setup;
        private readonly CoreExplorationObjects _objects;
        private readonly IShipSpecification _playerSpecification;
        private readonly ExplorationData _exploration;
        private readonly IMessenger _messenger;
        private readonly IGuiManager _guiManager;
    }
}
