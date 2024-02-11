using UnityEngine;
using System.Linq;
using Combat.Ai;
using Combat.Component.Ship;
using Combat.Component.Triggers;
using Combat.Component.Unit.Classification;
using Combat.Domain;
using Combat.Factory;
using Combat.Scene;
using Combat.Unit;
using Combat.Unit.Ship.Effects.Special;
using GameStateMachine.States;
using Services.Audio;
using Services.Messenger;
using GameDatabase;
using Gui.Combat;
using Maths;
using Zenject;
using GameDatabase.Enums;

namespace Combat.Manager
{
    public class CombatManager : IInitializable, ITickable
    {
        [Inject]
        private CombatManager(
            IMessenger messenger,
            ISoundPlayer soundPlayer,
            ExitSignal.Trigger exitTrigger)
        {
            _soundPlayer = soundPlayer;
            _exitTrigger = exitTrigger;
            _messenger = messenger;

            _messenger.AddListener(EventType.EscapeKeyPressed, OnEscapeKeyPressed);
            _messenger.AddListener<IShip>(EventType.CombatShipCreated, OnShipCreated);
            _messenger.AddListener<IShip>(EventType.CombatShipDestroyed, OnShipDestroyed);
        }

        [Inject] private readonly IScene _scene;
        [Inject] private readonly IDatabase _database;
        [Inject] private readonly ShipControlsPanel _shipControlsPanel;
        [Inject] private readonly ShipFactory _shipFactory;
        [Inject] private readonly SpaceObjectFactory _spaceObjectFactory;
        [Inject] private readonly EffectFactory _effectFactory;

        [Inject] private readonly ShipSelectionPanel _shipSelectionPanel;
        [Inject] private readonly ShipStatsPanel _playerStatsPanel;
        [Inject] private readonly ShipStatsPanel _enemyStatsPanel;
        [Inject] private readonly CombatMenu _combatMenu;
        [Inject] private readonly Settings _settings;
        [Inject] private readonly RadarPanel _radarPanel;
        [Inject] private readonly IKeyboard _keyboard;
        [Inject] private readonly IMouse _mouse;
        [Inject] private readonly ICombatModel _combatModel;
		[Inject] private readonly Ai.BehaviorTree.BehaviorTreeBuilder _behaviorTreeBuilder;

        public void Initialize()
        {
            UnityEngine.Debug.Log("OnCombatStarted");

            var random = new System.Random();

            var level = _database.GalaxySettings.EnemyLevel(_combatModel.Rules.StarLevel);
            var powerMultiplier = Experience.LevelToPowerMultiplier(level);

            if (!_combatModel.Rules.DisableAsteroids)
            {
                for (int i = 0; i < 10; ++i)
                {
                    var size = Random.Range(2f, 5f);
                    var position = _scene.FindFreePlace(20f, UnitSide.Undefined);

                    var weight = size * size * 5f;
                    var hitPoints = size * size * 100 * powerMultiplier;
                    var damageMultiplier = powerMultiplier;

                    var velocity = Random.insideUnitCircle * 10 / size;
                    _spaceObjectFactory.CreateAsteroid(position, velocity, size, weight, hitPoints, damageMultiplier);
                }
            }

            if (!_combatModel.Rules.DisablePlanet)
            {
                var r = random.NextFloat();
                var g = Mathf.Sqrt(1f - r * r);
                var b = random.NextFloat();
                var color = Color.Lerp(new Color(r, g, b), Color.gray, 0.5f);

                var position = new Vector2(_scene.Settings.AreaWidth * random.NextFloat(), _scene.Settings.AreaHeight * random.NextFloat());
                var size = 30 + random.NextFloat() * 10;
                _spaceObjectFactory.CreatePlanet(position, size, color);
            }

            var initialEnemies = _combatModel.Rules.InitialEnemyShips;
            if (initialEnemies > 1)
                foreach (var ship in _combatModel.EnemyFleet.Ships.Where(item => item.Status == ShipStatus.Ready).Skip(1).Take(initialEnemies - 1))
                    CreateShip(ship);
        }

        public void OnShipCreated(IShip ship)
        {
            if (ship.Type.Class != UnitClass.Ship)
                return;

            CheckIfCanCallNextEnemy();

            switch (ship.Type.Side)
            {
                case UnitSide.Player:
                    _shipControlsPanel.Load(ship);
                    _messenger.Broadcast(EventType.PlayerShipCountChanged,
                        _combatModel.PlayerFleet.Ships.Count(item => item.Status == ShipStatus.Ready));
                    break;
                case UnitSide.Enemy:
                    _radarPanel.Add(ship);
                    _messenger.Broadcast(EventType.EnemyShipCountChanged,
                        _combatModel.EnemyFleet.Ships.Count(item => item.Status == ShipStatus.Ready));
                    break;
            }
        }

        public void OnShipDestroyed(IShip ship)
        {
            if (ship.Type.Class != UnitClass.Ship)
                return;
            
            CheckIfCanCallNextEnemy();
        }

        public void CreateShip(IShipInfo ship)
        {
            var position = _scene.FindFreePlace(40, ship.Side);

			IControllerFactory controllerFactory;
			if (ship.Side == UnitSide.Player)
				controllerFactory = new KeyboardController.Factory(_keyboard, _mouse);
			else if (_database.CombatSettings.EnemyAI != null)
				controllerFactory = new BehaviorTreeController.Factory(_database.CombatSettings.EnemyAI, 
					_scene, Ai.BehaviorTree.AiSettings.FromAiLevel(_combatModel.EnemyFleet.AiLevel), _behaviorTreeBuilder);
			else
				controllerFactory = new Computer.Factory(_scene, _combatModel.EnemyFleet.AiLevel);

            ship.Create(_shipFactory, controllerFactory, position);
        }

