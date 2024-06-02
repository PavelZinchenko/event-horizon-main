using Combat.Collision.Manager;
using Combat.Component.Unit;
using Combat.Effects;
using GameDatabase.DataModel;
using System.Collections.Generic;
using UnityEngine;

namespace Combat.Collision.Behaviour.Action
{
    public class ShowMultipleHitEffectsAction : ICollisionAction
    {
        public ShowMultipleHitEffectsAction(Factory.EffectFactory effectFactory, VisualEffect effectData, Color color, float size, float lifetime = 0.2f)
        {
            _effectFactory = effectFactory;
            _visualEffect = effectData;
            _color = color;
            _size = size;
            _lifetime = lifetime;
        }

        public void Invoke(IUnit self, IUnit target, CollisionData collisionData, ref Impact selfImpact, ref Impact targetImpact)
        {
            if (!_effectFactory.IsObjectVisible(collisionData.Position, _size)) return;

            if (!_effects.TryGetValue(target, out var effect) || !effect.IsAlive)
            {
                effect = CompositeEffect.Create(_visualEffect, _effectFactory, null);
                effect.Color = _color;
                effect.Size = _size;
                _effects[target] = effect;
            }

            effect.Position = collisionData.Position;
            effect.Run(_lifetime, target.Body.Velocity, 0);
        }

        public void Dispose() { }

        private readonly Dictionary<IUnit, IEffect> _effects = new();
        private readonly float _lifetime;
        private readonly Color _color;
        private readonly float _size;
        private readonly Factory.EffectFactory _effectFactory;
        private readonly VisualEffect _visualEffect;
    }
}
