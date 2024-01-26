using Combat.Collision;
using Combat.Component.Unit;
using Combat.Effects;
using Combat.Factory;
using GameDatabase.DataModel;
using UnityEngine;

namespace Combat.Component.Bullet.Action
{
    public class PlayEffectAction : IAction
    {
        public PlayEffectAction(IUnit unit, EffectFactory effectFactory, float cooldown, VisualEffect effectData, Color color, float size, float lifetime, ConditionType condition)
        {
            _effectFactory = effectFactory;
            _unit = unit;
            _visualEffect = effectData;
            _color = color;
            _size = size;
            _lifetime = lifetime;
            Condition = condition;
            Cooldown = cooldown;
        }

        public ConditionType Condition { get; private set; }
        public float Cooldown { get; set; }

        public void Dispose()
        {
            if (_effect != null && _effect.IsAlive)
                _effect.Detach();
        }

        public CollisionEffect Invoke()
        {
            var time = Time.fixedTime;

            if (time < _nextActivation)
                return CollisionEffect.None;
            
            if (_effect == null || !_effect.IsAlive)
            {
                _effect = CompositeEffect.Create(_visualEffect, _effectFactory, _unit.Body);
                _effect.Color = _color;
                _effect.Size = _size;
            }

            _effect.Run(_lifetime, Vector2.zero, 0);

            _nextActivation = time + Cooldown;
            return CollisionEffect.None;
        }

        private readonly IUnit _unit;

        private float _nextActivation;
        private CompositeEffect _effect;
        private readonly float _lifetime;
        private readonly Color _color;
        private readonly float _size;
        private readonly EffectFactory _effectFactory;
        private readonly VisualEffect _visualEffect;
    }
}
