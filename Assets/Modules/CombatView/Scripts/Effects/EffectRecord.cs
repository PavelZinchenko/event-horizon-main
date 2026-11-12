using System;
using System.Collections.Generic;
using UnityEngine;

namespace CombatView.Effects
{
    // The element types of a visual effect record (migration phase 5b, step 3): the database VisualEffectType in the
    // same order, plus Laser (owner question 10), which the database gets in step 7. The adapter converts the values.
    public enum EffectKind : byte
    {
        Flash,
        FlashAdditive,
        Shockwave,
        Smoke,
        SmokeAdditive,
        Shake,
        Spark,
        Lightning,
        LightningStrike,
        Sprite,
        Laser,
    }

    // The database ColorMode in the same order; prefabs serialize it as a number.
    public enum ColorMode
    {
        TakeFromOwner,
        UseMyOwn,
        Blend,
        Multiply,
    }

    public static class ColorModeExtensions
    {
        // Legacy ColorExtensions.Apply.
        public static Color Apply(this ColorMode mode, Color own, Color owner)
        {
            switch (mode)
            {
                case ColorMode.UseMyOwn: return own;
                case ColorMode.Blend: return Color.Lerp(owner, own, 0.5f);
                case ColorMode.Multiply: return owner * own;
                default: return owner;
            }
        }
    }

    // One element of a visual effect record: the database VisualEffectElement as plain data, so this module needs no
    // database types. The adapter resolves the image.
    public sealed class EffectElement
    {
        public EffectElement(EffectKind kind, Sprite image, ColorMode colorMode, Color color, int quantity, float size,
            float growthRate, float turnRate, float startTime, float lifetime, float particleSize, Vector2 offset,
            float rotation, bool loop, bool inverse, bool useRealTime)
        {
            Kind = kind;
            Image = image;
            ColorMode = colorMode;
            Color = color;
            Quantity = Math.Max(1, quantity);
            Size = size;
            GrowthRate = growthRate;
            TurnRate = turnRate;
            StartTime = startTime;
            Lifetime = lifetime;
            ParticleSize = particleSize;
            Offset = offset;
            Rotation = rotation;
            Loop = loop;
            Inverse = inverse;
            UseRealTime = useRealTime;
        }

        public readonly EffectKind Kind;
        public readonly Sprite Image;
        public readonly ColorMode ColorMode;
        public readonly Color Color;
        public readonly int Quantity;
        public readonly float Size;
        public readonly float GrowthRate;
        public readonly float TurnRate;
        public readonly float StartTime;
        public readonly float Lifetime;
        public readonly float ParticleSize;
        public readonly Vector2 Offset;
        public readonly float Rotation;
        public readonly bool Loop;
        public readonly bool Inverse;
        public readonly bool UseRealTime;
    }

    // A database VisualEffect record as plain data. CompositeEffect plays it.
    public sealed class EffectRecord
    {
        public EffectRecord(IReadOnlyList<EffectElement> elements)
        {
            Elements = elements;
            foreach (var element in elements)
            {
                Lifetime = Mathf.Max(Lifetime, element.StartTime + element.Lifetime);
                MaxElementLifetime = Mathf.Max(MaxElementLifetime, element.Lifetime);
            }
        }

        public IReadOnlyList<EffectElement> Elements { get; }

        // Legacy CompositeEffect._lifetimeMax: the end of the last element. Run with no lifetime uses it.
        public float Lifetime { get; }

        // The longest element lifetime, without start times: legacy ShipExplosionAction runs the explosion for it.
        public float MaxElementLifetime { get; }
    }
}
