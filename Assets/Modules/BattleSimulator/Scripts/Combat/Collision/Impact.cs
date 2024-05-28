using System;
using Combat.Component.Body;
using GameDatabase.Enums;
using UnityEngine;

namespace Combat.Collision
{
    public class Impulse
    {
        public Impulse()
        {
            _values = new Vector2[8];
            _count = 0;
        }

        public void Apply(IBody body)
        {
            for (var i = 0; i < _count; ++i)
                body.ApplyForce(_values[i*2], _values[i*2 + 1]);
        }

        public void Apply(IBody body, float multiplier)
        {
            for (var i = 0; i < _count; ++i)
                body.ApplyForce(_values[i * 2], _values[i * 2 + 1] * multiplier);
        }

        public void Append(Vector2 position, Vector2 impulse)
        {
            if (_count + 2 >= _values.Length)
                Array.Resize(ref _values, _count + 2);

            _values[_count++] = position;
            _values[_count++] = impulse;
        }

        public Impulse Append(Impulse other)
        {
            if (other == null || other._count == 0)
                return this;

            if (_count + other._count >= _values.Length)
                Array.Resize(ref _values, _count + other._count);

            Array.Copy(other._values, 0, _values, _count, other._count);
            _count += other._count;

            return this;
        }

        public void Clear()
        {
            _count = 0;
        }

        private int _count;
        private Vector2[] _values;
    }

    public struct Impact
    {
        public float KineticDamage;
        public float EnergyDamage;
        public float HeatDamage;
        public float DirectDamage;
        public float Repair;
        public float ShieldDamage;
        public float EnergyDrain;
        public bool IgnoresShield;
        public Impulse Impulse;
        public CollisionEffect Effects;

        public float GetTotalDamage(in Resistance resistance)
        {
			var damage =
				resistance.ModifyKineticDamage(KineticDamage) +
				resistance.ModifyEnergyDamage(EnergyDamage) +
				resistance.ModifyHeatDamage(HeatDamage) +
				resistance.ModifyDirectDamage(DirectDamage);

            return damage;
        }

        public void AddDamage(DamageType type, float amount)
        {
            if (amount < 0)
                throw new InvalidOperationException();

            if (type == DamageType.Direct)
                DirectDamage += amount;
            else if (type == DamageType.Impact)
                KineticDamage += amount;
            else if (type == DamageType.Energy)
                EnergyDamage += amount;
            else if (type == DamageType.Heat)
                HeatDamage += amount;
            else
                throw new System.ArgumentException("unknown damage type");
        }

        public void AddImpulse(in Vector2 position, in Vector2 impulse)
        {
            if (Impulse == null)
                Impulse = new Impulse();

            Impulse.Append(position, impulse);
        }

        public void ApplyImpulse(IBody body)
        {
            if (Impulse != null)
                Impulse.Apply(body);
        }

        public void RemoveImpulse()
        {
            if (Impulse != null)
                Impulse.Clear();
        }

        public Impact GetDamage(in Resistance resistance)
        {
            return new Impact
            {
                KineticDamage = KineticDamage * (1f - resistance.Kinetic),
                EnergyDamage = EnergyDamage * (1f - resistance.Energy),
                HeatDamage = HeatDamage * (1f - resistance.Heat),
                DirectDamage = DirectDamage * (1f - 0.5f * resistance.MinResistance),
                ShieldDamage = ShieldDamage,
                EnergyDrain = EnergyDrain,
                Impulse = Impulse,
                Repair = Repair,
                Effects = Effects
            };
        }

        public void ApplyShield(float shieldPoints)
        {
            if (IgnoresShield || shieldPoints <= 0 || ShieldDamage >= shieldPoints) return;

            if (ShieldDamage > 0)
                shieldPoints -= ShieldDamage;

            var damage = KineticDamage + EnergyDamage + HeatDamage + DirectDamage;
            if (damage <= 0) return;
            if (damage <= shieldPoints)
            {
                RemoveDamage();
                ShieldDamage += damage;
                return;
            }

            KineticDamage -= shieldPoints * KineticDamage / damage;
            EnergyDamage -= shieldPoints * EnergyDamage / damage;
            HeatDamage -= shieldPoints * HeatDamage / damage;
            DirectDamage -= shieldPoints * DirectDamage / damage;
            ShieldDamage += shieldPoints;
        }

        public void RemoveDamage()
        {
            KineticDamage = 0;
            EnergyDamage = 0;
            HeatDamage = 0;
            DirectDamage = 0;
        }

        public void Append(in Impact second)
        {
            KineticDamage += second.KineticDamage;
            EnergyDamage += second.EnergyDamage;
            HeatDamage += second.HeatDamage;
            DirectDamage += second.DirectDamage;
            ShieldDamage += second.ShieldDamage;
            Repair += second.Repair;
            Effects |= second.Effects;
            Impulse = Impulse == null ? second.Impulse : Impulse.Append(second.Impulse);
        }
    }
}
