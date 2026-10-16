using System;

namespace MuMuSDK.Core.Interfaces
{
    public enum MuMuLogLevel
    {
        Trace,
        Info,
        Warning,
        Error,
        Fatal
    }

    public interface ILogBroker
    {
        /// <summary>
        /// Gets or sets the master global logging threshold limit.
        /// </summary>
        MuMuLogLevel GlobalLevel { get; set; }

        /// <summary>
        /// Fires thread-safely whenever a new formatted log line is flushed out of the background task processor queue.
        /// </summary>
        event Action<string>? OnLogLineFlushed;

        /// <summary>
        /// Toggles tracking masks dynamically to silence or isolate specific library namespaces.
        /// </summary>
        void SetNamespaceMuted(string namespaceKey, bool isMuted);

        /// <summary>
        /// Low-overhead non-allocating structure signature to accept fast processing messages.
        /// </summary>
        void Log(MuMuLogLevel level, string sourceFile, int lineNumber, string message);
    }
}
