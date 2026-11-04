using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Combat.Integration.Core;
using CombatSimulation.Ai;
using CombatSimulation.Core;
using CombatSimulation.Physics;
using CombatSimulation.Runtime;
using Constructor;
using GameDatabase;
using GameDatabase.DataModel;
using GameDatabase.Enums;
using Services.Resources;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

// The M1 batch check (phase 4, owner 2026-10-31): every built-in build fights a short core battle without rendering.
// The build flies to a fixed fixture target, brakes at half its shortest weapon range (core ships drift at throttle 0,
// as in legacy), turns its first weapon's barrel to the target, and holds each weapon while the target is in its range
// and its barrel points at it. Ships that cannot turn hold their weapons all the time. A bullet without a range (an area
// bullet at the muzzle, as the AntiMatterBomb carrier) reaches as far as its radius. A charging weapon fires at the release
// (phase 5, step 4f): the pilot holds its key until the charge is full and releases it on target, as the AI's ChargeWeapons
// and IsReadyToFire. Ships that cannot turn release it at each full charge. Drone bays are always held;
// drones run their behaviour trees, as in the core scene (step 7f). After the fight, kill-all from the target side destroys
// the build; the removal reason is Death, as for armor 0.
// Failures: exceptions, armed builds that never fired, drone bays that never launched, ships with an engine that never
// moved, and ships that did not die. Builds that fired but never hit are listed for information only: M1 does not ask
// for hits, and this simple pilot misses with some ships (it fires while sliding, so bullets inherit the drift). The
// visual part of the M1 check stays with the owner's playtest.
// A build that spawns within its stopping distance first demonstrates movement, then holds range. Otherwise a
// long-range weapon can keep the pilot at throttle 0 for the entire check and falsely fail the engine check.
public static class CoreShipBattleCheck
{
    [MenuItem("Tools/Combat/Check Ship Battles")]
    public static void Run()
    {
        var database = UnityDatabaseFactory.Create();
        database.LoadDefault();
        var mapper = new CoreShipMapper(database, Resources.Load<ResourceLocator>("ResourceLocator"));
        var builds = database.ShipBuildList.Where(item => item.Ship.ShipType != ShipType.Drone).OrderBy(item => item.Id.Value).ToList();

        var results = new List<Result>();
        var clock = Stopwatch.StartNew();
        try
        {
            for (var i = 0; i < builds.Count; ++i)
            {
                var id = builds[i].Id.Value;
                if (EditorUtility.DisplayCancelableProgressBar("Check Ship Battles", $"build {id}", (float)i / builds.Count))
                    break;

                var result = new Result(id);
                try
                {
                    var mapping = mapper.Map(new ShipBuilder(builds[i]).Build(database.ShipSettings), isDrone: false);
                    Fight(mapping, mapper, database.CombatSettings, result);
                }
                catch (Exception e)
                {
                    result.Exception = $"{e.GetType().Name}: {e.Message}";
                }

                results.Add(result);
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }

        Debug.Log(Report(results, clock.Elapsed));
    }

    private static void Fight(CoreShipMapping mapping, CoreShipMapper mapper, CombatSettings settings, Result result)
    {
        var definition = mapping.Definition;
        result.Armed = definition.Devices.Any(device => device is CannonDefinition || device is BeamDefinition);
        result.HasBays = definition.Devices.Any(device => device is DroneBayDefinition);
        result.HasEngine = definition.Engine.Propulsion > 0f;
        result.CanTurn = definition.Engine.TurnRate > 0f;
        result.Weapons = string.Join(", ", definition.Devices.Select((device, index) => Describe(definition, device, index)).Where(text => text != null));

        var roster = new[]
        {
            new RosterSlotSetup(BuildSlot, 0, definition),
            new RosterSlotSetup(TargetSlot, 1, Target),
        };
        var setup = new BattleSetup(new BattleId(1), (ulong)result.BuildId, roster, ArenaSize, ArenaSize);
        var torus = new Torus(setup.Width, setup.Height);
        // The fixture target has no mapping, so the trees get its profile here.
        var mappings = mapper.Mappings;
        var trees = new BehaviorTreeFactory(setup.MatchSeed, torus,
            ship => ship == Target ? TargetProfile : mappings.TryGetValue(ship, out var item) ? item.Ai : null,
            bullet => mapper.BulletSources.TryGetValue(bullet, out var source) ? source.BodySize : 1f);
        var drones = new CorePilotProvider(settings, trees, mappings, playerSide: -1, enemyAiLevel: HardAiLevel);
        var runner = new BattleRunner(setup, new Pilots(torus, mapper.Mappings, drones));
        runner.Deploy(BuildSlot, runner.Tick);
        runner.Deploy(TargetSlot, runner.Tick);

        var ship = default(EntityId);
        var target = default(EntityId);
        var start = default(Vec2);
        var started = false;
        var killSent = false;
        var bullets = new HashSet<EntityId>();
        for (var tick = 0; tick < FightTicks + DeathTicks && !runner.IsCompleted; ++tick)
        {
            runner.Step();
            var snapshot = runner.Snapshot;
            foreach (var item in snapshot.Events)
            {
                switch (item.Kind)
                {
                    case BattleEventKind.Spawn when item.RosterId == BuildSlot && !item.IsDrone:
                        ship = item.EntityId;
                        break;
                    case BattleEventKind.Spawn when item.RosterId == TargetSlot && !item.IsDrone:
                        target = item.EntityId;
                        break;
                    case BattleEventKind.Spawn when item.IsDrone && item.ParentId == ship:
                        ++result.Launches;
                        break;
                    case BattleEventKind.Spawn when item.IsBullet && item.ParentId == ship:
                        ++result.Shots;
                        bullets.Add(item.EntityId);
                        break;
                    // Only the build's own bullets and beams count: a ram is a hit too.
                    case BattleEventKind.Hit when item.EntityId == target && !target.IsNull && bullets.Contains(item.AttackerId):
                        ++result.Hits;
                        break;
                    case BattleEventKind.Removal when item.EntityId == ship && !ship.IsNull:
                        result.Died = item.RemovalReason == RemovalReason.Death;
                        break;
                }
            }

            if (!ship.IsNull && snapshot.TryGetShip(ship, out var current))
            {
                if (!started)
                {
                    start = current.Position;
                    started = true;
                }

                result.MaxDistance = Math.Max(result.MaxDistance, torus.Delta(start, current.Position).Length);
                if (!target.IsNull && snapshot.TryGetShip(target, out var other))
                {
                    var delta = torus.Delta(current.Position, other.Position);
                    var gap = delta.Length - current.Definition.HullRadius - other.Definition.HullRadius;
                    if (gap < result.MinGap)
                    {
                        result.MinGap = gap;
                        result.AimError = Math.Abs(Mathf.DeltaAngle(current.Angle * Mathf.Rad2Deg, Mathf.Atan2(delta.Y, delta.X) * Mathf.Rad2Deg));
                    }
                }
            }

            // The fight ends early once every expected kind of activity was seen.
            var done = (!result.Armed || result.Hits > 0 || !result.CanTurn && result.Shots > 0) && (!result.HasBays || result.Launches > 0) &&
                       (!result.HasEngine || result.MaxDistance > MinDistance);
            if (!killSent && !ship.IsNull && (tick >= FightTicks || done))
                killSent = runner.KillAll(1, runner.Tick);
        }
    }

    private static string Describe(ShipDefinition definition, DeviceDefinition device, int index)
    {
        switch (device)
        {
            case CannonDefinition cannon:
                return $"{index}: cannon range {cannon.Bullet.Range:F0} barrel {BarrelAngle(definition, cannon.BarrelIndex):F0} deg" +
                       $"{(cannon.Bullet.IsHoming ? " homing" : "")}{(cannon.Mode == CannonMode.Charging ? " charging" : "")}";
            case BeamDefinition beam:
                return $"{index}: beam range {beam.Range:F0} barrel {BarrelAngle(definition, beam.BarrelIndex):F0} deg{(beam.IsCharged ? " charging" : "")}";
            default:
                return null;
        }
    }

    private static float BarrelAngle(ShipDefinition definition, int index) => definition.Barrels[index].Angle * Mathf.Rad2Deg;

    private static string Report(List<Result> results, TimeSpan elapsed)
    {
        var report = new StringBuilder();
        report.AppendLine($"CoreShipBattleCheck: {results.Count} builds in {elapsed.TotalSeconds:F0} s; " +
                          $"armed {results.Count(item => item.Armed)}, with drone bays {results.Count(item => item.HasBays)}.");

        Section(report, "Exceptions", results.Where(item => item.Exception != null), item => item.Exception);
        var fought = results.Where(item => item.Exception == null).ToList();
        Section(report, "Armed, never fired", fought.Where(item => item.Armed && item.Shots == 0),
            item => $"closest gap {item.MinGap:F1}; {item.Weapons}");
        Section(report, "Information: fired, never hit the target, cannot turn",
            fought.Where(item => item.Shots > 0 && item.Hits == 0 && !item.CanTurn), item => $"{item.Shots} shots");
        Section(report, "Information: fired, never hit the target", fought.Where(item => item.Shots > 0 && item.Hits == 0 && item.CanTurn),
            item => $"{item.Shots} shots, closest gap {item.MinGap:F1} at heading error {item.AimError:F0} deg; {item.Weapons}");
        Section(report, "Drone bays, never launched", fought.Where(item => item.HasBays && item.Launches == 0), null);
        Section(report, "Engine, never moved", fought.Where(item => item.HasEngine && item.MaxDistance <= MinDistance),
            item => $"moved {item.MaxDistance:F2}");
        Section(report, "Did not die", fought.Where(item => !item.Died), null);
        return report.ToString();
    }

    private static void Section(StringBuilder report, string title, IEnumerable<Result> items, Func<Result, string> detail)
    {
        var list = items.ToList();
        report.AppendLine($"{title}: {list.Count}");
        foreach (var item in list)
            report.AppendLine(detail == null ? $"  build {item.BuildId}" : $"  build {item.BuildId}: {detail(item)}");
    }

    private sealed class Pilots : IPilotProvider
    {
        public Pilots(Torus arena, IReadOnlyDictionary<ShipDefinition, CoreShipMapping> mappings, CorePilotProvider drones)
        {
            _arena = arena;
            _mappings = mappings;
            _drones = drones;
        }

        public IPilot CreatePilot(BattleSnapshot snapshot, in ShipState ship, out PilotSchedule schedule)
        {
            schedule = ship.IsDrone ? PilotSchedule.Drone : PilotSchedule.Ship;
            if (ship.Side != 0)
                return null;

            if (ship.IsDrone)
                return _drones.CreatePilot(snapshot, ship, out schedule);

            _mappings.TryGetValue(ship.Definition, out var mapping);
            var autoActivation = CoreAutoActivation.Create(mapping);

            // Inert devices stay off (D6); devices without a weapon or bay follow the scene's rules.
            var definition = ship.Definition;
            var devices = definition.Devices;
            var weapons = new Weapon[devices.Count];
            var bays = 0UL;
            var shortest = float.MaxValue;
            var aimOffset = float.NaN;
            for (var i = 0; i < devices.Count; ++i)
            {
                switch (devices[i])
                {
                    case CannonDefinition cannon:
                        var bullet = cannon.Bullet;
                        weapons[i] = new Weapon(bullet.Range > 0f ? bullet.Range : bullet.Radius, definition.Barrels[cannon.BarrelIndex].Angle,
                            (bullet.IsHoming ? HomingAimTolerance : AimTolerance) * Mathf.Deg2Rad, cannon.Mode == CannonMode.Charging);
                        break;
                    case BeamDefinition beam:
                        weapons[i] = new Weapon(beam.Range, definition.Barrels[beam.BarrelIndex].Angle, AimTolerance * Mathf.Deg2Rad, beam.IsCharged);
                        break;
                    case DroneBayDefinition _:
                        bays |= 1UL << i;
                        continue;
                    default:
                        continue;
                }

                shortest = Math.Min(shortest, weapons[i].Range);
                if (float.IsNaN(aimOffset))
                    aimOffset = weapons[i].Angle;
            }

            return new ApproachPilot(_arena, shortest < float.MaxValue ? 0.5f * shortest : DefaultStandOff,
                float.IsNaN(aimOffset) ? 0f : aimOffset, weapons, bays, definition.Engine.TurnRate > 0f, autoActivation);
        }

        private readonly Torus _arena;
        private readonly IReadOnlyDictionary<ShipDefinition, CoreShipMapping> _mappings;
        private readonly CorePilotProvider _drones;
    }

    private readonly struct Weapon
    {
        public Weapon(float range, float angle, float tolerance, bool charging)
        {
            Range = range;
            Angle = angle;
            Tolerance = tolerance;
            Charging = charging;
        }

        // Zero for a device that is not a weapon.
        public readonly float Range;

        // The barrel angle relative to the ship, and the largest aim error the weapon fires at; radians.
        public readonly float Angle;
        public readonly float Tolerance;

        // The weapon fires when its key is released, with the charge it reached.
        public readonly bool Charging;
    }

    // Flies to the nearest enemy at a speed that falls with the distance and brakes against its own velocity when it is
    // too fast or inside StandOff. Otherwise it stops thrusting and turns so that the aim barrel faces the enemy.
    private sealed class ApproachPilot : IPilot
    {
        public ApproachPilot(Torus arena, float standOff, float aimOffset, Weapon[] weapons, ulong alwaysHeld, bool canTurn,
            CoreAutoActivation autoActivation)
        {
            _arena = arena;
            _standOff = standOff;
            _aimOffset = aimOffset;
            _weapons = weapons;
            _alwaysHeld = alwaysHeld;
            _canTurn = canTurn;
            _autoActivation = autoActivation;
        }

        public bool Think(BattleSnapshot snapshot, in ShipState self, out PilotControls controls)
        {
            if (!_started)
            {
                _start = self.Position;
                _started = true;
            }
            if (_arena.Delta(_start, self.Position).Length > MinDistance) _hasMoved = true;
            var found = false;
            var delta = default(Vec2);
            var gap = float.MaxValue;
            foreach (var other in snapshot.Ships)
            {
                if (other.Side == self.Side) continue;
                var candidate = _arena.Delta(self.Position, other.Position);
                var distance = candidate.Length - other.Definition.HullRadius - self.Definition.HullRadius;
                if (distance >= gap) continue;
                found = true;
                gap = distance;
                delta = candidate;
            }

            if (!found)
            {
                controls = PilotControls.Create(null, 0f, _autoActivation.Apply(snapshot, self, _alwaysHeld));
                return true;
            }

            var toEnemy = (float)Math.Atan2(delta.Y, delta.X);
            var velocity = self.LinearVelocity;
            var length = delta.Length;
            var closing = length > 0f ? (velocity.X * delta.X + velocity.Y * delta.Y) / length : 0f;
            var demonstrateMovement = !_hasMoved && self.Definition.Engine.Propulsion > 0f;
            var approach = gap > _standOff || demonstrateMovement;
            var desired = approach ? Math.Min(MaxApproachSpeed, 0.5f * Math.Max(0f, gap - _standOff) + 1f) : 0f;

            if (closing > desired + 1f || !approach && velocity.Length > 1f)
                controls = PilotControls.Create((float)Math.Atan2(-velocity.Y, -velocity.X), 1f, Systems(snapshot, self, gap, toEnemy));
            else if (approach && closing < desired)
                controls = PilotControls.Create(toEnemy, 1f, Systems(snapshot, self, gap, toEnemy));
            else
                controls = PilotControls.Create(toEnemy - _aimOffset, 0f, Systems(snapshot, self, gap, toEnemy));
            return true;
        }

        // A turning ship fires each weapon while the target is in its range and its barrel points at it; a ship that
        // cannot turn fires all the time. A charging weapon is held until its charge is full and released on target.
        private ulong Systems(BattleSnapshot snapshot, in ShipState self, float gap, float toEnemy)
        {
            var held = _alwaysHeld;
            var devices = snapshot.GetDevices(self);
            for (var i = 0; i < _weapons.Length; ++i)
            {
                var weapon = _weapons[i];
                if (weapon.Range <= 0f) continue;
                var onTarget = !_canTurn || gap <= weapon.Range && AimError(self.Angle + weapon.Angle, toEnemy) <= weapon.Tolerance;
                if (weapon.Charging ? !onTarget || devices[i].Charge < 1f : onTarget)
                    held |= 1UL << i;
            }

            return _autoActivation.Apply(snapshot, self, held);
        }

        private static float AimError(float heading, float direction) =>
            Math.Abs(Mathf.DeltaAngle(heading * Mathf.Rad2Deg, direction * Mathf.Rad2Deg)) * Mathf.Deg2Rad;

        private const float MaxApproachSpeed = 8f;

        private readonly Torus _arena;
        private readonly float _standOff;
        private readonly float _aimOffset;
        private readonly Weapon[] _weapons;
        private readonly ulong _alwaysHeld;
        private readonly bool _canTurn;
        private readonly CoreAutoActivation _autoActivation;
        private Vec2 _start;
        private bool _started;
        private bool _hasMoved;
    }

    private sealed class Result
    {
        public Result(int buildId) => BuildId = buildId;

        public readonly int BuildId;
        public string Exception;
        public bool Armed;
        public bool HasBays;
        public bool HasEngine;
        public bool CanTurn;
        public int Shots;
        public int Hits;
        public int Launches;
        public float MaxDistance;
        public bool Died;
        public float MinGap = float.MaxValue;
        public float AimError;
        public string Weapons;
    }

    // A fixed, unkillable and unarmed circle. Its mass keeps pushes and rams from moving it; it deals no ramming damage.
    private static readonly ShipDefinition Target = new ShipDefinition(3f, 1e6f, new EngineDefinition(0f, 0f, 0f, 0f), 1e9f,
        new ResourceDefinition(0f), new ResourceDefinition(0f), rammingDamageMultiplier: 0f);

    // Unarmed, no engine; legacy body scale 6 for the hull radius 3.
    private static readonly AiShipProfile TargetProfile = new AiShipProfile(null, null, null, isDrone: false, isStarbase: false,
        SizeClass.Undefined, enginePower: 0f, turnRate: 0f, propulsion: 0f, kineticResistance: 0f, rammingDamageMultiplier: 0f,
        modelScale: 6f, AiTargetPriority.Normal, chanceToAvoidDrone: 0f);

    private const int HardAiLevel = 100;

    private static readonly RosterId BuildSlot = new RosterId(1);
    private static readonly RosterId TargetSlot = new RosterId(2);

    private const float ArenaSize = 200f;
    private const float MinDistance = 1f;
    private const float DefaultStandOff = 8f;
    private const float AimTolerance = 30f;
    private const float HomingAimTolerance = 90f;

    // 120 s to fight at 50 ticks per second, then 2 s for the death. The slowest builds (top speed under 2 units per
    // second) need about 50 s to cross the spawn distance; the fight ends early once every check has passed.
    private const int FightTicks = 6000;
    private const int DeathTicks = 100;
}
