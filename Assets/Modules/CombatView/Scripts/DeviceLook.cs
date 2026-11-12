using CombatSimulation.Core;
using CombatView.Effects;
using UnityEngine;

namespace CombatView
{
    // What a device shows (migration phase 5b, step 7a). The view acts on the core's device states and events, not on
    // the device class, so a kind names only the legacy rule it follows.
    public enum DeviceViewKind : byte
    {
        None,

        // Legacy WeaponFactory: a shot sound and a flash at the barrel on each shot.
        Weapon,

        // Legacy DeviceFactory: the sound on activation (the device's Active goes on). Detonator, fortification,
        // stealth and both shields.
        Toggle,

        // Legacy AcceleratorDevice: the sound on activation and the effect at each engine while active.
        Accelerator,

        // Legacy GhostDevice: the sound and a flash on the ship on activation and on deactivation.
        Ghost,

        // Legacy InfinityStone: the sound and a flash on the ship on each use, also the automatic one at a lethal hit.
        InfinityStone,

        // Legacy GravityGenerator: the sound and the effect on the ship for the device lifetime, on the core's
        // GravityWave event.
        GravityGenerator,

        // Legacy TeleporterDevice: the sound and the effect at both ends of the jump, on the core's Teleport event.
        Teleporter,

        // Legacy WarpDrive: the sound at the start, the effect at the start and at the end of the warp.
        WarpDrive,

        // Legacy PointDefenseAction: the sound and a lightning zap at each disarm and each drone hit.
        PointDefense,

        // Legacy DroneBayFactory and ClonningDevice: the sound and the effect at each launch.
        DroneBay,
        ClonningCenter,
    }

    // How a weapon's sound and flash follow its shots. Single: legacy CommonCannon, MultishotCannon and MachineGun.
    // Held: legacy ManageableCannon and ContinuousCannon, whose sound stops when their bullet goes; a continuous weapon
    // also keeps the flash lit while its bullet lives. Charging: legacy ChargeableCannon, the charge sound and effect.
    public enum WeaponViewMode : byte { Single, Held, Continuous, Charging }

    // The view data of one device of a ship definition (phase 5b, step 7a). The adapter builds it from the database, as
    // it builds ShipLook. An effect is a record, or a named prefab of Resources/CombatView/Effects, or none.
    public sealed class DeviceLook
    {
        public DeviceLook(DeviceViewKind kind)
        {
            Kind = kind;
        }

        public DeviceViewKind Kind { get; }
        public SoundClip Sound { get; set; }
        public EffectRecord Record { get; set; }
        public string Prefab { get; set; }
        public bool HasEffect => Record != null || !string.IsNullOrEmpty(Prefab);

        // The effect colour and size in world units.
        public Color Color { get; set; } = Color.white;
        public float Size { get; set; } = 1f;

        // Seconds a flash takes to fade out: legacy FlashEffect and StaticEffect lifetimes.
        public float Lifetime { get; set; }

        // Weapons: the barrel the flash rides on, the mode and the charge sound (legacy WeaponStats.ChargeSound).
        public int Barrel { get; set; } = -1;
        public BarrelDefinition BarrelDefinition { get; set; }
        public WeaponViewMode Mode { get; set; }
        public SoundClip ChargeSound { get; set; }
    }
}
