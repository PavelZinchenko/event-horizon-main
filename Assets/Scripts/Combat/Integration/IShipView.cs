using GameDatabase.Enums;
using GameDatabase.Model;
using UnityEngine;

namespace Combat.Integration
{
    public readonly struct ResourceValue
    {
        public ResourceValue(bool exists, float value, float maxValue, float fraction)
        {
            Exists = exists;
            Value = value;
            MaxValue = maxValue;
            Fraction = fraction;
        }

        public bool Exists { get; }
        public float Value { get; }
        public float MaxValue { get; }

        // Value relative to MaxValue, 0 to 1.
        public float Fraction { get; }
    }

    public readonly struct ShipResistance
    {
        public ShipResistance(float heat, float energy, float kinetic)
        {
            Heat = heat;
            Energy = energy;
            Kinetic = kinetic;
        }

        public float Heat { get; }
        public float Energy { get; }
        public float Kinetic { get; }
    }

    // Read-only view of one spawned ship. Consumers never write through it.
    public interface IShipView
    {
        // EntityId.None in scenes without a combat adapter (exploration).
        EntityId Id { get; }
        CombatSide Side { get; }
        bool IsActive { get; }

        ResourceValue Armor { get; }
        ResourceValue Shield { get; }
        ResourceValue Energy { get; }
        ShipResistance Resistance { get; }

        // Display metadata.
        SpriteId IconImage { get; }
        SpriteId ModelImage { get; }
        SizeClass SizeClass { get; }
        DifficultyClass Class { get; }
        int Level { get; }

        // Presentation pose: interpolated, in Unity units and degrees. Input must not use it.
        Vector2 VisualPosition { get; }
        float VisualRotation { get; }
    }

    public interface IShipViews
    {
        // Finds a ship by an entity ID that this battle has published.
        bool TryGetShip(EntityId id, out IShipView ship);
    }
}
