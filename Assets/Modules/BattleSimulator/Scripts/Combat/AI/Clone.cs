using Combat.Component.Ship;
using Combat.Scene;
using Combat.Unit;

namespace Combat.Ai
{
    public class Clone : IController
    {
        public Clone(IScene scene, IShip ship)
        {
            _ship = ship;
            _scene = scene;
        }

        public ControllerStatus Status => _ship.IsActive() ? ControllerStatus.Active : ControllerStatus.Dead;

        public void Update(float deltaTime, in AiManager.Options options)
        {
            _currentTime += deltaTime;
            var enemy = GetEnemy();
            var context = new Context(_ship, enemy, null, null, _currentTime);
            _strategy.Apply(context, _controls);
            _controls.Apply(_ship);
        }

        private void UpdateStrategy(IShip enemy)
        {
            _strategy = new AggressiveClone(_ship, enemy);
        }

        private void CreateNoEnemyStrategy()
        {
            _strategy = new PassiveDrone(_ship);
        }

        private IShip GetEnemy()
        {
            var elapsedTime = _currentTime - _lastUpdateTargetTime;

            if (!_enemy.IsActive())
            {
                if (elapsedTime < FindTargetDelay)
                    return _enemy;
            }
            else if (elapsedTime < ChangeTargetDelay)
            {
                return _enemy;
            }

            var newEnemy = _scene.Ships.GetEnemy(_ship, EnemyMatchingOptions.EnemyForDrone(0));
            if (!newEnemy.IsActive())
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
        private readonly IShip _ship;
        private readonly IScene _scene;
        private readonly ShipControls _controls = new();
        private const float ChangeTargetDelay = 5.0f;
        private const float FindTargetDelay = 0.5f;

        public class Factory : IControllerFactory
        {
            public Factory(IScene scene)
            {
                _scene = scene;
            }

            public IController Create(IShip ship)
            {
                return new Clone(_scene, ship);
            }

            private readonly IScene _scene;
        }
    }
}