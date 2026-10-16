using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MuMuSDK.Core.Interfaces;

namespace MuMuSDK.Core.Services
{
    public sealed class AsyncLogBroker : ILogBroker, IDisposable
    {
        private struct LogPayloadToken
        {
            public MuMuLogLevel Level;
            public string SourceFile;
            public int LineNumber;
            public string Message;
            public DateTime Timestamp;
        }

        private readonly ConcurrentQueue<LogPayloadToken> _logQueue = new();
        private readonly HashSet<string> _mutedNamespaces = new();
        private readonly CancellationTokenSource _cts = new();
        private readonly Task _consumerTask;
        private readonly object _lockObj = new();

        public MuMuLogLevel GlobalLevel { get; set; } = MuMuLogLevel.Info;
        public event Action<string>? OnLogLineFlushed;

        public AsyncLogBroker()
        {
            // Spin up a dedicated long-running background worker thread loop to process text formatting
            _consumerTask = Task.Factory.StartNew(
                ProcessLogQueueAsync,
                _cts.Token,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default
            );
        }

        public void SetNamespaceMuted(string namespaceKey, bool isMuted)
        {
            lock (_lockObj)
            {
                if (isMuted) _mutedNamespaces.Add(namespaceKey);
                else _mutedNamespaces.Remove(namespaceKey);
            }
        }

        public void Log(MuMuLogLevel level, string sourceFile, int lineNumber, string message)
        {
            // Boundary Guard A: Immediate short-circuit evaluation by severity rank
            if (level < GlobalLevel) return;

            // Boundary Guard B: Immediate short-circuit check by structural namespace muting constraints
            string namespaceName = Path.GetFileNameWithoutExtension(sourceFile);
            lock (_lockObj)
            {
                if (_mutedNamespaces.Contains(namespaceName)) return;
            }

            // Enqueue the compact struct token onto the lock-free queue with zero heap allocations at call-site
            _logQueue.Enqueue(new LogPayloadToken
            {
                Level = level,
                SourceFile = sourceFile,
                LineNumber = lineNumber,
                Message = message,
                Timestamp = DateTime.Now
            });
        }

        private async Task ProcessLogQueueAsync()
        {
            var token = _cts.Token;
            while (!token.IsCancellationRequested || !_logQueue.IsEmpty)
            {
                try
                {
                    if (_logQueue.TryDequeue(out var logItem))
                    {
                        // Single centralized heap serialization pass happens safely off the core execution loops
                        string formattedLine = $"[{logItem.Timestamp:HH:mm:ss}][{logItem.Level,-7}][{logItem.SourceFile}:{logItem.LineNumber}] {logItem.Message}";

                        // Fire event notification safely to anyone listening (such as our Avalonia UI MainViewModel)
                        OnLogLineFlushed?.Invoke(formattedLine);
                    }
                    else
                    {
                        // Yield processing thread resource slots cleanly when the queue pool is empty
                        await Task.Delay(15, token);
                    }
                }
                catch (OperationCanceledException) when (token.IsCancellationRequested)
                {
                    break;
                }
                catch
                {
                    // Suppressed for pipeline processing isolation stability
                }
            }
        }

        public void Dispose()
        {
            _cts.Cancel();
            try
            {
                _consumerTask.Wait(500);
            }
            catch { /* Shutdown timeout fallback */ }
            _cts.Dispose();
        }
    }
}
