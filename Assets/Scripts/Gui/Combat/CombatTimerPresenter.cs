using Combat.Integration;
using Zenject;

namespace Gui.Combat
{
    // Drives the timer panel from IBattleView. The panel cannot do this itself:
    // a closed window deactivates its object, so its Update stops running.
    class CombatTimerPresenter : ITickable
    {
        [Inject]
        public CombatTimerPresenter(IBattleView battle, TimerPanel panel)
        {
            _battle = battle;
            _panel = panel;
        }

        public void Tick()
        {
            var timer = _battle.Timer;
            _panel.Enabled = timer.Visible;
            if (timer.Visible)
                _panel.Time = timer.SecondsLeft;
        }

        private readonly IBattleView _battle;
        private readonly TimerPanel _panel;
    }
}
