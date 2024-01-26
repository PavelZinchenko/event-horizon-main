using Combat.Component.Bullet.Tickers;

namespace Combat.Component.Bullet.Action
{
    /// <summary>
    /// A ticker that triggers a specified <see cref="IAction"/>.
    /// </summary>
    public class ActionTicker : ITicker
    {
        /// <param name="cooldown">The cooldown time in seconds between activations.</param>
        /// <param name="time">The current time.</param>
        /// <param name="action">The action to be performed on activation.</param>
        /// <remarks>
        /// This constructor should be called with a fixed time value (e.g., <c>Time.fixedTime</c>) to ensure consistent
        /// behavior.
        /// </remarks>
        public ActionTicker(float cooldown, float time, IAction action)
        {
            Cooldown = cooldown;
            NextActivation = time + cooldown;
        }

        public void Dispose()
        {
            _action.Dispose();
        }

        public float Cooldown { get; }
        public float NextActivation { get; set; }
        public void Activate()
        {
            _action.Invoke(); // Collision effect is discarded, same as in `Created` condition
        }
        
        private readonly IAction _action;
    }
}