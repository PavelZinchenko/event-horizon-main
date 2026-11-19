using System;
using System.Collections.Generic;
using System.Linq;
using CombatSimulation.Core;
using CombatSimulation.Modes;
using Constructor;
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
        public CoreShipMapping Map(ExplorationShipRole role, long contentKey, int ordinal) =>
            Get(role, contentKey, ordinal).Mapping;

        // The legacy specification of the same ship, for its HUD view (step 8).
        public IShipSpecification Specification(ExplorationShipRole role, long contentKey, int ordinal) =>
            Get(role, contentKey, ordinal).Specification;

        private (CoreShipMapping Mapping, IShipSpecification Specification) Get(ExplorationShipRole role, long contentKey,
            int ordinal)
        {
            var key = (role, contentKey, ordinal);
            if (!_cache.TryGetValue(key, out var entry))
                _cache.Add(key, entry = Build(role, contentKey, ordinal));
            return entry;
        }

        private (CoreShipMapping, IShipSpecification) Build(ExplorationShipRole role, long contentKey, int ordinal)
        {
            switch (role)
            {
                case ExplorationShipRole.RandomShip:
                    return Mapped(_randomShips[Index(contentKey, _randomShips.Count)].CreateSpecification());

                case ExplorationShipRole.Hostile:
                {
                    var objective = Objective(contentKey);
                    if (objective.Type == ObjectiveType.Outpost)
                        return Stationary(_exploration.GetOutpost(objective.Seed).CreateSpecification(), isTurret: true);
                    if (objective.Type == ObjectiveType.Hive)
                        return Stationary(_exploration.GetHive(objective.Seed).CreateSpecification(), isTurret: false);
                    break;
                }

                // Legacy: turret j has the seed objective.Seed + j. E5: the hive guardian takes the same seed.
                case ExplorationShipRole.Guardian:
                {
                    var objective = Objective(contentKey);
                    if (objective.Type == ObjectiveType.Outpost)
                        return Stationary(_exploration.GetTurret(objective.Seed + ordinal).CreateSpecification(), isTurret: true);
                    if (objective.Type == ObjectiveType.Hive)
                        return Mapped(_exploration.GetHiveGuardian(objective.Seed + ordinal).CreateSpecification());
                    break;
                }
            }

            throw new ArgumentException($"CoreExplorationContent: no {role} ship for the content key {contentKey}.");
        }

        private (CoreShipMapping, IShipSpecification) Mapped(IShipSpecification specification) =>
            (_mapper.Map(specification, isDrone: false), specification);

        private (CoreShipMapping, IShipSpecification) Stationary(IShipSpecification specification, bool isTurret) =>
            (_mapper.MapStationary(specification, isTurret), specification);

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
        private readonly Dictionary<(ExplorationShipRole, long, int), (CoreShipMapping Mapping, IShipSpecification Specification)>
            _cache = new Dictionary<(ExplorationShipRole, long, int), (CoreShipMapping, IShipSpecification)>();
    }
}
