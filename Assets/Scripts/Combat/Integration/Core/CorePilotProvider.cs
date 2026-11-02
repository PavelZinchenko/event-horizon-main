using System;
using System.Collections.Generic;
using CombatSimulation.Ai;
using CombatSimulation.Core;
using CombatSimulation.Physics;
using CombatSimulation.Runtime;
using GameDatabase.DataModel;
using GameDatabase.Model;
using UnityEngine;
using CoreEntityId = CombatSimulation.Core.EntityId;

namespace Combat.Integration.Core
{
    // Gives every ship its pilot by the legacy ShipFactory and ControllerFactory rules (phase 4, step 7f; T6). A player
    // ship gets a new player pilot, with the autopilot tree when it has the autopilot component. An enemy ship runs its
    // own tree or the enemy tree at the fleet's AI level; a starbase its own or the starbase tree; a drone the tree of
    // its bay; a clone its own or the clone tree. The runner creates the pilot at the spawn event, so it exists before the
    // adapter handles that event.
    public sealed class CorePilotProvider : IPilotProvider
    {
        // playerSide −1: no player pilots, every ship runs its tree (Editor checks). An AI level below 0 gives enemy
        // ships no tree (legacy EmptyController, for testing); they keep the automatic keys.
        public CorePilotProvider(CombatSettings settings, BehaviorTreeFactory trees,
            IReadOnlyDictionary<ShipDefinition, CoreShipMapping> mappings, int playerSide, int enemyAiLevel)
        {
            _settings = settings;
            _trees = trees;
            _mappings = mappings;
            _playerSide = playerSide;
            _enemyAiLevel = enemyAiLevel;
        }

        // The tree factory of one battle: profiles and bullet scales come from the mapper (steps 7b and 7c).
        public static BehaviorTreeFactory CreateTrees(ulong seed, Torus arena, CoreShipMapper mapper,
            Action<CoreEntityId, string, ColorData> broadcast = null)
        {
            var mappings = mapper.Mappings;
            var sources = mapper.BulletSources;
            return new BehaviorTreeFactory(seed, arena,
                definition => mappings.TryGetValue(definition, out var mapping) ? mapping.Ai : null,
                bullet => sources.TryGetValue(bullet, out var source) ? source.BodySize : DefaultBulletScale,
                broadcast);
        }

        public IPilot CreatePilot(BattleSnapshot snapshot, in ShipState ship, out PilotSchedule schedule)
        {
            schedule = ship.IsDrone ? PilotSchedule.Drone : PilotSchedule.Ship;
            if (!_mappings.TryGetValue(ship.Definition, out var mapping))
            {
                Debug.LogError($"CorePilotProvider: no mapping for entity {ship.Id}; it gets no pilot");
                return null;
            }

            var autoActivation = CoreAutoActivation.Create(mapping);

            // Legacy CreateCloneController: AiSettings.Default. A clone flies as a ship, so it thinks every tick.
            if (mapping.IsClone)
            {
                schedule = PilotSchedule.Ship;
                if (mapping.AiTree == null) LogNoTree(mapping, "clone");
                return Tree(mapping.AiTree, mapping, AiSettings.Default, ship.Id, schedule, autoActivation);
            }

            if (ship.IsDrone)
            {
                var tree = mapping.AiTree;
                if (tree == null) LogNoTree(mapping, "drone");
                return Tree(tree, mapping, AiSettings.ForDrone(mapping.DroneRange), ship.Id, schedule, autoActivation);
            }

            if (ship.Side == _playerSide)
            {
                var pilot = new PlayerPilot(autoActivation);
                _playerPilots[ship.Id] = pilot;
                schedule = PlayerPilot.Schedule;
                if (!mapping.HasAutopilot)
                    return pilot;

                // Legacy falls back to the hard-coded Computer controller without a tree; here the player flies alone.
                var autopilot = BehaviorTreeSelection.ForAutopilot(_settings);
                if (autopilot == null)
                {
                    LogNoTree(mapping, "autopilot");
                    return pilot;
                }

                return new AutoActivationPilot(_trees.CreateAutopilot(pilot, autopilot, mapping.Ai, ship.Id), autoActivation);
            }

            if (mapping.Ai.IsStarbase)
            {
                var tree = BehaviorTreeSelection.ForStarbase(_settings, mapping.AiTree);
                if (tree == null) LogNoTree(mapping, "starbase");
                return Tree(tree, mapping, AiSettings.Default, ship.Id, schedule, autoActivation);
            }

            if (_enemyAiLevel < 0)
                return Tree(null, mapping, AiSettings.Default, ship.Id, schedule, autoActivation);

            var enemyTree = BehaviorTreeSelection.ForEnemy(_settings, mapping.AiTree);
            if (enemyTree == null) LogNoTree(mapping, "enemy");
            return Tree(enemyTree, mapping, AiSettings.FromAiLevel(_enemyAiLevel), ship.Id, schedule, autoActivation);
        }

