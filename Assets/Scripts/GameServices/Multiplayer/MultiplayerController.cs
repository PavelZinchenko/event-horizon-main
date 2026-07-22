using Combat.Ai;
using Combat.Component.Ship;
using Combat.Unit;

namespace GameServices.Multiplayer
{
    /// <summary>Applies the remote player's abstract controls to host-side ships.</summary>
    public sealed class MultiplayerController : IController
    {
        public MultiplayerController(IShip ship, bool acceptInput)
        {
            _ship = ship;
            _acceptInput = acceptInput;
        }

        public ControllerStatus Status => _ship.IsActive() ? ControllerStatus.Active : ControllerStatus.Dead;

        public void Update(float deltaTime, in AiManager.Options options)
        {
            var session = MultiplayerSession.Instance;
            if (session == null) return;
            var input = _acceptInput ? session.LatestRemoteInput : null;
            if (input == null) return;
            Apply(_ship, input.throttle, input.hasCourse, input.course, input.systems);
        }

        internal static void Apply(IShip ship, float throttle, bool hasCourse, float course, string systems)
        {
            ship.Controls.Throttle = throttle;
            ship.Controls.Course = hasCourse ? course : (float?)null;
            var state = ship.Controls.Systems;
            var length = systems?.Length ?? 0;
            for (var i = 0; i < System.Math.Max(state.Count, length); i++)
                state.SetState(i, i < length && systems[i] == '1');
        }

        public sealed class Factory : IControllerFactory
        {
            public Factory(bool acceptInput) => _acceptInput = acceptInput;
            public IController Create(IShip ship) => new MultiplayerController(ship, _acceptInput);
            private readonly bool _acceptInput;
        }

        private readonly IShip _ship;
        private readonly bool _acceptInput;
    }
}
