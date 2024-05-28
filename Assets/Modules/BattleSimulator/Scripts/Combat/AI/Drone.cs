using System.Linq;
using Combat.Component.Ship;
using Combat.Component.Systems.Weapons;
using Combat.Scene;
using Combat.Unit;
using Constructor;

namespace Combat.Ai
{
    public class Drone : IController
    {
        public Drone(IScene scene, IShip ship, float range, DroneBehaviour behaviour, bool improvedAi)
        {
            _ship = ship;
            _range = range;
            _attackRange = range + Helpers.ShipMaxRange(ship);
            _scene = scene;
            _behaviour = behaviour;
            _improvedAi = improvedAi;
            _haveWeapons = _ship.Systems.All.Any(item => item is IWeapon);

            if (improvedAi)
            {
                _targets = new TargetList(_scene);
                _threats = new ThreatList(_scene);
            }
        }

        public ControllerStatus Status => _ship.IsActive() ? ControllerStatus.Active : ControllerStatus.Dead;

        public void Update(float deltaTime, in AiManager.Options options)
        {
            _currentTime += deltaTime;

            var enemy = GetEnemy();

            if (_improvedAi)
            {
                _threats.Update(deltaTime, _ship, _strategy);
                _targets.Update(deltaTime, _ship, enemy);
            }

            var context = new Context(_ship, enemy, _targets, _threats, _currentTime);
            _strategy.Apply(context, _controls);
            _controls.Apply(_ship);
        }

        private void UpdateStrategy(IShip enemy)
        {
            if (_behaviour == DroneBehaviour.Aggressive)
                UpdateAggressiveStrategy(enemy);
            else
                UpdateDefensiveStrategy(enemy);
        }

        private void UpdateAggressiveStrategy(IShip enemy)
        {
            _strategy = new AggressiveDrone(_ship, enemy, _range, _improvedAi);
        }

        private void UpdateDefensiveStrategy(IShip enemy)
        {
            _strategy = new DefensiveDrone(_ship, enemy, _range, _improvedAi);
        }

        private void CreateNoEnemyStrategy()
        {
            _strategy = new PassiveDrone(_ship);
        }

        private bool EnemyOutOfRange(IShip enemy)
        {
            return _ship.Type.Owner != null && _ship.Type.Owner.Body.Position.Distance(enemy.Body.Position) > _attackRange;
        }

        private IShip GetEnemy()
        {
            var elapsedTime = _currentTime - _lastUpdateTargetTime;

            if (!_enemy.IsActive() || EnemyOutOfRange(_enemy))
            {
                if (elapsedTime < FindTargetDelay)
                    return _enemy;
            }
            else if (elapsedTime < ChangeTargetDelay)
            {
                return _enemy;
            }

            var newEnemy = _haveWeapons ? _scene.Ships.GetEnemy(_ship, EnemyMatchingOptions.EnemyForDrone(_attackRange)) : null;
            if (!newEnemy.IsActive() || EnemyOutOfRange(newEnemy))
            {
                if (_strategy == null || _enemy != newEnemy)
                    CreateNoEnemyStrategy();
            }
            else
            {
                if (newEnemy != _enemy)
                    UpdateStrategy(newEnemy);
            }

            _enemy = newEnemy;
            _lastUpdateTargetTime = _currentTime;
            return _enemy;
        }

        private IShip _enemy;

        private float _lastUpdateTargetTime = -10f;
        private float _currentTime;

        private IStrategy _strategy;
        private readonly bool _haveWeapons;
        private readonly bool _improvedAi;
        private readonly float _range;
        private readonly float _attackRange;
        private readonly IShip _ship;
        private readonly IScene _scene;
        private readonly ShipControls _controls = new();
        private readonly DroneBehaviour _behaviour;
        private readonly ThreatList _threats;
        private readonly TargetList _targets;
        private const float ChangeTargetDelay = 5.0f;
        private const float FindTargetDelay = 0.5f;

        public class Factory : IControllerFactory
        {
            public Factory(IScene scene, float range, DroneBehaviour behaviour, bool improvedAi)
            {
                _scene = scene;
                _range = range;
                _behaviour = behaviour;
                _improvedAi = improvedAi;
            }

            public IController Create(IShip ship)
            {
                return new Drone(_scene, ship, _range, _behaviour, _improvedAi);
            }

            private readonly bool _improvedAi;
            private readonly float _range;
            private readonly DroneBehaviour _behaviour;
            private readonly IScene _scene;
        }
    }
}
