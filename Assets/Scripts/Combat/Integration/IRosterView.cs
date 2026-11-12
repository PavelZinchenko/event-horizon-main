using System;
using System.Collections.Generic;
using Combat.Domain;

namespace Combat.Integration
{
    // Read-only projection of one fleet slot. Consumers never write through it.
    public interface IRosterSlotView
    {
        RosterId Id { get; }
        CombatSide Side { get; }
        Constructor.Ships.IShip ShipData { get; }
        ShipStatus Status { get; }
        float Condition { get; }

        // 0 until the first deployment; a higher value means a later deployment.
        int DeploymentOrder { get; }

        // EntityId.None unless the slot is Active.
        EntityId ActiveEntity { get; }
    }

    public enum RosterChangeReason
    {
        Deployed,
        Retreated,
        Killed,
        // Surrender or kill-all destroyed the slot by command; a ready reserve has no entity.
        ForcedDestruction,
    }

    public readonly struct RosterSlotChange
    {
        public RosterSlotChange(IRosterSlotView slot, ShipStatus previousStatus, RosterChangeReason reason, EntityId entity)
        {
            Slot = slot;
            PreviousStatus = previousStatus;
            Reason = reason;
            Entity = entity;
        }

        public IRosterSlotView Slot { get; }
        public ShipStatus PreviousStatus { get; }
        public RosterChangeReason Reason { get; }

        // The entity that spawned or left. EntityId.None when no entity was involved.
        public EntityId Entity { get; }
    }

    public interface IRosterView
    {
        IReadOnlyList<IRosterSlotView> PlayerSlots { get; }
        IReadOnlyList<IRosterSlotView> EnemySlots { get; }

        bool TryGetSlot(RosterId id, out IRosterSlotView slot);
        int Count(CombatSide side, ShipStatus status);

        // Raised once per status change. Repeated destruction raises nothing.
        event Action<RosterSlotChange> SlotChanged;
    }
}
