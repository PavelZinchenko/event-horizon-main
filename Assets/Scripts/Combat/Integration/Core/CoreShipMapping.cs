using System.Collections.Generic;
using CombatSimulation.Core;
using GameDatabase.Model;

namespace Combat.Integration.Core
{
    // The facts about one mapped ship that stay outside the core: legacy key bindings, button icons, activation costs
    // and the inert devices (phase 4, step 3a). Every list is in device order.
    public sealed class CoreShipMapping
    {
        public CoreShipMapping(string name, ShipDefinition definition, IReadOnlyList<CoreDeviceInfo> devices)
        {
            Name = name;
            Definition = definition;
            Devices = devices;
        }

        // For logs and reports: "ship" or "drone" and the legacy ship ID.
        public string Name { get; }

        public ShipDefinition Definition { get; }
        public IReadOnlyList<CoreDeviceInfo> Devices { get; }
    }

    public readonly struct CoreDeviceInfo
    {
        public CoreDeviceInfo(string name, int keyBinding, SpriteId icon, float activationCost, string inertReason)
        {
            Name = name;
            KeyBinding = keyBinding;
            Icon = icon;
            ActivationCost = activationCost;
            InertReason = inertReason;
        }

        public readonly string Name;

        // Legacy ActivationType.ValidateKey result. Below 0: the pilot uses the device by the energy rule
        // (CoreAutoActivation); otherwise the key group of the HUD and the keyboard.
        public readonly int KeyBinding;

        public readonly SpriteId Icon;

        // Legacy ISystem.ActivationCost, for the automatic use rule.
        public readonly float ActivationCost;

        // Not null for an inert device (D1).
        public readonly string InertReason;
        public bool IsInert => InertReason != null;
    }
}
