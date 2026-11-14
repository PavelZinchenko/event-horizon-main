using Combat.Integration.Core;
using CombatSimulation.Core;

namespace GameStateMachine.States
{
    public enum OnlineDuelOutcome { None, Victory, Defeat, Draw, NoResult }

    // The last duel's outcome, from the duel combat state to the lobby that reopens over the main menu (phase 7, step 7).
    public sealed class OnlineDuelResult
    {
        public bool HasResult => _outcome != OnlineDuelOutcome.None;

        public void Set(CoreDuelBattle duel)
        {
            var result = duel.Result;
            if (!result.IsCompleted)
                _outcome = OnlineDuelOutcome.NoResult;
            else if (result.Outcome == BattleOutcome.Draw)
                _outcome = OnlineDuelOutcome.Draw;
            else
                _outcome = result.WinningSide == duel.LocalSide ? OnlineDuelOutcome.Victory : OnlineDuelOutcome.Defeat;
        }

        public OnlineDuelOutcome Take()
        {
            var outcome = _outcome;
            _outcome = OnlineDuelOutcome.None;
            return outcome;
        }

        private OnlineDuelOutcome _outcome;
    }
}
