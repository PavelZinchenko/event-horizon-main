using CombatView.Effects;
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

        // Legacy FrontalShield: a sprite (step 7c: the database ColliderSpriteObject record, or the prefab's).
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

    // The repair bots of one device: the device colour, the beam (step 7c: the first Laser element of the device's
    // record), and the device sound. The beam's Size multiplies the legacy length; a null beam draws none.
    public sealed class RepairBotLook
    {
        public RepairBotLook(Color color, EffectElement beam, SoundClip sound)
        {
            Color = color;
            Beam = beam;
            Sound = sound;
        }

        public Color Color { get; }
        public EffectElement Beam { get; }
        public SoundClip Sound { get; }
    }
}
