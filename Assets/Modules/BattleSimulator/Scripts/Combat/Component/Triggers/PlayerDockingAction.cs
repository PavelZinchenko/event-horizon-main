using Services.Messenger;

namespace Combat.Component.Triggers
{
    public class PlayerDockingAction : IUnitAction
    {
        public PlayerDockingAction(IMessenger messenger, int stationId)
        {
            _stationId = stationId;
            _messenger = messenger;
        }

        public ConditionType TriggerCondition => ConditionType.OnActivate;

        public bool TryUpdateAction(float elapsedTime)
        {
            _remainingTime -= elapsedTime;
            if (_remainingTime > 0)
                return true;

            _messenger.Broadcast(EventType.PlayerShipUndocked, _stationId);
            return false;
        }

        public bool TryInvokeAction(ConditionType condition)
        {
            if (_remainingTime <= 0)
                _messenger.Broadcast(EventType.PlayerShipDocked, _stationId);

            // Scanning an exploration point takes ten seconds.  Keeping the
            // docking session alive slightly longer makes a brief physics
            // contact interruption (common with large ship colliders) unable
            // to cancel an otherwise valid scan.
            _remainingTime = DockingSessionDuration;
            return true;
        }

        public void Dispose() { }

        private const float DockingSessionDuration = 12f;
        private float _remainingTime;
        private readonly int _stationId;
        private readonly IMessenger _messenger;
    }
}
