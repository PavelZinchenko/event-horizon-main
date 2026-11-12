using System;

namespace Combat.Integration
{
    // Application view of allegiance. The shared core uses side IDs; local player/enemy labels belong here.
    public enum CombatSide
    {
        Player,
        Enemy,
    }

    // One fleet slot for the whole battle, including retreat and redeployment. Default value means "none".
    public readonly struct RosterId : IEquatable<RosterId>
    {
        public RosterId(int value) => Value = value;

        public int Value { get; }
        public bool IsValid => Value > 0;

        public static readonly RosterId None = default;

        public bool Equals(RosterId other) => Value == other.Value;
        public override bool Equals(object obj) => obj is RosterId other && Equals(other);
        public override int GetHashCode() => Value;
        public override string ToString() => "Roster#" + Value;

        public static bool operator ==(RosterId a, RosterId b) => a.Value == b.Value;
        public static bool operator !=(RosterId a, RosterId b) => a.Value != b.Value;
    }

    // One spawned unit. Redeploying a roster slot creates a new entity ID. Default value means "none".
    // Never derived from Unity instance IDs, object references or physics indices.
    public readonly struct EntityId : IEquatable<EntityId>
    {
        public EntityId(int value) => Value = value;

        public int Value { get; }
        public bool IsValid => Value > 0;

        public static readonly EntityId None = default;

        public bool Equals(EntityId other) => Value == other.Value;
        public override bool Equals(object obj) => obj is EntityId other && Equals(other);
        public override int GetHashCode() => Value;
        public override string ToString() => "Entity#" + Value;

        public static bool operator ==(EntityId a, EntityId b) => a.Value == b.Value;
        public static bool operator !=(EntityId a, EntityId b) => a.Value != b.Value;
    }
}
