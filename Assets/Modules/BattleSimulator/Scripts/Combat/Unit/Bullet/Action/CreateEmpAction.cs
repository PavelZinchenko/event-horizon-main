using Combat.Collision;
using Combat.Component.Ship;
using Combat.Component.Ship.Effects;
using Combat.Component.Unit;
using Combat.Component.Unit.Classification;
using Combat.Factory;
using Combat.Scene;
using Combat.Unit;
using GameDatabase.Enums;

namespace Combat.Component.Bullet.Action
{
    public class CreateEmpAction : IAction
    {
        public CreateEmpAction(IUnit unit, SpaceObjectFactory factory, DamageType damageType, float damage, float shieldDamage, float energyDrain, float radius, ConditionType condition = ConditionType.OnDetonate)
        {
            _factory = factory;
            _unit = unit;
            _damageType = damageType;
            _damage = damage;
            _shieldDamage = shieldDamage;
            _energyDrain = energyDrain;
            _radius = radius;
            Condition = condition;
        }

        public ConditionType Condition { get; private set; }

        public void Dispose() { }

        public CollisionEffect Invoke()
        {
            var position = _unit.GetHitPoint();
            _factory.CreateEmp(position, _radius, _damageType, _damage, _shieldDamage, _energyDrain, _unit.GetOwnerShip(), _unit.View.Color);
            return CollisionEffect.None;
        }

        private readonly DamageType _damageType;
        private readonly float _damage;
        private readonly float _radius;
        private readonly float _shieldDamage;
        private readonly float _energyDrain;
        private readonly IUnit _unit;
        private readonly SpaceObjectFactory _factory;
    }

    public sealed class CreateBattlewideEmpAction : IAction
    {
        public CreateBattlewideEmpAction(IScene scene, float duration,
            float initialEnergyDrainFraction, float energyDrainPerSecond,
            ConditionType condition = ConditionType.OnDetonate)
        {
            _scene = scene;
            _duration = duration;
            _initialEnergyDrainFraction = initialEnergyDrainFraction;
            _energyDrainPerSecond = energyDrainPerSecond;
            Condition = condition;
        }

        public ConditionType Condition { get; }

        public CollisionEffect Invoke()
        {
            if (_scene == null)
                return CollisionEffect.None;

            IShip playerShip = _scene.PlayerShip;
            lock (_scene.Ships.LockObject)
            {
                foreach (IShip ship in _scene.Ships.Items)
                {
                    if (!ship.IsActive() || ship == playerShip)
                        continue;

                    if (_initialEnergyDrainFraction > 0f)
                        ship.Stats.Energy.Get(ship.Stats.Energy.MaxValue * _initialEnergyDrainFraction);
                    RadarStatus.ApplyJammed(ship, _duration, _energyDrainPerSecond);
                }
            }
            return CollisionEffect.None;
        }

        public void Dispose() { }

        private readonly IScene _scene;
        private readonly float _duration;
        private readonly float _initialEnergyDrainFraction;
        private readonly float _energyDrainPerSecond;
    }

    public sealed class CreateEnemyFleetEmpAction : IAction
    {
        public CreateEnemyFleetEmpAction(IScene scene, IShip source, float duration,
            float initialEnergyDrainFraction, float energyDrainPerSecond,
            ConditionType condition = ConditionType.OnDetonate)
        {
            _scene = scene;
            _source = source;
            _duration = duration;
            _initialEnergyDrainFraction = initialEnergyDrainFraction;
            _energyDrainPerSecond = energyDrainPerSecond;
            Condition = condition;
        }

        public ConditionType Condition { get; }

        public CollisionEffect Invoke()
        {
            if (_scene == null || _source == null)
                return CollisionEffect.None;

            lock (_scene.Ships.LockObject)
            {
                foreach (IShip ship in _scene.Ships.Items)
                {
                    if (!ship.IsActive() || !CombatRelations.AreEnemies(_source.Type, ship.Type))
                        continue;

                    if (_initialEnergyDrainFraction > 0f)
                        ship.Stats.Energy.Get(ship.Stats.Energy.MaxValue * _initialEnergyDrainFraction);
                    RadarStatus.ApplyJammed(ship, _duration, _energyDrainPerSecond);
                }
            }

            RadarStatus.RevealStealthFor(_source.Type.Side, _duration);
            return CollisionEffect.None;
        }

        public void Dispose() { }

        private readonly IScene _scene;
        private readonly IShip _source;
        private readonly float _duration;
        private readonly float _initialEnergyDrainFraction;
        private readonly float _energyDrainPerSecond;
    }
}
