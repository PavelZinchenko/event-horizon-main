﻿using Combat.Collision;
using Combat.Component.Body;
using Combat.Component.Unit;
using Combat.Effects;
using Combat.Factory;
using GameDatabase.DataModel;
using UnityEngine;

namespace Combat.Component.Bullet.Action
{
    public class SpawnEffectAction : IAction
    {
        public SpawnEffectAction(IUnit unit, EffectFactory effectFactory, VisualEffect effectData, Color color, float size, float lifetime, ConditionType condition)
        {
            _effectFactory = effectFactory;
            _unit = unit;
            _visualEffect = effectData;
            _color = color;
            _size = size;
            _lifetime = lifetime;
            Condition = condition;
        }

        public ConditionType Condition { get; private set; }

        public void Dispose() {}

        public CollisionEffect Invoke()
        {
            var position = _unit.Body.WorldPosition();

            if (_lifetime <= ShortEffectMaxLifetime && !_effectFactory.IsObjectVisible(position, _size)) 
                return CollisionEffect.None;

            var rotation = _unit.Body.WorldRotation();
            var effect = CompositeEffect.Create(_visualEffect, _effectFactory, null);
            effect.Position = position;
            effect.Rotation = rotation;
            effect.Color = _color;
            effect.Size = _size;
            effect.Run(_lifetime, Vector2.zero, 0);
            return CollisionEffect.None;
        }

        private readonly IUnit _unit;

        private readonly float _lifetime;
        private readonly Color _color;
        private readonly float _size;
        private readonly EffectFactory _effectFactory;
        private readonly VisualEffect _visualEffect;

        public const float ShortEffectMaxLifetime = 3.0f;
    }
}