        public bool IsGamePaused { get { return _pausedCount > 0; } }

        public void OnEscapeKeyPressed()
        {
            if (_combatMenu)
                _combatMenu.Open();
        }

        public void Surrender()
        {
            _combatModel.PlayerFleet.DestroyAllShips();
            Exit();
        }

        public void Exit()
        {
            _exitTrigger.Fire();
			_scene.Clear();
        }

        public bool CanChangeShip()
        {
            if (_combatModel.Rules.ShipSelection != PlayerShipSelectionMode.Default) return false;
            return _combatModel.PlayerFleet.IsAnyShipLeft();
        }

        public bool CanKillAllEnemies => _combatModel.Rules.KillThemAllButton;

        public void ChangeShip()
        {
            var player = _scene.PlayerShip;
            if (player.Effects.All.OfType<ShipRetreatEffect>().Any())
                return;

            var chargeEffect = new ShipRetreatingEffect(player, _effectFactory, ConditionType.OnActivate, ConditionType.OnDeactivate);
            var warpEffect = new ShipWarpEffect(player, _effectFactory, _soundPlayer, _settings.ShipWarpSound, ConditionType.OnDeactivate);
            var soundEffect = new SoundLoopEffect(_soundPlayer, _settings.ShipRetreatSound, ConditionType.OnActivate, ConditionType.OnDeactivate);
            player.AddEffect(new ShipRetreatEffect(7.0f, soundEffect, warpEffect, chargeEffect));
        }

        public void KillAllEnemies()
        {
            _combatModel.EnemyFleet.DestroyAllShips();
        }

        public bool CanCallNextEnemy() { return _canCallNextEnemy; }

        public void CallNextEnemy()
        {
            if (!CanCallNextEnemy())
                return;

            var shipInfo = _combatModel.EnemyFleet.Ships.FirstOrDefault(item => item.Status == ShipStatus.Ready);
            if (shipInfo == null)
                return;

            CreateShip(shipInfo);
            _soundPlayer.Play(_settings.ReinforcementSound);
        }

        private void CheckIfCanCallNextEnemy()
        {
            var rules = _combatModel.Rules;
            if (rules.TimeOutMode != TimeOutMode.CallNextEnemy && rules.TimeOutMode != TimeOutMode.CallNextEnemyOrDraw)
            {
                _canCallNextEnemy = false;
                return;
            }

            if (rules.TimeLimit <= 0)
            {
                _canCallNextEnemy = false;
                return;
            }

            var enemyCount = _combatModel.EnemyFleet.Ships.Count(item => item.Status == ShipStatus.Active);
            _canCallNextEnemy = enemyCount < rules.MaxEnemyShips && _combatModel.EnemyFleet.IsAnyShipLeft();
        }

        public void Tick()
        {
            if (_combatModel == null)
                return;

            UpdateLocalGame();
        }

        private void UpdateLocalGame()
        {
            var player = _scene.PlayerShip;
            var enemy = _scene.EnemyShip;

            if (!enemy.IsActive())
            {
                _nextShipCooldown += Time.deltaTime;
                if (_nextShipCooldown > _nextShipMaxCooldown)
                {
                    _nextShipCooldown = 0;

                    var shipInfo = _combatModel.EnemyFleet.Ships.FirstOrDefault(item => item.Status == ShipStatus.Ready);
                    if (shipInfo == null)
                    {
                        UnityEngine.Debug.Log("No more ships");
                        Exit();
                        return;
                    }

                    CreateShip(shipInfo);
                }
            }
            else if (!player.IsActive())
            {
                _nextPlayerShipCooldown += Time.deltaTime;
                if (_nextPlayerShipCooldown > _nextShipMaxCooldown)
                {
                    _nextPlayerShipCooldown = 0;

                    if (IsPlayerDefeated())
                    {
                        UnityEngine.Debug.Log("No more ships");
                        Exit();
                    }
                    else if (_combatModel.Rules.ShipSelection == PlayerShipSelectionMode.Default || 
                        _combatModel.Rules.ShipSelection == PlayerShipSelectionMode.NoRetreats)
                    {
                        _shipSelectionPanel.Open(_combatModel);
                    }
                    else
                    {
                        var shipInfo = _combatModel.PlayerFleet.AnyAvailableShip();
                        CreateShip(shipInfo);
                    }
                }
            }
            else if (!IsGamePaused)
            {
                _playerStatsPanel.Open(player);
                _enemyStatsPanel.Open(enemy);
            }
        }

        private bool IsPlayerDefeated()
        {
            if (_combatModel.Rules.ShipSelection == PlayerShipSelectionMode.OnlyOneShip && 
                _scene.PlayerShip != null && _scene.PlayerShip.State == UnitState.Destroyed)
                return true;

            if (!_combatModel.PlayerFleet.IsAnyShipLeft())
                return true;

            return false;
        }

        private bool _canCallNextEnemy;

        private float _nextShipCooldown = _nextShipMaxCooldown;
        private float _nextPlayerShipCooldown = _nextShipMaxCooldown;
        private const float _nextShipMaxCooldown = 3.0f;

        private int _pausedCount;
        private readonly ISoundPlayer _soundPlayer;
        private readonly ExitSignal.Trigger _exitTrigger;
        private readonly IMessenger _messenger;
    }
}
