using Combat.Collision.Manager;
using Combat.Component.Unit;
using Combat.Effects;
using GameDatabase.DataModel;
using UnityEngine;

namespace Combat.Collision.Behaviour.Action
{
    public class SpawnHitEffectAction : ICollisionAction
    {
        public SpawnHitEffectAction(Factory.EffectFactory effectFactory, VisualEffect effectData, Color color, float size, float lifetime = 0.2f, float cooldown = 0, bool newOnly = false)
        {
            _cooldown = cooldown;
            _effectFactory = effectFactory;
            _visualEffect = effectData;
            _color = color;
            _size = size;
            _lifetime = lifetime;
            _newOnly = newOnly;
        }

        public void Invoke(IUnit self, IUnit target, CollisionData collisionData, ref Impact selfImpact, ref Impact targetImpact)
        {
            var time = Time.fixedTime;
            if (time - _lastSpawnTime < _cooldown) return;
            if (_newOnly && !collisionData.IsNew) return;

            if (_effectFactory.IsObjectVisible(collisionData.Position, _size))
            {
                var effect = CompositeEffect.Create(_visualEffect, _effectFactory, null);
                effect.Position = collisionData.Position;
                effect.Rotation = self.Body.Rotation;
                effect.Color = _color;
                effect.Size = _size;
                effect.Run(_lifetime, target.Body.Velocity, 0);
            }

            _lastSpawnTime = time;
        }

        public void Dispose() {}

        private float _lastSpawnTime;
        private readonly bool _newOnly;
        private readonly float _cooldown;
        private readonly float _lifetime;
        private readonly Color _color;
        private readonly float _size;
        private readonly Factory.EffectFactory _effectFactory;
        private readonly VisualEffect _visualEffect;
    }
}
