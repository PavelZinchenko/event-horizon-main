using System.Collections.Generic;
using CombatSimulation.Ai;
using CombatSimulation.Core;
using Constructor.Model;
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
            SpriteId modelImage, float modelScale, AiShipProfile ai, BehaviorTreeModel aiTree, float droneRange, bool hasAutopilot,
            ColorData? tintColor = null, bool isClone = false, IReadOnlyList<CoreTurretSource> turrets = null,
            IReadOnlyList<CoreSatelliteSource> satellites = null, CoreShipLookSource look = null)
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
            TintColor = tintColor;
            IsClone = isClone;
            Turrets = turrets ?? System.Array.Empty<CoreTurretSource>();
            Satellites = satellites ?? System.Array.Empty<CoreSatelliteSource>();
            Look = look;
        }

        // For logs and reports: "ship", "drone" or "clone" and the legacy ship ID.
        public string Name { get; }

        public ShipDefinition Definition { get; }
        public IReadOnlyList<CoreDeviceInfo> Devices { get; }

        public SpriteId ModelImage { get; }
        public float ModelScale { get; }

        public AiShipProfile Ai { get; }

        // A ship: its own tree (CustomAi), null for the default of its kind. A drone: the tree its bay selects (T6), null
        // for none. A clone: its own tree, else the CombatSettings clone tree (T6).
        public BehaviorTreeModel AiTree { get; }

        // A drone: its bay range (legacy AiSettings.ForDrone). 0 for a ship.
        public float DroneRange { get; }

        // Legacy Stats.Autopilot: a player ship also runs the autopilot tree.
        public bool HasAutopilot { get; }

        // The colour ShipState.Tint moves the ship to: the first detonator's (phase 5, step 7b), ghost's or
        // fortification's (step 9d); null without one.
        public ColorData? TintColor { get; }

        // The clone a clonning center launches (phase 5, step 7c): a drone in the core that flies with the clone tree.
        public bool IsClone { get; }
        public IReadOnlyList<CoreTurretSource> Turrets { get; }

        // In ShipDefinition.Satellites order (phase 5, step 12c).
        public IReadOnlyList<CoreSatelliteSource> Satellites { get; }

        // What the ship view draws besides the model (phase 5b, step 4).
        public CoreShipLookSource Look { get; }
    }

    // Legacy ShipFactory.CreateShip's view parts (phase 5b, step 4): the colour schemes, the engines, the shield
    // outline, the explosion by the legacy rule (CreateDestructionEffect) and the wreck. Explosion is null for the
    // legacy obsolete explosion; its sound is then the hard-coded one.
    public sealed class CoreShipLookSource
    {
        public CoreShipLookSource(ColorScheme shipColor, ColorScheme turretColor, ColorData engineColor,
            IReadOnlyList<Engine> engines, bool hasShield, bool isDrone, VisualEffect explosion, AudioClipId explosionSound,
            bool leavesWreck)
        {
            ShipColor = shipColor;
            TurretColor = turretColor;
            EngineColor = engineColor;
            Engines = engines;
            HasShield = hasShield;
            IsDrone = isDrone;
            Explosion = explosion;
            ExplosionSound = explosionSound;
            LeavesWreck = leavesWreck;
        }

        public ColorScheme ShipColor { get; }
        public ColorScheme TurretColor { get; }
        public ColorData EngineColor { get; }
        public IReadOnlyList<Engine> Engines { get; }
        public bool HasShield { get; }

        // Drones and clones (legacy motherShip != null).
        public bool IsDrone { get; }
        public VisualEffect Explosion { get; }
        public AudioClipId ExplosionSound { get; }
        public bool LeavesWreck { get; }
    }

    // What a bullet or beam view needs from the database (phase 4, step 5, "Views (5)"). Prefab is null when the
    // ammunition has no BulletPrefab record. ParentColor is legacy BulletStats.Color: the weapon's colour, else the body's.
    // HitEffect is the first PlaySfx trigger on Hit with a visual effect, or null. Hidden marks a carrier without a
    // prefab and without effects: legacy draws nothing for it (phase 5, step 11a, finding 9).
    public readonly struct CoreBulletSource
    {
        public CoreBulletSource(BulletPrefab prefab, float bodySize, ColorData parentColor, BulletTrigger_PlaySfx hitEffect,
            bool hidden = false)
        {
            Prefab = prefab;
            BodySize = bodySize;
            ParentColor = parentColor;
            HitEffect = hitEffect;
            Hidden = hidden;
        }

        public readonly BulletPrefab Prefab;
        public readonly float BodySize;
        public readonly ColorData ParentColor;
        public readonly BulletTrigger_PlaySfx HitEffect;
        public readonly bool Hidden;
    }

    public readonly struct CoreTurretSource
    {
        public readonly int Barrel;
        public readonly SpriteId Image;
        public readonly float Size;
        public readonly ColorData Color;
        public CoreTurretSource(int barrel, SpriteId image, float size, ColorData color)
        { Barrel = barrel; Image = image; Size = size; Color = color; }
    }

    // Legacy SatelliteFactory.CreateEnergyShield and CreateFrontalShield (phase 5, step 10d). An empty Image draws the
    // built-in sprite at ResourcePath. Size is the drawn width; the offset is in the ship frame.
    public readonly struct CoreShieldLook
    {
        public readonly SpriteId Image;
        public readonly string ResourcePath;
        public readonly float Size, OffsetX, OffsetY;
        public readonly ColorData Color;
        public readonly float IdleOpacity;
        public CoreShieldLook(SpriteId image, string resourcePath, float size, float offsetX, float offsetY, ColorData color,
            float idleOpacity)
        {
            Image = image;
            ResourcePath = resourcePath;
            Size = size;
            OffsetX = offsetX;
            OffsetY = offsetY;
            Color = color;
            IdleOpacity = idleOpacity;
        }
    }

    // Legacy StaticEffect of a teleporter (phase 5, step 11b): one flash for 0.5 s at each end of the jump, Diameter
    // device Size x model scale. Legacy FlashEffect of a gravity generator (step 11c): one flash for Duration (device
    // Lifetime) at the wave centre, Diameter 2 x Range.
    public readonly struct CoreFlashLook
    {
        public readonly ColorData Color;
        public readonly float Diameter;
        public readonly float Duration;

        public CoreFlashLook(ColorData color, float diameter, float duration = 0f)
        {
            Color = color;
            Diameter = diameter;
            Duration = duration;
        }
    }

    // Legacy SatelliteFactory: the satellite sprite at its model scale.
    public readonly struct CoreSatelliteSource
    {
        public readonly SpriteId Image;
        public readonly float Scale;
        public CoreSatelliteSource(SpriteId image, float scale) { Image = image; Scale = scale; }
    }

    public readonly struct CoreDeviceInfo
    {
        public CoreDeviceInfo(string name, int keyBinding, SpriteId icon, float activationCost, string inertReason,
            bool releaseImmediately = false)
        {
            Name = name;
            KeyBinding = keyBinding;
            Icon = icon;
            ActivationCost = activationCost;
            InertReason = inertReason;
            ReleaseImmediately = releaseImmediately;
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

        // Legacy ShouReleaseButtonImmediately for this system alone (phase 5, step 11b): a button of systems that all
        // have it cannot be locked by a double tap.
        public readonly bool ReleaseImmediately;
        public bool IsInert => InertReason != null;
    }
}
