using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using MuMuSDK.Core.Extensions;
using MuMuSDK.Core.Interfaces;

namespace MuMuSDK.Core.Services
{
    public sealed class BackgroundTickerEngine : ITickerEngine, ILogSource, IDisposable
    {
        private readonly object _lockObj = new();
        private CancellationTokenSource? _cts;
        private Task? _loopTask;
        private int _intervalMs = 100;
        private bool _disposed;

        public bool IsRunning { get; private set; }
        public event Action? OnTickPulse;

        public void UpdateInterval(int intervalMs)
        {
            lock (_lockObj)
            {
                _intervalMs = Math.Clamp(intervalMs, 10, 1000);
                this.Log(MuMuLogLevel.Trace, $"Master clock cadence modified to: {_intervalMs}ms");
            }
        }

        public void Start()
        {
            lock (_lockObj)
            {
                if (IsRunning) return;

                IsRunning = true;
                _cts = new CancellationTokenSource();

                _loopTask = Task.Factory.StartNew(
                    () => RunTickerLoopAsync(_cts.Token),
                    _cts.Token,
                    TaskCreationOptions.LongRunning,
                    TaskScheduler.Default
                );

                this.Log(MuMuLogLevel.Info, "Master timing ticker engine armed and spinning successfully.");
            }
        }

        public void Stop()
        {
            lock (_lockObj)
            {
                if (!IsRunning) return;

                IsRunning = false;
                _cts?.Cancel();

                try
                {
                    _loopTask?.Wait(200);
                }
                catch { /* Suppressed thread abort exceptions */ }

                _cts?.Dispose();
                _cts = null;
                _loopTask = null;

                this.Log(MuMuLogLevel.Info, "Master timing ticker engine disarmed gracefully.");
            }
        }

        private async Task RunTickerLoopAsync(CancellationToken token)
        {
            var stopwatch = new Stopwatch();

            while (!token.IsCancellationRequested)
            {
                stopwatch.Restart();
                int currentInterval;

                lock (_lockObj)
                {
                    currentInterval = _intervalMs;
                }

                try
                {
                    // Dispatch the unified tick event cleanly to all downstream listeners
                    OnTickPulse?.Invoke();
                }
                catch (Exception ex)
                {
                    this.Log(MuMuLogLevel.Error, $"Exception caught inside core frame loop execution track: {ex.Message}");
                }

                stopwatch.Stop();
                long elapsedMs = stopwatch.ElapsedMilliseconds;

                // Delta-time compensation calculus prevents drift accumulations
                long sleepDelay = currentInterval - elapsedMs;

                if (sleepDelay > 0)
                {
                    try
                    {
                        await Task.Delay((int)sleepDelay, token);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                }
                else
                {
                    this.Log(MuMuLogLevel.Warning, $"Introspection framework processing load exceeded frame threshold. Frame budget overran by: {Math.Abs(sleepDelay)}ms");
                }
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            Stop();
            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}
