using System;
using Combat.Domain;
using Zenject;

namespace Combat.Integration
{
    // Feeds experience checkpoints to the campaign model from roster events.
    // A player ship that retreats or dies gets a checkpoint. Forced destruction
    // (surrender) gets none, as in the legacy rule. GetReward adds the final checkpoint.
    public sealed class CombatExperienceRecorder : IDisposable
    {
        [Inject]
        public CombatExperienceRecorder(IRosterView roster, ICombatModel combatModel)
        {
            _roster = roster;
            _combatModel = combatModel;
            _roster.SlotChanged += OnSlotChanged;
        }

        public void Dispose()
        {
            _roster.SlotChanged -= OnSlotChanged;
        }

        private void OnSlotChanged(RosterSlotChange change)
        {
            if (change.Slot.Side != CombatSide.Player)
                return;
            if (change.Reason != RosterChangeReason.Retreated && change.Reason != RosterChangeReason.Killed)
                return;

            _combatModel.AddExperienceCheckpoint(change.Slot.ShipData);
        }

        private readonly IRosterView _roster;
        private readonly ICombatModel _combatModel;
    }
}
