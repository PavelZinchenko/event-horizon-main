﻿using Combat.Component.Body;
using Combat.Component.Unit;
using Combat.Effects;
using Combat.Factory;
using GameDatabase.DataModel;
using GameDatabase.Model;
using Services.Audio;
using UnityEngine;

namespace Combat.Component.Triggers
{
    public class ShipExplosionAction : IUnitAction
    {
        public ShipExplosionAction(IUnit unit, EffectFactory effectFactory, ISoundPlayer soundPlayer, VisualEffect visualEffect, AudioClipId audioClip)
        {
            _unit = unit;
            _factory = effectFactory;
            _soundPlayer = soundPlayer;
            _audioClip = audioClip;
            _visualEffect = visualEffect;
        }

        public ConditionType TriggerCondition { get { return ConditionType.OnDestroy; } }

        public bool TryUpdateAction(float elapsedTime) { return false; }

        public bool TryInvokeAction(ConditionType condition)
        {
            var effect = CompositeEffect.Create(_visualEffect, _factory, null);
            effect.Position = _unit.Body.Position;
            effect.Rotation = _unit.Body.Rotation;
            effect.Size = _unit.Body.Scale;

            var lifetime = 0f;
            foreach (var element in _visualEffect.Elements)
                if (element.Lifetime > lifetime)
                    lifetime = element.Lifetime;

            effect.Run(lifetime, _unit.Body.Velocity * 0.05f, 0);

            _soundPlayer.Play(_audioClip);

            return false;
        }

        public void Dispose() { }

        private readonly EffectFactory _factory;
        private readonly ISoundPlayer _soundPlayer;
        private readonly IUnit _unit;
        private readonly VisualEffect _visualEffect;
        private readonly AudioClipId _audioClip;
    }

    public class ShipExplosionActionObsolete : IUnitAction
    {
        public ShipExplosionActionObsolete(IUnit unit, EffectFactory effectFactory, ISoundPlayer soundPlayer)
        {
            _factory = effectFactory;
            _soundPlayer = soundPlayer;
            _unit = unit;
        }

        public ConditionType TriggerCondition { get { return ConditionType.OnDestroy; } }

        public bool TryUpdateAction(float elapsedTime) { return false; }

        public bool TryInvokeAction(ConditionType condition)
        {
            var effect = _factory.CreateEffect("Wave");
            effect.Color = new Color(1f, 0.66f, 0.59f); //ColorTable.ShipExplosionColor;
            effect.Position = _unit.Body.Position;
            effect.Size = _unit.Body.Scale*6f;
            effect.Run(1.0f, _unit.Body.Velocity*0.05f, 0f);

            effect = _factory.CreateEffect("FlashAdditive");
            effect.Color = new Color(1f, 0.66f, 0.59f); //ColorTable.ShipExplosionColor;
            effect.Position = _unit.Body.Position;
            effect.Size = _unit.Body.Scale * 4f;
            effect.Run(1.0f, _unit.Body.Velocity * 0.05f, 0);

            effect = _factory.CreateEffect("SmokeAdditive");
            effect.Color = new Color(1f, 0.66f, 0.59f); //ColorTable.ShipExplosionColor;
            effect.Position = _unit.Body.Position;
            effect.Size = _unit.Body.Scale * 1.5f;
            effect.Run(1.0f, _unit.Body.Velocity * 0.05f, new System.Random().Next(-20, 20));

            effect = _factory.CreateEffect("Smoke");
            effect.Color = new Color(0.1f,0.1f,0.1f,0.5f);
            effect.Position = _unit.Body.Position;
            effect.Size = _unit.Body.Scale * 2.0f;
            effect.Run(3.0f, _unit.Body.Velocity * 0.05f, new System.Random().Next(-20, 20));

            _factory.CreateDisturbance(_unit.Body.WorldPosition(), _unit.Body.Scale * 10);

            _soundPlayer.Play(new AudioClipId("explosion_01"));

            return false;
        }

        public void Dispose() {}

        private readonly EffectFactory _factory;
        private readonly ISoundPlayer _soundPlayer;
        private readonly IUnit _unit;
    }
}
