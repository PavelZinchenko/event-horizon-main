﻿using Combat.Collision;
using Combat.Component.Unit;
using Combat.Factory;
using Combat.Unit;
using GameDatabase.Enums;

namespace Combat.Component.Bullet.Action
{
    public class CreateExplosionAction : IAction
    {
        public CreateExplosionAction(IUnit unit, SpaceObjectFactory factory, DamageType damageType, float damage, float radius, ConditionType condition = ConditionType.OnDetonate)
        {
            _factory = factory;
            _unit = unit;
            _damageType = damageType;
            _damage = damage;
            _radius = radius;
            Condition = condition;
        }

        public ConditionType Condition { get; private set; }

        public void Dispose() {}

        public CollisionEffect Invoke()
        {
            var position = _unit.GetHitPoint();
            _factory.CreateExplosion(position, _radius, _damageType, _damage, _unit.GetOwnerShip(), _unit.View.Color, _unit.Body.Weight);
            return CollisionEffect.None;
        }

        private readonly DamageType _damageType;
        private readonly float _damage;
        private readonly float _radius;
        private readonly IUnit _unit;
        private readonly SpaceObjectFactory _factory;
    }
}
