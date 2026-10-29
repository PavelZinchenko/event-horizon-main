using System.Collections.Generic;
using System.Linq;
using CombatSimulation.Core;
using GameDatabase.Model;

namespace Combat.Integration.Core
{
    // Describes the controllable systems of one core ship. Key groups follow legacy GetKeyBindings: device indices
    // grouped by key binding, sorted by key; devices with a key below 0 have no button (phase 4, step 3a).
    public sealed class CoreShipControls : IShipControlDescription
    {
        public CoreShipControls(CoreShipMapping mapping)
        {
            _mapping = mapping;

            var keys = new SortedDictionary<int, List<int>>();
            var droneBays = new List<int>();
            var devices = mapping.Definition.Devices;
            for (var i = 0; i < devices.Count; ++i)
            {
                if (devices[i] is DroneBayDefinition)
                    droneBays.Add(i);

                var key = mapping.Devices[i].KeyBinding;
                if (key < 0)
                    continue;

                if (!keys.TryGetValue(key, out var group))
                    keys.Add(key, group = new List<int>());
                group.Add(i);
            }

            KeyBindings = keys.Values.Select(group => (IReadOnlyList<int>)group).ToList();
            DroneBays = droneBays;
        }

        public IReadOnlyList<IReadOnlyList<int>> KeyBindings { get; }
        public IReadOnlyList<int> DroneBays { get; }

        public SpriteId GetButtonIcon(int system) => _mapping.Devices[system].Icon;
        public bool ReleaseImmediately(int keyGroup) => false;

        // The snapshot has no device cooldowns yet (step 3b).
        public float GetCooldown(int system) => 0f;

        private readonly CoreShipMapping _mapping;
    }
}
