using System;
using System.Collections.Generic;
using CombatView.Effects;
using UnityEngine;

namespace CombatView
{
    // The legacy BulletPrefab shapes (phase 5b, step 5). Each has its prefab in Resources/CombatView/Bullets with the
    // legacy name.
    public enum BulletViewKind : byte
    {
        Projectile,
        Rocket,
        Laser,
        Lightning,
        EnergyBeam,
        PiercingLaser,
        Spark,
        Mine,
        Arc,
        BlackHole,
        Harpoon,
        CircularSaw,
    }

    // One database BulletPrefab record as plain data (phase 5b, step 5), so this module needs no database types. The
    // renderer makes one prefab template per look, as legacy PrefabCache.GetBulletPrefab made one per record.
    public sealed class BulletLook
    {
        public BulletLook(BulletViewKind kind, Sprite image, Color mainColor, ColorMode mainColorMode, Color secondColor,
            ColorMode secondColorMode, float size, float margins)
        {
            Kind = kind;
            Image = image;
            MainColor = mainColor;
            MainColorMode = mainColorMode;
            SecondColor = secondColor;
            SecondColorMode = secondColorMode;
            Size = size;
            Margins = margins;
        }

        public BulletViewKind Kind { get; }

        // Null: the prefab's own sprite; a beam draws the plain white texture.
        public Sprite Image { get; }
        public Color MainColor { get; }
        public ColorMode MainColorMode { get; }
        public Color SecondColor { get; }
        public ColorMode SecondColorMode { get; }
        public float Size { get; }
        public float Margins { get; }

        // The beam shapes: the renderer sets the view's Size to the ray length.
        public bool IsLine => Kind == BulletViewKind.Laser || Kind == BulletViewKind.Lightning ||
            Kind == BulletViewKind.EnergyBeam || Kind == BulletViewKind.PiercingLaser;
    }

    // How one bullet or beam definition is drawn (migration phase 4, step 5; phase 5b, steps 5 and 6). The adapter
    // builds it from the database. BodySize is the legacy root scale; Color is the legacy view colour (the weapon's
    // bullet colour), which each part of the prefab combines with its own colour by its colour mode. A null look draws
    // nothing (the legacy AreaOfEffect prefab is empty); the sound and effect triggers still play.
    public sealed class BulletVisual
    {
        public BulletVisual(BulletLook look, float bodySize, Color color, IReadOnlyList<BulletSfx> triggers = null,
            EffectRecord burst = null)
        {
            Look = look;
            BodySize = bodySize;
            Color = color;
            Triggers = triggers ?? Array.Empty<BulletSfx>();
            Burst = burst;
        }

        public BulletLook Look { get; }
        public float BodySize { get; }
        public Color Color { get; }
        public IReadOnlyList<BulletSfx> Triggers { get; }

        // Step 7c: the detonator's explosion record, played once at the first pose at half the BodySize (the area's
        // radius), or null. Its ship dies in the same tick without its own explosion.
        public EffectRecord Burst { get; }

        // Nothing to draw or play: the renderer makes no view.
        public bool IsEmpty => Look == null && Triggers.Count == 0 && Burst == null;
    }
}
