using System;

namespace Combat.Component.Bullet.Tickers
{
    /// <summary>
    /// Represents an interface for a ticker object that controls timed activations.
    /// </summary>
    public interface ITicker : IDisposable
    {
        /// <summary>
        /// Gets the cooldown time in seconds between activations.
        /// </summary>
        /// <value>
        /// The cooldown duration as a float.
        /// </value>
        float Cooldown { get; }

        /// <summary>
        /// Gets or sets the moment in time (as a float) when the next activation should occur.
        /// </summary>
        /// <value>
        /// Time of the next activation.
        /// </value>
        float NextActivation { get; set; }

        /// <summary>
        /// Activates the ticker, performing its associated action immediately.
        /// </summary>
        void Activate();
    }
}