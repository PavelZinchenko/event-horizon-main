﻿using Combat.Collision;
using Combat.Collision.Behaviour;
using Combat.Collision.Manager;
using Combat.Component.Body;
using Combat.Component.Collider;
using Combat.Component.Triggers;
using Combat.Component.Unit.Classification;
using Combat.Component.View;
using Combat.Unit;
using UnityEngine;

namespace Combat.Component.Unit
{
    public class ShortLivedObject : UnitBase
    {
        public ShortLivedObject(IBody body, IView view, ICollider collider, float lifetime, UnitType unitType)
            : base(unitType, body, view, collider, null)
        {
            _lifetime = lifetime;
        }

        public IUnit Parent { get; set; }

        public override void OnCollision(Impact impact, IUnit target, CollisionData collisionData)
        {
            if (impact.Effects.Contains(CollisionEffect.Destroy))
            {
                InvokeTriggers(ConditionType.OnDestroy);
                Destroy();
            }
        }

        public override ICollisionBehaviour CollisionBehaviour { get { return _collisionBehaviour; } }

        public override UnitState State
        {
            get 
            {
                if (_isDead)
                    return UnitState.Destroyed;

                if (_lifetime <= 0)
                    _isDead = !Parent.IsActive();
                else if (_elapsedTime >= _lifetime)
                    _isDead = true;

                return _isDead ? UnitState.Destroyed : UnitState.Active;
            }
        }

        public void SetCollisionBehaviour(ICollisionBehaviour collisionBehaviour)
        {
            _collisionBehaviour = collisionBehaviour;
        }

        public override void Vanish()
        {
            Destroy();
            Parent = null;
        }

        protected override void OnUpdatePhysics(float elapsedTime)
        {
            _elapsedTime += elapsedTime;
        }

        protected override void OnDispose()
        {
            if (_collisionBehaviour != null)
                _collisionBehaviour.Dispose();
        }

        protected override void OnUpdateView(float elapsedTime)
        {
            View.Life = Mathf.Clamp01(1.0f - _elapsedTime/_lifetime);
        }

        private void Destroy()
        {
            _isDead = true;
        }

        private bool _isDead;
        private ICollisionBehaviour _collisionBehaviour;
        private float _elapsedTime;
        private readonly float _lifetime;
    }
}
