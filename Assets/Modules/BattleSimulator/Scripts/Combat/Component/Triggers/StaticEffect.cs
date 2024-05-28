using Combat.Component.Body;
using Combat.Effects;
using Combat.Factory;
using GameDatabase.DataModel;
using GameDatabase.Model;
using UnityEngine;

namespace Combat.Component.Triggers
{
    public class StaticEffect : IUnitEffect
    {
        public StaticEffect(PrefabId effectId, EffectFactory effectFactory, IBody body, float lifetime, float size, Color color, ConditionType conditionType)
        {
            _body = body;
            _effectId = effectId;
            _lifetime = lifetime;
            _color = color;
            _size = size;
            _effectFactory = effectFactory;
            TriggerCondition = conditionType;
        }

        public StaticEffect(VisualEffect effect, EffectFactory effectFactory, IBody body, float lifetime, float size, Color color, ConditionType conditionType)
        {
            _body = body;
            _effect = effect;
            _lifetime = lifetime;
            _color = color;
            _size = size;
            _effectFactory = effectFactory;
            TriggerCondition = conditionType;
        }

        public ConditionType TriggerCondition { get; private set; }
        public bool TryUpdateEffect(float elapsedTime) { return false; }

        public bool TryInvokeEffect(ConditionType condition)
        {
            var position = _body.WorldPosition();
            IEffect effect;
            if (_effect != null)
                effect = CompositeEffect.Create(_effect, _effectFactory, null);
            else if (_effectId)
                effect = _effectFactory.CreateEffect(_effectId);
            else
                return false;

            effect.Position = position;
            effect.Rotation = _body.WorldRotation();
            effect.Size = _size;
            effect.Color = _color;
            effect.Run(_lifetime, Vector2.zero, 0);
            return false;
        }

        public void Dispose() {}

        private readonly float _size;
        private readonly float _lifetime;
        private readonly Color _color;
        private readonly IBody _body;
        private readonly VisualEffect _effect;
        private readonly PrefabId _effectId;
        private readonly EffectFactory _effectFactory;
    }
}
