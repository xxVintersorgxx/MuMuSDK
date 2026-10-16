using System;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace MuMuSDK.Core.Win32
{
    public static class NativeWin32
    {
        // --- WIN32 ACCESS PERMISSION MASKS ---
        public const uint PROCESS_QUERY_INFORMATION = 0x0400;
        public const uint PROCESS_VM_READ = 0x0010;

        // --- MEMORY PAGE PROTECTIONS & CONSTANTS ---
        public const uint MEM_COMMIT = 0x1000;
        public const uint PAGE_READWRITE = 0x04;
        public const uint MEM_PRIVATE = 0x20000;
        public const long MAX_USER_ADDRESS_RANGE = 0x7FFFFFFFFFFF;

        [StructLayout(LayoutKind.Sequential)]
        public struct MEMORY_BASIC_INFORMATION
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

        // --- EXCLUSIVE BARE-METAL PLATFORM SIGNATURES ---

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr OpenProcess(
            uint dwDesiredAccess,
            bool bInheritHandle,
            int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr VirtualQueryEx(
            SafeProcessHandle hProcess,
            IntPtr lpAddress,
            out MEMORY_BASIC_INFORMATION lpBuffer,
            IntPtr dwLength);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ReadProcessMemory(
            SafeProcessHandle hProcess,
            IntPtr lpBaseAddress,
            [Out] byte[] lpBuffer,
            IntPtr nSize,
            out IntPtr lpNumberOfBytesRead);
    }
}
