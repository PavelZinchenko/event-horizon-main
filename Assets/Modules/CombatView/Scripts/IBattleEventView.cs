using CombatSimulation.Core;

namespace CombatView
{
    // A view group that reacts to the tick's events (migration phase 5b, step 2). BattleRenderer.OnEvent passes it
    // every event of the tick in order, after the adapter has handled it, with the snapshot of the same tick.
    internal interface IBattleEventView
    {
        void OnEvent(BattleSnapshot snapshot, in BattleEvent item);
    }
}
