using System;

namespace MuMuSDK.Core.Interfaces
{
    public interface ITickerEngine
    {
        /// <summary>
        /// Gets whether the master background timing loop is actively spinning.
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// Fires thread-safely on every single timing clock pulse interval.
        /// </summary>
        event Action? OnTickPulse;

        /// <summary>
        /// Configures the core clock cadence sleep interval parameters on the fly.
        /// </summary>
        /// <param name="intervalMs">Target loop duration threshold (50ms - 100ms).</param>
        void UpdateInterval(int intervalMs);

        /// <summary>
        /// Arms and spins up the high-precision background timing worker task loop.
        /// </summary>
        void Start();

        /// <summary>
        /// Safely pauses the timing loop execution track.
        /// </summary>
        void Stop();
    }
}
