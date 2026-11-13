using System.Collections.Generic;
using System.Linq;
using CombatSimulation.Core;
using CombatSimulation.Runtime;
using GameDatabase;
using GameDatabase.DataModel;
using GameDatabase.Enums;
using GameServices.Multiplayer;
using CoreRosterId = CombatSimulation.Core.RosterId;

namespace Combat.Integration.Core
{
    // Builds the online duel's BattleSetup from the match plan (phase 7, step 4). Both players run it on the same
    // database and fleets; the handshake compares the setup hashes. Each fleet keeps its order (Q3). The host's ships
    // come first: side 0, owner 0; the guest's: side 1, owner 1. One active ship per side. No skill bonuses and no
    // asteroids, whatever the rules say (step 6). Main thread: the hull fallback reads sprites.
    public sealed class CoreDuelSetup
    {
        private const int TicksPerSecond = 50;

        public CoreDuelSetup(IDatabase database, CoreShipMapper mapper)
        {
            _database = database;
            _mapper = mapper;
        }

        // GalaxySettings.OnlineDuelRules; a database without it uses the default rules, as quick combat does.
        public static CombatRules Rules(IDatabase database) =>
            database.GalaxySettings.OnlineDuelRules ?? database.CombatSettings.DefaultCombatRules;

        public BattleSetup Build(MatchPlan plan)
        {
            // Star level 0 and no rescue unit, as quick combat.
            var rules = Rules(_database).Create(0, false);
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
            foreach (var ship in FleetWebSerializer.DeserializeFleetData(_database, fleet).ToList())
            {
                var specification = ship.CreateBuilder().Build(_database.ShipSettings);
                var definition = _mapper.Map(specification, isDrone: false).Definition;
                roster.Add(new RosterSlotSetup(new CoreRosterId(nextId++), side, definition, owner));
            }
        }

        private readonly IDatabase _database;
        private readonly CoreShipMapper _mapper;
    }
}
