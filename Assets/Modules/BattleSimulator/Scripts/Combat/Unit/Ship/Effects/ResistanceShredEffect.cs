using Combat.Collision;
using Combat.Component.Engine;
using Combat.Component.Features;
using Combat.Component.Stats;
using Combat.Component.Systems;
using Combat.Component.Triggers;
using GameDatabase.Enums;
using UnityEngine;

namespace Combat.Component.Ship.Effects
{
    public sealed class ResistanceShredEffect : IShipEffect, IStatsModification
    {
        public ResistanceShredEffect(DamageType type, float duration)
        {
            _type = type;
            _remaining = Mathf.Max(0f, duration);
        }

        public bool IsAlive => _remaining > 0f;
        public DamageType Type => _type;
        public int Stacks => _stacks;

        public void AddStack(float duration)
        {
            _stacks = Mathf.Min(3, _stacks + 1);
            _remaining = Mathf.Max(_remaining, Mathf.Max(0f, duration));
        }

        public bool TryApplyModification(ref Resistance data)
        {
            if (!IsAlive)
                return false;

            var reduction = 0.1f * _stacks;
            switch (_type)
            {
                case DamageType.Impact: data.Kinetic = Mathf.Max(0f, data.Kinetic - reduction); break;
                case DamageType.Energy: data.Energy = Mathf.Max(0f, data.Energy - reduction); break;
                case DamageType.Heat: data.Heat = Mathf.Max(0f, data.Heat - reduction); break;
                case DamageType.Corrosive: data.Corrosive = Mathf.Max(0f, data.Corrosive - reduction); break;
            }

            return true;
        }

        public void UpdatePhysics(IShip ship, float elapsedTime) => _remaining -= Mathf.Max(0f, elapsedTime);
        public void UpdateView(IShip ship, float elapsedTime) { }
        public void Dispose() { }

        public IEngineModification EngineModification => null;
        public IFeaturesModification FeaturesModification => null;
        public ISystemsModification SystemsModification => null;
        public IStatsModification StatsModification => this;
        public IUnitAction UnitAction => null;

        private int _stacks = 1;
        private float _remaining;
        private readonly DamageType _type;
    }

    public static class ResistanceShred
    {
        public static void Apply(IShip ship, DamageType type, float duration)
        {
            if (ship == null)
                return;

            foreach (var item in ship.Effects.All)
            {
                if (item is ResistanceShredEffect effect && effect.Type == type)
                {
                    effect.AddStack(duration);
                    return;
                }
            }

            ship.AddEffect(new ResistanceShredEffect(type, duration));
        }
    }
}
