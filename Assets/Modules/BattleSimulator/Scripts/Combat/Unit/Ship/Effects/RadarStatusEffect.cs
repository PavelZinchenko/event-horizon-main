using Combat.Component.Engine;
using Combat.Component.Features;
using Combat.Component.Ship;
using Combat.Component.Stats;
using Combat.Component.Systems;
using Combat.Component.Systems.Weapons;
using Combat.Component.Triggers;
using Combat.Component.Unit;
using Combat.Component.Unit.Classification;
using System.Collections.Generic;
using UnityEngine;

namespace Combat.Component.Ship.Effects
{
    public enum RadarStatusKind
    {
        Jammed,
        Stealthed,
    }

    public sealed class RadarStatusEffect : IShipEffect, IFeaturesModification
    {
        public RadarStatusEffect(RadarStatusKind kind, float duration, float energyDrainPerSecond = 0f)
        {
            _kind = kind;
            _remaining = Mathf.Max(0f, duration);
            _energyDrainPerSecond = Mathf.Max(0f, energyDrainPerSecond);
        }

        public RadarStatusKind Kind => _kind;
        public bool IsAlive => _remaining > 0f;

        public void Refresh(float duration, float energyDrainPerSecond = 0f)
        {
            _remaining = Mathf.Max(_remaining, Mathf.Max(0f, duration));
            _energyDrainPerSecond = Mathf.Max(_energyDrainPerSecond, Mathf.Max(0f, energyDrainPerSecond));
        }

        public void UpdatePhysics(IShip ship, float elapsedTime)
        {
            if (!IsAlive)
                return;

            var appliedTime = Mathf.Min(_remaining, Mathf.Max(0f, elapsedTime));
            _remaining -= appliedTime;

            if (_kind == RadarStatusKind.Jammed && _energyDrainPerSecond > 0f)
                ship.Stats.Energy.Get(_energyDrainPerSecond * appliedTime);
        }

        public void UpdateView(IShip ship, float elapsedTime) { }
        public void Dispose() { }

        public bool TryApplyModification(ref FeaturesData data)
        {
            if (!IsAlive)
                return false;

            if (_kind == RadarStatusKind.Stealthed)
                data.TargetPriority = TargetPriority.None;

            return true;
        }

        public IEngineModification EngineModification => null;
        public IFeaturesModification FeaturesModification => _kind == RadarStatusKind.Stealthed ? this : null;
        public ISystemsModification SystemsModification => null;
        public IStatsModification StatsModification => null;
        public IUnitAction UnitAction => null;

        private readonly RadarStatusKind _kind;
        private float _remaining;
        private float _energyDrainPerSecond;
    }

    public static class RadarStatus
    {
        public static bool IsJammed(IShip ship) => HasStatus(ship, RadarStatusKind.Jammed);
        public static bool IsStealthed(IShip ship) => HasStatus(ship, RadarStatusKind.Stealthed);
        public static bool IsStealthedFrom(IShip target, IShip observer)
        {
            if (!IsStealthed(target))
                return false;

            return observer == null || !HasStealthReveal(observer.Type.Side);
        }

        public static bool CanDetect(IShip observer, IShip target)
        {
            return observer != null && target != null && !IsJammed(observer) && !IsStealthedFrom(target, observer);
        }

        public static void ApplyJammed(IShip ship, float duration, float energyDrainPerSecond)
        {
            Apply(ship, RadarStatusKind.Jammed, duration, energyDrainPerSecond);
            ClearTargeting(ship);
        }

        public static void ApplyStealth(IShip ship, float duration)
        {
            Apply(ship, RadarStatusKind.Stealthed, duration, 0f);
        }

        public static void RevealStealthFor(UnitSide observerSide, float duration)
        {
            if (duration <= 0f)
                return;

            var until = Time.time + duration;
            if (_stealthRevealUntil.TryGetValue(observerSide, out var current) && current > until)
                return;

            _stealthRevealUntil[observerSide] = until;
        }

        private static bool HasStealthReveal(UnitSide observerSide)
        {
            if (!_stealthRevealUntil.TryGetValue(observerSide, out var until))
                return false;

            if (until > Time.time)
                return true;

            _stealthRevealUntil.Remove(observerSide);
            return false;
        }

        private static bool HasStatus(IShip ship, RadarStatusKind kind)
        {
            if (ship == null || ship.Effects == null)
                return false;

            foreach (var effect in ship.Effects.All)
                if (effect is RadarStatusEffect status && status.Kind == kind && status.IsAlive)
                    return true;

            return false;
        }

        private static void Apply(IShip ship, RadarStatusKind kind, float duration, float energyDrainPerSecond)
        {
            if (ship == null || ship.Effects == null || duration <= 0f)
                return;

            foreach (var effect in ship.Effects.All)
            {
                if (effect is not RadarStatusEffect status || status.Kind != kind)
                    continue;

                status.Refresh(duration, energyDrainPerSecond);
                return;
            }

            ship.AddEffect(new RadarStatusEffect(kind, duration, energyDrainPerSecond));
        }

        private static void ClearTargeting(IShip ship)
        {
            if (ship == null || ship.Systems == null)
                return;

            var systems = ship.Systems.All;
            for (int i = 0; i < systems.Count; i++)
            {
                if (systems[i] is not IWeapon weapon)
                    continue;

                weapon.Platform.ActiveTarget = null;
                if (ship.Controls != null)
                    ship.Controls.Systems.SetState(i, false);
            }
        }

        private static readonly Dictionary<UnitSide, float> _stealthRevealUntil = new();
    }
}
