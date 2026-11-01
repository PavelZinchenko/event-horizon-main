using System.Collections.Generic;
using CombatSimulation.Ai;
using CombatSimulation.Core;
using GameDatabase.DataModel;
using GameDatabase.Model;

namespace Combat.Integration.Core
{
    // The facts about one mapped ship that stay outside the core: legacy key bindings, button icons, activation costs
    // and the inert devices (phase 4, step 3a), the model the views draw (step 5), the AI side table (step 6) and the
    // pilot choice (step 7f). Every list is in device order.
    public sealed class CoreShipMapping
    {
        public CoreShipMapping(string name, ShipDefinition definition, IReadOnlyList<CoreDeviceInfo> devices,
            SpriteId modelImage, float modelScale, AiShipProfile ai, BehaviorTreeModel aiTree, float droneRange, bool hasAutopilot)
        {
            Name = name;
            Definition = definition;
            Devices = devices;
            ModelImage = modelImage;
            ModelScale = modelScale;
            Ai = ai;
            AiTree = aiTree;
            DroneRange = droneRange;
            HasAutopilot = hasAutopilot;
        }

        // For logs and reports: "ship" or "drone" and the legacy ship ID.
        public string Name { get; }

        public ShipDefinition Definition { get; }
        public IReadOnlyList<CoreDeviceInfo> Devices { get; }

        public SpriteId ModelImage { get; }
        public float ModelScale { get; }

        public AiShipProfile Ai { get; }

        // A ship: its own tree (CustomAi), null for the default of its kind. A drone: the tree its bay selects (T6), null
        // for none.
        public BehaviorTreeModel AiTree { get; }

        // A drone: its bay range (legacy AiSettings.ForDrone). 0 for a ship.
        public float DroneRange { get; }

        // Legacy Stats.Autopilot: a player ship also runs the autopilot tree.
        public bool HasAutopilot { get; }
    }

    // What a bullet or beam view needs from the database (phase 4, step 5, "Views (5)"). Prefab is null when the
    // ammunition has no BulletPrefab record. ParentColor is legacy BulletStats.Color: the weapon's colour, else the body's.
    // HitEffect is the first PlaySfx trigger on Hit with a visual effect, or null.
    public readonly struct CoreBulletSource
    {
        public CoreBulletSource(BulletPrefab prefab, float bodySize, ColorData parentColor, BulletTrigger_PlaySfx hitEffect)
        {
            Prefab = prefab;
            BodySize = bodySize;
            ParentColor = parentColor;
            HitEffect = hitEffect;
        }

        public readonly BulletPrefab Prefab;
        public readonly float BodySize;
        public readonly ColorData ParentColor;
        public readonly BulletTrigger_PlaySfx HitEffect;
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
