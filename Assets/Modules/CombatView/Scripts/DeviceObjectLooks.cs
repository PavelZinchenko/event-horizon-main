using System.Collections.Generic;
using UnityEngine;

namespace CombatView
{
    // The device object prefabs of Resources/CombatView/Objects (migration phase 5b, step 7b), with the legacy names.
    public enum ShieldViewKind : byte
    {
        // Legacy EnergyShield: a sprite (the database CircularSpriteObject record, or the prefab's).
        Energy,

        // Legacy EnergyShieldOutline: a ring mesh (the database CircularOutlineObject record).
        EnergyOutline,

        // Legacy FrontalShield: the prefab's sprite.
        Frontal,
    }

    // One energy or frontal shield of a ship definition as plain data. Size is the drawn width; the offset is in the
    // ship frame. A null image keeps the prefab's sprite. The idle colour is the colour with its alpha times IdleOpacity.
    public sealed class ShieldLook
    {
        public ShieldViewKind Kind { get; set; }
        public Sprite Image { get; set; }
        public float ImageScale { get; set; } = 1f;
        public float Thickness { get; set; } = 0.1f;
        public float AspectRatio { get; set; } = 1f;
        public Vector2 Offset { get; set; }
        public float Size { get; set; }
        public Color Color { get; set; } = Color.white;
        public float IdleOpacity { get; set; } = 1f;
    }

    // The segments of one worm tail: the WormSegment prefab set up from a database WormTailSegment record, the
    // WormSegment prefab as it is, or the WormSegment2 prefab (legacy DeviceFactory).
    public sealed class SegmentLook
    {
        public bool IsSegment2 { get; set; }

        // False: the prefab keeps its own sprites and joint.
        public bool HasRecord { get; set; }
        public Sprite Body { get; set; }
        public Sprite Joint { get; set; }
        public float JointScale { get; set; }
        public float JointOffset { get; set; }
    }

    // The repair bots of one device: the beam colour and texture, and the device sound.
    public sealed class RepairBotLook
    {
        public RepairBotLook(Color color, Sprite laserImage, SoundClip sound)
        {
            Color = color;
            LaserImage = laserImage;
            Sound = sound;
        }

        public Color Color { get; }
        public Sprite LaserImage { get; }
        public SoundClip Sound { get; }
    }

    // Named effect prefabs that an area plays once at its first pose (step 7b: the legacy detonator explosion until its
    // database record in step 7c). Each runs at SizeRatio x the bullet's BodySize in the bullet's colour.
    public sealed class AreaBurst
    {
        public AreaBurst(float lifetime, params (string Prefab, float SizeRatio)[] effects)
        {
            Lifetime = lifetime;
            Effects = effects;
        }

        public float Lifetime { get; }
        public IReadOnlyList<(string Prefab, float SizeRatio)> Effects { get; }
    }
}
