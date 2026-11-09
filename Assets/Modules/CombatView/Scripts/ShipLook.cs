using System;
using System.Collections.Generic;
using CombatView.Effects;
using UnityEngine;

namespace CombatView
{
    // One engine of a ship model (legacy ShipFactory.CreateEngineEffect): the position in the ship's local space and
    // the database size.
    public readonly struct EngineLook
    {
        public EngineLook(Vector2 position, float size)
        {
            Position = position;
            Size = size;
        }

        public readonly Vector2 Position;
        public readonly float Size;
    }

    // How one ship, drone or decoy is drawn (migration phase 5b, step 4). The adapter builds it from the database, so
    // this module never reads it; one look serves every ship of one definition.
    public sealed class ShipLook
    {
        public ShipLook(Sprite sprite, float scale)
        {
            Sprite = sprite;
            Scale = scale;
        }

        public Sprite Sprite { get; }

        // Legacy rule: the ship's transform scale is its model scale.
        public float Scale { get; }

        // Legacy Features colour: the ship's ColorScheme colour, or a decoy's device colour.
        public Color Color { get; set; } = Color.white;

        // The colour ShipState.Tint moves the ship to; null without one.
        public Color? Tint { get; set; }

        // Legacy ShipView.ApplyHsv with the ship's ColorScheme. Turrets take their own scheme in AddTurret.
        public bool IsHsv { get; set; }
        public float Hue { get; set; }
        public float Saturation { get; set; }

        // Drones and clones: the short trail and no damage text (legacy ShipFactory, motherShip != null).
        public bool IsDrone { get; set; }

        public Color EngineColor { get; set; } = Color.white;
        public IReadOnlyList<EngineLook> Engines { get; set; } = Array.Empty<EngineLook>();

        // Legacy ShipFactory.CreateShield: the outline in the engine colour, for a ship with shield points.
        public bool HasShield { get; set; }

        // Legacy CreateDestructionEffect. Without a record the view draws the legacy obsolete explosion.
        public EffectRecord Explosion { get; set; }
        public SoundClip ExplosionSound { get; set; }
        public bool LeavesWreck { get; set; }

        // Legacy rule: a model scale below 0.9 is a small ship (drone explosion, no wreck by default).
        public bool IsSmall => Scale < 0.9f;
    }
}
