using System;
using System.Collections.Generic;
using System.Linq;
using CombatSimulation.Core;
using CombatSimulation.Modes;
using Constructor;
using Game.Exploration;
using GameDatabase;
using UnityEngine;
using EnvironmentObjectType = Game.Exploration.ExplorationData.EnvironmentObjectType;

namespace Combat.Integration.Core
{
    // Main's exploration level on the core (exploration plan, step 6). It turns ExplorationData into the layout input,
    // the battle setup (the player's unit, the gas clouds, no asteroids) and the exploration rules. The legacy reference
    // is ExplorationSceneManager; the shared layout places everything again (risk 2).
    public sealed class CoreExplorationSetup
    {
        // Legacy ExplorationSceneInstaller: a 1000 x 1000 world.
        public const float WorldSize = 1000f;

        public const int PlayerSide = 0;
        public static readonly UnitId PlayerUnit = new UnitId(1);

        // The mode's units start above the player's (ExplorationLayoutInput.UnitIdBase).
        public const uint UnitIdBase = 2;

        // Legacy CreateEnvironment: 100 environment objects.
        public const int EnvironmentObjects = 100;

        private CoreExplorationSetup(ExplorationLayout layout, CoreExplorationContent content, ExplorationRules rules,
            BattleSetup setup, int enemyAiLevel)
        {
            Layout = layout;
            Content = content;
            Rules = rules;
            Setup = setup;
            EnemyAiLevel = enemyAiLevel;
        }

        public ExplorationLayout Layout { get; }
        public CoreExplorationContent Content { get; }
        public ExplorationRules Rules { get; }
        public BattleSetup Setup { get; }

        // E6: Maths.Distance.AiLevel(level) for every enemy, CorePilotProvider's enemy AI level.
        public int EnemyAiLevel { get; }

        // player: the legacy exploration ship with the player's skills. completedObjectives: CompletedObjectives below.
        // matchSeed: the core's seed; it does not move the layout, which comes from the planet seed.
        public static CoreExplorationSetup Create(ExplorationData exploration, IDatabase database, CoreShipMapper mapper,
            IShipSpecification player, IReadOnlyCollection<int> completedObjectives, ulong matchSeed)
        {
            if (exploration == null) throw new ArgumentNullException(nameof(exploration));
            if (database == null) throw new ArgumentNullException(nameof(database));
            if (mapper == null) throw new ArgumentNullException(nameof(mapper));
            if (player == null) throw new ArgumentNullException(nameof(player));

            var content = new CoreExplorationContent(exploration, mapper);
            var layout = ExplorationLayout.Generate(CreateLayoutInput(exploration, database, content.RandomShipCount));
            // player_lives.md: Main has no respawn; the battle ends 250 ticks after the death (legacy 5 s), with no question.
            var lives = new LivesSetup(0, PlayerOutDelayTicks, SpawnPlacement.At(layout.Start, layout.StartAngle));
            var rules = new ExplorationRules(layout, content, PlayerUnit, lives, completedObjectives);
            var roster = new[] { new UnitSetup(PlayerUnit, PlayerSide, mapper.Map(player, isDrone: false).Definition) };
            // Phase 2b (D12): a local battle keeps stepping after its result, through the exit delay.
            var setup = new BattleSetup(new BattleId(1), matchSeed, roster, WorldSize, WorldSize, rules, zones: layout.Zones,
                continueAfterResult: true);
            return new CoreExplorationSetup(layout, content, rules, setup, Maths.Distance.AiLevel(exploration.Level));
        }

        // E13: the set bits of the planet's data, as objective IDs.
        public static IReadOnlyCollection<int> CompletedObjectives(ExplorationData exploration) =>
            exploration.Objectives.Where(exploration.IsCompleted).Select(objective => objective.Id).ToList();

