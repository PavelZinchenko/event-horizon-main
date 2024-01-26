namespace Combat.Component.Bullet.Tickers
{
    public static class ITickerExtensions
    {
        /// <summary>
        /// Extension method for updating an ITicker instance. This method checks if the ticker is ready for activation
        /// based on the given time and, if so, activates it.
        /// </summary>
        /// <param name="ticker">The ITicker instance to be updated.</param>
        /// <param name="time">The current time.</param>
        /// <remarks>
        /// This method should be called with a fixed time value (e.g., <c>Time.fixedTime</c>) to ensure consistent
        /// behavior.
        /// </remarks>
        public static void Update(this ITicker ticker, float time)
        {
            // If the next activation time is not yet reached, return without activating.
            if (ticker.NextActivation < time) return;

            // Activate the ticker and set the next activation time.
            ticker.Activate();
            ticker.NextActivation = time + ticker.Cooldown;
        }

    }
}