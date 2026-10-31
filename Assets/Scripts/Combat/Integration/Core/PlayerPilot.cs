using System;
using CombatSimulation.Core;
using CombatSimulation.Runtime;

namespace Combat.Integration.Core
{
    // The pilot of one player ship deployment (migration phase 3c, step 5). Input and the HUD write the sink;
    // each tick the pilot submits the stored values. A new deployment gets a new pilot, so it starts with empty controls.
    public sealed class PlayerPilot : IPilot, IShipCommandSink
    {
        public PlayerPilot(CoreAutoActivation autoActivation) => _autoActivation = autoActivation;

        public static PilotSchedule Schedule => PilotSchedule.PlayerShip;

        public bool Think(BattleSnapshot snapshot, in ShipState self, out PilotControls controls)
        {
            controls = PilotControls.Create(_course * DegreesToRadians, _throttle, _autoActivation.Apply(snapshot, self, _systems));
            return true;
        }

        public void SetThrottle(float throttle)
        {
            if (float.IsNaN(throttle))
                return;

            _throttle = throttle < 0f ? 0f : throttle > 1f ? 1f : throttle;
        }

        // The core rejects a frame with a NaN or infinite course, so such a course is never stored.
        public void SetCourse(float? course)
        {
            if (course.HasValue && (float.IsNaN(course.Value) || float.IsInfinity(course.Value)))
                return;

            _course = course;
        }

        // The system ID is the device index, which is also the bit of the control frame.
        public void SetSystemState(int system, bool active)
        {
            if (system < 0 || system >= MaxSystems)
                return;

            var bit = 1UL << system;
            _systems = active ? _systems | bit : _systems & ~bit;
        }

        private const int MaxSystems = 64;
        private const float DegreesToRadians = (float)(Math.PI / 180.0);

        private readonly CoreAutoActivation _autoActivation;

        private float _throttle;
        private float? _course;
        private ulong _systems;
    }
}