        // E4: a dockable objective's key and a hostile objective's key are its ID; each hostile objective has its own
        // guardian group, with the objective ID as the key. A random ship's key is its index in EnemyShips.
        public static ExplorationLayoutInput CreateLayoutInput(ExplorationData exploration, IDatabase database,
            int randomShipCount)
        {
            var objectives = new List<ExplorationObjectiveSetup>();
            var groups = new List<ExplorationGuardianGroup>();
            foreach (var objective in exploration.Objectives)
            {
                if (objective.Type == ObjectiveType.Outpost || objective.Type == ObjectiveType.Hive)
                {
                    var respawn = objective.Type == ObjectiveType.Hive ? HiveRespawnTicks : 0;
                    groups.Add(new ExplorationGuardianGroup(MinGuardians, MaxGuardians, GuardianRing, respawn, objective.Id));
                    objectives.Add(new ExplorationObjectiveSetup(ExplorationObjectiveKind.Hostile, HostileRadius,
                        objective.Id, groups.Count - 1));
                }
                else
                {
                    objectives.Add(new ExplorationObjectiveSetup(ExplorationObjectiveKind.Dockable,
                        DockingRadius(exploration, objective), objective.Id));
                }
            }

            var randomShips = new long[randomShipCount];
            for (var i = 0; i < randomShips.Length; ++i) randomShips[i] = i;

            // E8: corrosive, GasCloudDPS as given, without the legacy defense multiplier (owner, 2026-11-17).
            var zones = new ExplorationZones(GasCloudCount(exploration), GasCloudMinRadius, GasCloudMaxRadius,
                LegacyDamageModel.Corrosive, database.ExplorationSettings.GasCloudDPS(exploration.Level));

            return new ExplorationLayoutInput(unchecked((ulong)(uint)exploration.Seed), WorldSize, UnitIdBase, objectives,
                groups, randomShips, zones);
        }

        // E7: the legacy view size of a dockable objective, drawn from the objective's seed (legacy drew it from the
        // scene's shared random). Step 7 draws the view with it.
        public static float ObjectiveSize(ExplorationData exploration, ObjectiveInfo objective)
        {
            var random = new System.Random(objective.Seed);
            switch (objective.Type)
            {
                case ObjectiveType.Container: return 4f + random.NextFloat() * 3f;
                case ObjectiveType.Meteorite: return 5f + random.NextFloat() * 5f;
                case ObjectiveType.ShipWreck: return 5f + exploration.GetShipWreck(objective.Seed).ModelScale;
                case ObjectiveType.Minerals:
                case ObjectiveType.MineralsRare: return 5f + random.NextFloat() * 10f;
                default: throw new ArgumentException($"Objective {objective.Id} ({objective.Type}) has no view size.");
            }
        }

        // E7: the legacy trigger radius x the view size.
        public static float DockingRadius(ExplorationData exploration, ObjectiveInfo objective) =>
            TriggerRadius(exploration, objective.Type) * ObjectiveSize(exploration, objective);

        // E8: the gas clouds of the legacy environment.
        public static int GasCloudCount(ExplorationData exploration) =>
            LegacyEnvironment(exploration).Count(item => item.Type == EnvironmentObjectType.GasCloud);

        // Legacy CreateEnvironment, in its draw order from Seed + 3: the type, position, size and rotation of each
        // environment object. The position is in the legacy world, where the player starts at (0, 0).
        public static IEnumerable<(EnvironmentObjectType Type, Vector2 Position, int Size, int Rotation)> LegacyEnvironment(
            ExplorationData exploration)
        {
            var random = new System.Random(exploration.Seed + 3);
            for (var i = 0; i < EnvironmentObjects; ++i)
            {
                var x = random.NextFloat() * WorldSize;
                var y = random.NextFloat() * WorldSize;
                var size = 5 + random.Next(10);
                var rotation = random.Next(360);
                var type = exploration.GetEnvironmentObject(random);
                // Legacy draws a gas cloud's spin after its type.
                if (type == EnvironmentObjectType.GasCloud) random.NextFloatSigned();
                yield return (type, new Vector2(x, y), size, rotation);
            }
        }

        // The CircleCollider2D radius of each legacy prefab at scale 1 (Combat/Exploration). Box and Container: the
        // floating and the ground container. Rock, Asteroid, Volcano, Volcano2 and Vortex: 0.5.
        private static float TriggerRadius(ExplorationData exploration, ObjectiveType type)
        {
            switch (type)
            {
                case ObjectiveType.Container: return exploration.HasSolidGround ? 0.7f : 1.2f;
                case ObjectiveType.ShipWreck: return 0.8f;
                default: return 0.5f;
            }
        }

        // Legacy outpost and hive: 1 to 4 guardians on a ring of 10. A hive guardian respawns 30 s after its death.
        private const int MinGuardians = 1;
        private const int MaxGuardians = 4;
        private const float GuardianRing = 10f;
        private const long HiveRespawnTicks = 30 * TicksPerSecond;

        // The mode does not use a hostile objective's radius; the ship draws its own view.
        private const float HostileRadius = 1f;

        // Legacy gas cloud: the GasCloud collider (0.5) x a body size of 1.5 x (5 to 14).
        private const float GasCloudMinRadius = 0.5f * 1.5f * 5f;
        private const float GasCloudMaxRadius = 0.5f * 1.5f * 14f;

        private const long PlayerOutDelayTicks = 5 * TicksPerSecond;
        private const int TicksPerSecond = 50;
    }
}
