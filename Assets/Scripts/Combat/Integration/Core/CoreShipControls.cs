using System;
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
            _devices = new DeviceState[devices.Count];
        }

        public IReadOnlyList<IReadOnlyList<int>> KeyBindings { get; }
        public IReadOnlyList<int> DroneBays { get; }

        public SpriteId GetButtonIcon(int system) => _mapping.Devices[system].Icon;
        public bool ReleaseImmediately(int keyGroup) => false;

        public float GetCooldown(int system) => _devices[system].Cooldown;

        // The adapter copies the device states after every step; snapshot spans are valid only until the next step.
        public void Update(ReadOnlySpan<DeviceState> devices) => devices.CopyTo(_devices);

        private readonly CoreShipMapping _mapping;
        private readonly DeviceState[] _devices;
    }
}