        public bool TryGetPlayerPilot(CoreEntityId ship, out PlayerPilot pilot) => _playerPilots.TryGetValue(ship, out pilot);

        public void Release(CoreEntityId ship) => _playerPilots.Remove(ship);

        public void Clear() => _playerPilots.Clear();

        private IPilot Tree(BehaviorTreeModel tree, CoreShipMapping mapping, AiSettings settings, CoreEntityId ship,
            PilotSchedule schedule, CoreAutoActivation autoActivation) =>
            new AutoActivationPilot(_trees.Create(tree, mapping.Ai, settings, ship, schedule), autoActivation);

        // T6: a null tree idles the ship (automatic keys only); logged once per mapped ship.
        private void LogNoTree(CoreShipMapping mapping, string kind)
        {
            if (_loggedNoTree.Add(mapping))
                Debug.LogWarning($"CorePilotProvider: {mapping.Name} has no {kind} tree; it idles");
        }

        // Legacy Body.Scale of a bullet without view data; every mapped bullet has one.
        private const float DefaultBulletScale = 1f;

        private readonly CombatSettings _settings;
        private readonly BehaviorTreeFactory _trees;
        private readonly IReadOnlyDictionary<ShipDefinition, CoreShipMapping> _mappings;
        private readonly int _playerSide;
        private readonly int _enemyAiLevel;
        private readonly Dictionary<CoreEntityId, PlayerPilot> _playerPilots = new Dictionary<CoreEntityId, PlayerPilot>();
        private readonly HashSet<CoreShipMapping> _loggedNoTree = new HashSet<CoreShipMapping>();
    }

    // Legacy ShipSystems applies the automatic key rule every frame, whatever the controller does ("Ship mapping (3a)",
    // row 10). A tree pilot submits nothing while its root keeps failing (T7), so this pilot submits the kept frame again
    // when the automatic bits change. One-shot commands are never sent twice.
    public sealed class AutoActivationPilot : IPilot
    {
        public AutoActivationPilot(IPilot pilot, CoreAutoActivation autoActivation)
        {
            Pilot = pilot;
            _autoActivation = autoActivation;
        }

        public IPilot Pilot { get; }

        public bool Think(BattleSnapshot snapshot, in ShipState self, out PilotControls controls)
        {
            if (Pilot.Think(snapshot, self, out controls))
            {
                controls = With(controls, _autoActivation.Apply(snapshot, self, controls.Systems), keepCommands: true);
                _kept = With(controls, controls.Systems, keepCommands: false);
                return true;
            }

            var systems = _autoActivation.Apply(snapshot, self, _kept.Systems);
            if (systems == _kept.Systems)
                return false;

            _kept = With(_kept, systems, keepCommands: false);
            controls = _kept;
            return true;
        }

        private static PilotControls With(in PilotControls controls, ulong systems, bool keepCommands) =>
            PilotControls.Create(controls.HasCourse ? controls.Course : (float?)null, controls.Throttle, systems,
                keepCommands && controls.SelfDestruct, keepCommands && controls.Vanish,
                keepCommands && controls.ChangesAllegiance ? controls.Allegiance : (Allegiance?)null);

        private readonly CoreAutoActivation _autoActivation;

        // The frame in force: the last one submitted, without its commands. Before the first frame, zero controls.
        private PilotControls _kept = PilotControls.Create(null, 0f, 0UL);
    }
}
