using System;
using System.IO;
using System.Runtime.CompilerServices;
using MuMuSDK.Core.Interfaces;

namespace MuMuSDK.Core.Extensions
{
    public static class LogExtensions
    {
        // Holds a thread-safe reference back to our centralized logging engine service
        private static ILogBroker? _logBrokerInstance;

        /// <summary>
        /// Global bootstrap hook to link your dependency-injected log broker engine down to static method blocks.
        /// </summary>
        public static void Initialize(ILogBroker broker)
        {
            _logBrokerInstance = broker;
        }

        /// <summary>
        /// Universal low-overhead extension method allowing fluent logging syntax across the platform.
        /// </summary>
        public static void Log(
            this ILogSource source,
            MuMuLogLevel level,
            string message,
            [CallerFilePath] string sourceFile = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            if (_logBrokerInstance == null) return;

            // Automatically resolve the absolute filename string out of the compiler's full path array trace
            string cleanFileName = !string.IsNullOrEmpty(sourceFile)
                ? Path.GetFileName(sourceFile)
                : "UnknownSource";

            // Dispatch directly onto our non-allocating ring buffer queue pipeline
            _logBrokerInstance.Log(level, cleanFileName, lineNumber, message);
        }
    }
}
