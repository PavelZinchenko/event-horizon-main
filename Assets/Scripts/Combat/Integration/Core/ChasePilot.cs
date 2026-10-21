using System;
using CombatSimulation.Core;
using CombatSimulation.Physics;
using CombatSimulation.Runtime;

namespace Combat.Integration.Core
{
    // Temporary AI for the core combat scene (migration phase 3c, owner decision S3); behavior trees replace it in phase 4.
    // A copy of the core tests' ScriptedBattle.ChasePilot: flies at the nearest enemy by the shortest torus vector and
    // holds a weapon bit while the enemy is in range and inside the weapon's aim cone.
    public sealed class ChasePilot : IPilot
    {
        public ChasePilot(ShipDefinition definition, Torus arena)
        {
            _arena = arena;

            var devices = definition.Devices;
            _range = new float[devices.Count];
            _aim = new float[devices.Count];
            var maxRange = 0f;
            for (var i = 0; i < devices.Count; ++i)
            {
                switch (devices[i])
                {
                    case CannonDefinition cannon:
                        _range[i] = cannon.Bullet.Range;
                        _aim[i] = (cannon.Bullet.IsHoming ? 60f : 20f) * DegreesToRadians;
                        break;
                    case BeamDefinition beam:
                        _range[i] = beam.Range;
                        _aim[i] = 10f * DegreesToRadians;
                        break;
                    default:
                        _alwaysOn |= 1UL << i;
                        break;
                }

                if (_range[i] > maxRange) maxRange = _range[i];
            }

            // Inside the stand-off distance the ship only turns, so the guns can aim; the fraction keeps the enemy in range.
            _standOff = StandOffFraction * maxRange;
        }

        public bool Think(BattleSnapshot snapshot, in ShipState self, out PilotControls controls)
        {
            var found = false;
            var nearest = Vec2.Zero;
            var nearestDistance = float.MaxValue;
            foreach (var other in snapshot.Ships)
            {
                if (other.Side == self.Side) continue;
                var delta = _arena.Delta(self.Position, other.Position);
                var distance = delta.Length - other.Definition.HullRadius - self.Definition.HullRadius;
                if (distance >= nearestDistance) continue;
                found = true;
                nearest = delta;
                nearestDistance = distance;
            }

            if (!found)
            {
                controls = PilotControls.Create(null, 0f, _alwaysOn);
                return true;
            }

            var course = (float)Math.Atan2(nearest.Y, nearest.X);
            var error = Math.Abs(DeltaAngle(self.Angle, course));
            var systems = _alwaysOn;
            for (var i = 0; i < _range.Length; ++i)
                if (_range[i] > 0f && nearestDistance <= _range[i] && error <= _aim[i]) systems |= 1UL << i;

            controls = PilotControls.Create(course, nearestDistance > _standOff ? 1f : 0f, systems);
            return true;
        }

        private static float DeltaAngle(float from, float to)
        {
            var delta = (to - from) % (2f * (float)Math.PI);
            if (delta > Math.PI) delta -= 2f * (float)Math.PI;
            if (delta < -Math.PI) delta += 2f * (float)Math.PI;
            return delta;
        }

        private const float StandOffFraction = 0.7f;
        private const float DegreesToRadians = (float)(Math.PI / 180.0);

        private readonly Torus _arena;
        private readonly float[] _range;
        private readonly float[] _aim;
        private readonly ulong _alwaysOn;
        private readonly float _standOff;
    }

    // Gives every ship a chase pilot. Step 5 gives the player ship its input pilot.
    public sealed class ChasePilotProvider : IPilotProvider
    {
        public ChasePilotProvider(Torus arena)
        {
            _arena = arena;
        }

        public IPilot CreatePilot(BattleSnapshot snapshot, in ShipState ship, out PilotSchedule schedule)
        {
            schedule = ship.IsDrone ? PilotSchedule.Drone : PilotSchedule.Ship;
            return new ChasePilot(ship.Definition, _arena);
        }

        private readonly Torus _arena;
    }
}
