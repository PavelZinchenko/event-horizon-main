using System;
using System.Collections.Generic;
using System.Linq;
using CombatSimulation.Core;
using CombatSimulation.Modes;
using Game.Exploration;

namespace Combat.Integration.Core
{
    // Main's exploration ships on the core (exploration plan, step 6; E4, E5, E6). A content key is an index into lists
    // that Main builds before the battle: a random ship's key is its index in ExplorationData.EnemyShips (the build and
    // the seed Seed + i); the key of a hostile objective and of its guardian group is the objective ID. The ships are the
    // legacy builders' ships. The outpost, its turrets and the hive are stationary (E15).
    //
    // The mode calls CreateShip in BeginTick. Main steps a local battle on the main thread (CoreBattleHost), so the
    // mapper and its sprite reads are safe there.
    public sealed class CoreExplorationContent : IExplorationContent
    {
        public CoreExplorationContent(ExplorationData exploration, CoreShipMapper mapper)
        {
            _exploration = exploration ?? throw new ArgumentNullException(nameof(exploration));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _randomShips = exploration.EnemyShips.ToList();
        }

        // The random ships' content keys are 0 to RandomShipCount - 1.
        public int RandomShipCount => _randomShips.Count;

        public ShipDefinition CreateShip(ExplorationShipRole role, long contentKey, int ordinal) =>
            Map(role, contentKey, ordinal).Definition;

        // Cached by role, key and ordinal (E4): a cache by role and key alone gives every guardian of a group the first
        // guardian's ship.
        public CoreShipMapping Map(ExplorationShipRole role, long contentKey, int ordinal)
        {
            var key = (role, contentKey, ordinal);
            if (!_cache.TryGetValue(key, out var mapping))
                _cache.Add(key, mapping = Build(role, contentKey, ordinal));
            return mapping;
        }

        private CoreShipMapping Build(ExplorationShipRole role, long contentKey, int ordinal)
        {
            switch (role)
            {
                case ExplorationShipRole.RandomShip:
                    return _mapper.Map(_randomShips[Index(contentKey, _randomShips.Count)].CreateSpecification(), isDrone: false);

                case ExplorationShipRole.Hostile:
                {
                    var objective = Objective(contentKey);
                    if (objective.Type == ObjectiveType.Outpost)
                        return _mapper.MapStationary(_exploration.GetOutpost(objective.Seed).CreateSpecification(), isTurret: true);
                    if (objective.Type == ObjectiveType.Hive)
                        return _mapper.MapStationary(_exploration.GetHive(objective.Seed).CreateSpecification(), isTurret: false);
                    break;
                }

                // Legacy: turret j has the seed objective.Seed + j. E5: the hive guardian takes the same seed.
                case ExplorationShipRole.Guardian:
                {
                    var objective = Objective(contentKey);
                    if (objective.Type == ObjectiveType.Outpost)
                        return _mapper.MapStationary(_exploration.GetTurret(objective.Seed + ordinal).CreateSpecification(), isTurret: true);
                    if (objective.Type == ObjectiveType.Hive)
                        return _mapper.Map(_exploration.GetHiveGuardian(objective.Seed + ordinal).CreateSpecification(), isDrone: false);
                    break;
                }
            }

            throw new ArgumentException($"CoreExplorationContent: no {role} ship for the content key {contentKey}.");
        }

        private ObjectiveInfo Objective(long contentKey)
        {
            var objectives = _exploration.Objectives;
            return objectives[Index(contentKey, objectives.Count)];
        }

        private static int Index(long contentKey, int count)
        {
            if (contentKey < 0 || contentKey >= count)
                throw new ArgumentOutOfRangeException(nameof(contentKey), $"CoreExplorationContent: unknown content key {contentKey}.");
            return (int)contentKey;
        }

        private readonly ExplorationData _exploration;
        private readonly CoreShipMapper _mapper;
        private readonly List<IEnemyShipBuilder> _randomShips;
        private readonly Dictionary<(ExplorationShipRole, long, int), CoreShipMapping> _cache =
            new Dictionary<(ExplorationShipRole, long, int), CoreShipMapping>();
    }
}
