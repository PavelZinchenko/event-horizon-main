using System.Runtime.CompilerServices;
using Combat.Component.Unit;
using Combat.Unit;
using UnityEngine;

namespace Combat.Integration.Legacy
{
    // Reads a legacy objective unit through IBeaconTarget, for the legacy exploration scene.
    public sealed class LegacyBeaconTarget : IBeaconTarget
    {
        private LegacyBeaconTarget(IUnit unit)
        {
            _unit = unit;
        }

        // The same target for the same unit, so RadarPanel.RemoveBeacon finds the beacon that AddBeacon opened.
        public static LegacyBeaconTarget Of(IUnit unit)
        {
            return unit == null ? null : Targets.GetValue(unit, item => new LegacyBeaconTarget(item));
        }

        public Vector2 Position => _unit.Body.Position;
        public bool IsActive => _unit.IsActive();

        private readonly IUnit _unit;

        private static readonly ConditionalWeakTable<IUnit, LegacyBeaconTarget> Targets =
            new ConditionalWeakTable<IUnit, LegacyBeaconTarget>();
    }
}
