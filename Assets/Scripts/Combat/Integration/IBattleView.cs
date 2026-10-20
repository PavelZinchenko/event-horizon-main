namespace Combat.Integration
{
    public readonly struct CombatTimerState
    {
        public CombatTimerState(bool visible, int secondsLeft, bool alarm)
        {
            Visible = visible;
            SecondsLeft = secondsLeft;
            Alarm = alarm;
        }

        public bool Visible { get; }
        public int SecondsLeft { get; }
        public bool Alarm { get; }
    }

    // Battle-level values for the HUD, menu and selection UI.
    public interface IBattleView
    {
        EntityId ControlledShip { get; }
        EntityId TargetShip { get; }

        CombatTimerState Timer { get; }

        bool CanChangeShip { get; }
        bool CanCallReinforcement { get; }

        // True when the combat rules enable KillThemAllButton. The Unity Editor shows the button regardless.
        bool KillAllAllowedByRules { get; }
    }
}
