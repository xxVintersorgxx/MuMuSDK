using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using MuMuSDK.Core.Interfaces;

namespace MuMuSDK.Core.Services
{
    public class MemoryMapper : IMemoryMapper
    {
        // Win32 Allocation Filtering Constants
        private const uint MEM_COMMIT = 0x1000;
        private const uint MEM_PRIVATE = 0x2000;
        private const uint PAGE_READWRITE = 0x04;
        private const uint PROCESS_QUERY_INFORMATION = 0x0400;
        private const uint PROCESS_VM_READ = 0x0010;

        // Target Allocation Bound Sizes (in bytes)
        private const ulong LowerTargetSize = 1677721600;  // Exactly 1.5625 GB
        private const ulong UpperTargetSize = 3690987520;  // Exactly 3.4375 GB

        public IntPtr LowerBaseHva { get; private set; } = IntPtr.Zero;
        public IntPtr UpperBaseHva { get; private set; } = IntPtr.Zero;

        [StructLayout(LayoutKind.Sequential)]
        private struct MEMORY_BASIC_INFORMATION
        {
            public IntPtr BaseAddress;
            public IntPtr AllocationBase;
            public uint AllocationProtect;
            public ushort PartitionId;
            public IntPtr RegionSize;
            public uint State;
            public uint Protect;
            public uint Type;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenProcess(uint processAccess, bool bInheritHandle, int processId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr VirtualQueryEx(IntPtr hProcess, IntPtr lpAddress, out MEMORY_BASIC_INFORMATION lpBuffer, IntPtr dwLength);

        /// <summary>
        /// Dynamically targets the emulator process and performs an un-opinionated linear allocation sweep
        /// to capture invariant Host Virtual Address (HVA) base anchors cleanly.
        /// </summary>
        public bool InitializeMapping(string targetProcessName = "MuMuNxDevice")
        {
            Process[] processes = Process.GetProcessesByName(targetProcessName);
            if (processes.Length == 0) return false;

            Process targetProcess = processes[0];
            IntPtr processHandle = OpenProcess(PROCESS_QUERY_INFORMATION | PROCESS_VM_READ, false, targetProcess.Id);
            if (processHandle == IntPtr.Zero) return false;

            IntPtr address = IntPtr.Zero;
            IntPtr structSize = new IntPtr(Marshal.SizeOf<MEMORY_BASIC_INFORMATION>());

            // Linear sweep across active user-mode address allocation boundaries
            while (VirtualQueryEx(processHandle, address, out MEMORY_BASIC_INFORMATION mbi, structSize) != IntPtr.Zero)
            {
                ulong size = (ulong)mbi.RegionSize.ToInt64();

                // Apply multi-tool validation constraints: MEM_COMMIT + PAGE_READWRITE + MEM_PRIVATE
                if (mbi.State == MEM_COMMIT && mbi.Protect == PAGE_READWRITE && mbi.Type == MEM_PRIVATE)
                {
                    if (size == LowerTargetSize)
                    {
                        LowerBaseHva = mbi.BaseAddress;
                    }
                    else if (size == UpperTargetSize)
                    {
                        UpperBaseHva = mbi.BaseAddress;
                    }
                }

                // Move calculation handle forward linearly to bypass redundant overlapping overhead
                long nextAddress = mbi.BaseAddress.ToInt64() + mbi.RegionSize.ToInt64();
                if (nextAddress <= address.ToInt64()) break; // Prevent arithmetic overflow wrapping

                address = new IntPtr(nextAddress);
            }

            CloseHandle(processHandle);

            // Verify both anchoring coordinates resolved perfectly
            return LowerBaseHva != IntPtr.Zero && UpperBaseHva != IntPtr.Zero;
        }
    }
}
