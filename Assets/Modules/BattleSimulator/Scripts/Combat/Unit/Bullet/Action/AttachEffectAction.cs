using Combat.Collision;
using Combat.Component.Body;
using Combat.Effects;
using Combat.Factory;
using GameDatabase.DataModel;
using UnityEngine;

namespace Combat.Component.Bullet.Action
{
    public class AttachEffectAction : IAction
    {
        public AttachEffectAction(IBullet bullet, EffectFactory effectFactory, VisualEffect effectData, Color color, float size, ConditionType condition)
        {
            _effectFactory = effectFactory;
            _bullet = bullet;
            _visualEffect = effectData;
            _color = color;
            _size = size;
            Condition = condition;
        }

        public ConditionType Condition { get; private set; }

        public void Dispose()
        {
            if (_effect != null && _effect.IsAlive)
                _effect.Dispose();
        }

        public CollisionEffect Invoke()
        {
            var lifetime = _bullet.Lifetime.Left;
            if (lifetime <= SpawnEffectAction.ShortEffectMaxLifetime)
            {
                var position = _bullet.Body.WorldPosition();
                var size = _bullet.Body.WorldScale() * _size;
                if (!_effectFactory.IsObjectVisible(position, size))
                    return CollisionEffect.None;
            }

            if (_effect == null || !_effect.IsAlive)
            {
                _effect = CompositeEffect.Create(_visualEffect, _effectFactory, _bullet.Body);
                _effect.Color = _color;
                _effect.Size = _size;
            }

            _effect.Run(lifetime, Vector2.zero, 0);
            return CollisionEffect.None;
        }

        private readonly IBullet _bullet;

        private CompositeEffect _effect;
        private readonly Color _color;
        private readonly float _size;
        private readonly EffectFactory _effectFactory;
        private readonly VisualEffect _visualEffect;
    }
}
