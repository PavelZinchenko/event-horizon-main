using System.Collections.Generic;
using CombatSimulation.Core;
using GameDatabase.Model;

namespace Combat.Integration.Core
{
    // Describes the controllable systems of one core ship (migration phase 3c, step 5).
    // The core has no key bindings, so each device gets its own key, in device order.
    public sealed class CoreShipControls : IShipControlDescription
    {
        public CoreShipControls(ShipDefinition definition)
        {
            var keyBindings = new List<IReadOnlyList<int>>();
            var droneBays = new List<int>();
            var devices = definition.Devices;
            for (var i = 0; i < devices.Count; ++i)
            {
                keyBindings.Add(new[] { i });
                if (devices[i] is DroneBayDefinition)
                    droneBays.Add(i);
            }

            KeyBindings = keyBindings;
            DroneBays = droneBays;
        }

        public IReadOnlyList<IReadOnlyList<int>> KeyBindings { get; }
        public IReadOnlyList<int> DroneBays { get; }

        // The fixture loadout has only cannons; real device icons come with phases 4-5.
        public SpriteId GetButtonIcon(int system) => ButtonIcon;
        public bool ReleaseImmediately(int keyGroup) => false;

        // The snapshot has no device cooldowns yet.
        public float GetCooldown(int system) => 0f;

        private static readonly SpriteId ButtonIcon = new SpriteId("controls_shot", SpriteId.Type.ActionButton);
    }
}
