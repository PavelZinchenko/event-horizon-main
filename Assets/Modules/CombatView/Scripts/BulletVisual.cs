using UnityEngine;

namespace CombatView
{
    public enum BulletViewKind : byte
    {
        Sprite,
        Rocket,
        Line,
        Arc,
        Harpoon,
    }

    // The flash at each hit of a bullet: diameter in world units, duration in seconds.
    public readonly struct HitFlash
    {
        public HitFlash(float diameter, Color color, float duration)
        {
            Diameter = diameter;
            Color = color;
            Duration = duration;
        }

        public readonly float Diameter;
        public readonly Color Color;
        public readonly float Duration;
    }

    // How one bullet or beam is drawn (migration phase 4, step 5). The adapter builds it from the database, so this
    // module never reads it. Sizes are world units; the sprite is drawn at its bounds x SpriteScale.
    public sealed class BulletVisual
    {
        private BulletVisual(BulletViewKind kind, Sprite sprite, float spriteScale, float lineWidth, float lineBorder,
            Color color, Color trailColor, float trailWidth, float trailOffset, HitFlash flash)
        {
            Kind = kind;
            Sprite = sprite;
            SpriteScale = spriteScale;
            LineWidth = lineWidth;
            LineBorder = lineBorder;
            Color = color;
            TrailColor = trailColor;
            TrailWidth = trailWidth;
            TrailOffset = trailOffset;
            Flash = flash;
        }

        public static BulletVisual ForSprite(Sprite sprite, float spriteScale, Color color, HitFlash flash) =>
            new BulletVisual(BulletViewKind.Sprite, sprite, spriteScale, 0f, 0f, color, default, 0f, 0f, flash);

        // The trail starts TrailOffset behind the origin, TrailWidth wide, and narrows to half of it.
        public static BulletVisual ForRocket(Sprite sprite, float spriteScale, Color color, Color trailColor,
            float trailWidth, float trailOffset, HitFlash flash) =>
            new BulletVisual(BulletViewKind.Rocket, sprite, spriteScale, 0f, 0f, color, trailColor, trailWidth, trailOffset, flash);

        // A null sprite draws a plain line. Otherwise the texture's first and last thirds cover LineBorder at each
        // end, and its middle third stretches between them (legacy LaserView).
        public static BulletVisual ForLine(Sprite sprite, float lineWidth, float lineBorder, Color color, HitFlash flash) =>
            new BulletVisual(BulletViewKind.Line, sprite, 0f, lineWidth, lineBorder, color, default, 0f, 0f, flash);

        public static BulletVisual ForArc(Sprite sprite, float lineWidth, Color color, HitFlash flash) =>
            new BulletVisual(BulletViewKind.Arc, sprite, 0f, lineWidth, 0f, color, default, 0f, 0f, flash);

        public static BulletVisual ForHarpoon(Sprite sprite, float spriteScale, float ropeWidth, Color color, HitFlash flash) =>
            new BulletVisual(BulletViewKind.Harpoon, sprite, spriteScale, ropeWidth, 0f, color, default, 0f, 0f, flash);

        public BulletViewKind Kind { get; }

        // Null: the renderer's generic sprite.
        public Sprite Sprite { get; }
        public float SpriteScale { get; }
        public float LineWidth { get; }
        public float LineBorder { get; }
        public Color Color { get; }

        public Color TrailColor { get; }
        public float TrailWidth { get; }
        public float TrailOffset { get; }

        public HitFlash Flash { get; }
    }
}
