using System.Collections.Generic;
using System.Linq;
using Constructor;
using CombatSimulation.Core;
using CombatSimulation.Runtime;
using GameDatabase;
using GameDatabase.DataModel;
using GameDatabase.Enums;
using GameServices.Multiplayer;
using CoreRosterId = CombatSimulation.Core.RosterId;

namespace Combat.Integration.Core
{
    // One roster slot of the duel as the last Build made it: the ship, its specification and its mapping.
    public sealed class CoreDuelSlot
    {
        public CoreDuelSlot(CoreRosterId rosterId, int side, Constructor.Ships.IShip ship, IShipSpecification specification,
            CoreShipMapping mapping)
        {
            RosterId = rosterId;
            Side = side;
            Ship = ship;
            Specification = specification;
            Mapping = mapping;
        }

        public CoreRosterId RosterId { get; }
        public int Side { get; }
        public Constructor.Ships.IShip Ship { get; }
        public IShipSpecification Specification { get; }
        public CoreShipMapping Mapping { get; }
    }

    // Builds the online duel's BattleSetup from the match plan (phase 7, step 4). Both players run it on the same
    // database and fleets; the handshake compares the setup hashes. Each fleet keeps its order (Q3). The host's ships
    // come first: side 0, owner 0; the guest's: side 1, owner 1. One active ship per side. No skill bonuses and no
    // asteroids, whatever the rules say (step 6). Main thread: the hull fallback reads sprites.
    // The battle scene reuses the mapper and the slots of the last Build: each Map call makes new definitions, and the
    // views look the session's definitions up in the mapper that made them (step 6).
    public sealed class CoreDuelSetup
    {
        private const int TicksPerSecond = 50;

        // rules: replaces the duel rules record; the editor probe uses it to test other rules (step 6 check).
        public CoreDuelSetup(IDatabase database, CoreShipMapper mapper, CombatRules rules = null)
        {
            Database = database;
            Mapper = mapper;
            RulesRecord = rules ?? Rules(database);
        }

        public IDatabase Database { get; }
        public CoreShipMapper Mapper { get; }
        public CombatRules RulesRecord { get; }

        // The slots of the last Build, in roster order.
        public IReadOnlyList<CoreDuelSlot> Slots => _slots;

        // GalaxySettings.OnlineDuelRules; a database without it uses the default rules, as quick combat does.
        public static CombatRules Rules(IDatabase database) =>
            database.GalaxySettings.OnlineDuelRules ?? database.CombatSettings.DefaultCombatRules;

        // Star level 0 and no rescue unit, as quick combat.
        public CombatRulesAdapter CreateRules() => RulesRecord.Create(0, false);

        public BattleSetup Build(MatchPlan plan)
        {
            var rules = CreateRules();
            _slots.Clear();
            var roster = new List<RosterSlotSetup>();
            var nextId = 1u;
            AddFleet(plan.HostFleet, MatchPlan.HostSide, MatchPlan.HostOwner, roster, ref nextId);
            AddFleet(plan.GuestFleet, MatchPlan.GuestSide, MatchPlan.GuestOwner, roster, ref nextId);

            var size = rules.BattleMapSize;
            return new BattleSetup(plan.BattleId, plan.Seed, roster, size, size, (long)rules.TimeLimit * TicksPerSecond,
                maxActiveShipsPerSide: 1, localOwners: plan.LocalOwners, ownerDelays: plan.OwnerDelays,
                timerMode: CoreCombatAdapter.ToTimerMode(rules.TimeOutMode), playerSide: MatchPlan.HostSide,
                onlyOneShip: rules.ShipSelection == PlayerShipSelectionMode.OnlyOneShip);
        }

        private void AddFleet(byte[] fleet, int side, int owner, List<RosterSlotSetup> roster, ref uint nextId)
        {
            foreach (var ship in FleetWebSerializer.DeserializeFleetData(Database, fleet).ToList())
            {
                // The new simulator gets every satellite mount point (satellite plan, phase 7, step 4).
                var builder = ship.CreateBuilder();
                builder.AllMountPoints = true;
                var specification = builder.Build(Database.ShipSettings);
                var mapping = Mapper.Map(specification, isDrone: false);
                var id = new CoreRosterId(nextId++);
                roster.Add(new RosterSlotSetup(id, side, mapping.Definition, owner));
                _slots.Add(new CoreDuelSlot(id, side, ship, specification, mapping));
            }
        }

        private readonly List<CoreDuelSlot> _slots = new List<CoreDuelSlot>();
    }
}
