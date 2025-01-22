﻿using System;
using System.Collections.Generic;
using Combat.Collision;
using Combat.Collision.Behaviour;
using Combat.Collision.Manager;
using Combat.Component.Body;
using Combat.Component.Bullet.Action;
using Combat.Component.Bullet.Lifetime;
using Combat.Component.Collider;
using Combat.Component.Controller;
using Combat.Component.DamageHandler;
using Combat.Component.Physics;
using Combat.Component.Unit;
using Combat.Component.Unit.Classification;
using Combat.Component.View;
using Combat.Unit;
using UnityEngine;

namespace Combat.Component.Bullet
{
    public class Bullet : IBullet
    {
        public Bullet(IBody body, IView view, ILifetime lifetime, UnitType unitType, in Options options)
        {
            _body = body;
            _view = view;
            _lifetime = lifetime;
            _unitType = unitType;
            State = UnitState.Active;
            _options = options;
        }

        public UnitType Type { get { return _unitType; } }
        public IBody Body { get { return _body; } }
        public IView View { get { return _view; } }
        public IController Controller { get; set; }
        public ILifetime Lifetime { get { return _lifetime; } }
        public IDamageHandler DamageHandler { get; set; }
        public ICollider Collider { get; set; }
        public PhysicsManager Physics { get; set; }
        public ICollisionBehaviour CollisionBehaviour { get; set; }

        public float DefenseMultiplier => _unitType?.Owner?.DefenseMultiplier ?? 1.0f;

        public void OnCollision(Impact impact, IUnit target, CollisionData collisionData)
        {
            if (State != UnitState.Active)
                return;

            if (DamageHandler != null)
                impact.Effects |= DamageHandler.ApplyDamage(impact, target);

            impact.Effects |= InvokeActions(ConditionType.OnCollide);
            if (impact.Effects.Contains(CollisionEffect.Disarm) && _options.CanBeDisarmed)
            {
                Disarm();
            }
            else if (impact.Effects.Contains(CollisionEffect.Destroy))
            {
                if (_options.DetonateWhenDestroyed)
                    Detonate();
                else
                    Destroy();
            }
        }

        public UnitState State { get; private set; }

        public void UpdatePhysics(float elapsedTime)
        {
            if (State != UnitState.Active)
                return;

            if (Controller != null)
                Controller.UpdatePhysics(elapsedTime);

            Body.UpdatePhysics(elapsedTime);

            _unitSizedBody?.SetSize(1 / _body.WorldScale());

            Lifetime.Take(elapsedTime);
            Collider.UpdatePhysics(elapsedTime);
            
            foreach (var action in _cooldownActions)
            {
                action.Invoke();
            }

            if (Lifetime.IsExpired)
                Expire();
        }

        public void UpdateView(float elapsedTime)
        {
            Body.UpdateView(elapsedTime);
            View.Life = Lifetime.Value;
            View.UpdateView(elapsedTime);
        }

        public void Vanish()
        {
            State = UnitState.Inactive;
        }

        public IBody GetUnitSizedBody()
        {
            if (_unitSizedBody != null) return _unitSizedBody;
            const string sizedObjectName = "UnitSizedBody";
            
            var obj = _body.FindChild(sizedObjectName)?.gameObject;
            GameObjectBody body;
            if (Equals(obj, null))
            {
                obj = new GameObject(sizedObjectName); 
                body = obj.AddComponent<GameObjectBody>();
            }
            else
            {
                body = obj.GetComponent<GameObjectBody>();
            }
            body.Initialize(_body, Vector2.zero, 0, 1, Vector2.zero, 0, 0);
            body.Scale = 1 / _body.WorldScale();
            _unitSizedBody = body;
            return _unitSizedBody;
        }
        
        public void Dispose()
        {
            Body.Dispose();
            View.Dispose();

            if (CollisionBehaviour != null)
                CollisionBehaviour.Dispose();
            if (Controller != null)
                Controller.Dispose();
            if (DamageHandler != null)
                DamageHandler.Dispose();
            if (Physics != null)
                Physics.Dispose();

            foreach (var item in _resources)
                item.Dispose();

            if (Collider != null)
                Collider.Dispose();
            
            if(_unitSizedBody != null)
                _unitSizedBody.Dispose();
        }

        public void AddAction(IAction action)
        {
            if(action.Condition == ConditionType.OnCooldown)
                _cooldownActions.Add(action);
            else if (action.Condition != ConditionType.None)
                _actions.Add(action);

            AddResource(action);

            if (action.Condition == ConditionType.None)
                action.Invoke();
        }

        public void AddResource(IDisposable resource)
        {
            _resources.Add(resource);
        }

        public void Detonate()
        {
            if (State != UnitState.Active)
                return;

            Destroy();
            // Invoke triggers *after* marking as destroyed, to avoid spawning
            // attached ammo that gets cleaned up immediately
            InvokeActions(ConditionType.OnDetonate);
        }

        private void Expire()
        {
            // Temporarily mark ammunition as destroyed, to avoid the trigger
            // spawning attached ammo that gets cleaned up immediately 
            var oldState = State;
            State = UnitState.Destroyed;
            
            var effect = InvokeActions(ConditionType.OnExpire);
            
            State = oldState;

            if (effect == CollisionEffect.Destroy)
                Detonate();
            else
                Destroy();
        }

        private void Disarm()
        {
            MarkAsDestroyed();
            // Invoke triggers *after* marking as destroyed, to avoid spawning
            // attached ammo that gets cleaned up immediately
            InvokeActions(ConditionType.OnDisarm);
        }

        private void Destroy()
        {
            MarkAsDestroyed();
            // Invoke triggers *after* marking as destroyed, to avoid spawning
            // attached ammo that gets cleaned up immediately
            InvokeActions(ConditionType.OnDestroy);
        }

        private void MarkAsDestroyed()
        {
            if (State == UnitState.Active)
                State = UnitState.Destroyed;
        }

        private CollisionEffect InvokeActions(ConditionType condition)
        {
            var effect = CollisionEffect.None;
            var count = _actions.Count;
            for (var i = 0; i < count; ++i)
            {
                var action = _actions[i];
                if (action.Condition.Contains(condition))
                    effect |= action.Invoke();
            }

            return effect;
        }

        private IBody _unitSizedBody;
        private readonly Options _options;
        private readonly ILifetime _lifetime;
        private readonly UnitType _unitType;
        private readonly IBody _body;
        private readonly IView _view;
        private readonly List<IAction> _actions = new List<IAction>();
        private readonly List<IAction> _cooldownActions = new List<IAction>();
        private readonly List<IDisposable> _resources = new List<IDisposable>();

        public struct Options
        {
            public bool CanBeDisarmed;
            public bool DetonateWhenDestroyed;
        }
    }
}
