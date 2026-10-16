using Microsoft.Win32.SafeHandles;
using MuMuSDK.Core.Extensions; // Import the extensions namespace hook
using MuMuSDK.Core.Interfaces;
using MuMuSDK.Core.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace MuMuSDK.Core.Services
{
    public sealed class Win32ProcessTracker : IProcessTracker, ILogSource, IDisposable
    {
        private const uint PROCESS_QUERY_INFORMATION = 0x0400;
        private const uint PROCESS_VM_READ = 0x0010;

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        private SafeProcessHandle? _targetHandle;
        private bool _disposed;

        public int ProcessId { get; private set; }
        public bool IsAttached => _targetHandle is not null && !_targetHandle.IsInvalid && !_targetHandle.IsClosed;
        public SafeProcessHandle? TargetHandle => _targetHandle;

        public bool TryAttach(string processIdentifier)
        {
            Detach();
            if (string.IsNullOrWhiteSpace(processIdentifier)) return false;

            try
            {
                int targetPid = 0;

                // If the parameter is a precise configuration-driven numeric string string, convert it instantly
                if (int.TryParse(processIdentifier, out int parsedPid))
                {
                    targetPid = parsedPid;
                }
                else
                {
                    // Fallback to name-based resolution loops if manually attaching to standard windows binaries
                    string cleanName = processIdentifier.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)
                        ? Path.GetFileNameWithoutExtension(processIdentifier)
                        : processIdentifier;

                    var processes = Process.GetProcessesByName(cleanName);
                    if (processes.Length > 0) targetPid = processes[0].Id;
                }

                if (targetPid == 0) return false;
                ProcessId = targetPid;

                // Open handle securely using the absolute, confirmed Process ID
                IntPtr rawHandle = NativeWin32.OpenProcess(
                    NativeWin32.PROCESS_QUERY_INFORMATION | NativeWin32.PROCESS_VM_READ,
                    false,
                    ProcessId);

                if (rawHandle == IntPtr.Zero) return false;

                _targetHandle = new SafeProcessHandle(rawHandle, ownsHandle: true);
                return true;
            }
            catch
            {
                ResetState();
                return false;
            }
        }

        public void Detach()
        {
            if (_targetHandle is not null && !_targetHandle.IsClosed)
            {
                _targetHandle.Dispose();
                this.Log(MuMuLogLevel.Info, $"Released unmanaged process context parameters for PID {ProcessId} cleanly.");
            }
            ResetState();
        }

        private void ResetState()
        {
            _targetHandle = null;
            ProcessId = 0;
        }

        public void Dispose()
        {
            if (_disposed) return;
            Detach();
            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}
