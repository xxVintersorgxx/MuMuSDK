using System;
using System.Runtime.InteropServices;
using MuMuSDK.Core.Extensions;
using MuMuSDK.Core.Interfaces;
using MuMuSDK.Core.Win32;

namespace MuMuSDK.Core.Services
{
    public sealed class Win32MemoryMapper : IMemoryMapper, IMemoryReader, ILogSource
    {
        private readonly IProcessTracker _processTracker;

        public IntPtr LowerBaseHva { get; private set; } = IntPtr.Zero;
        public IntPtr UpperBaseHva { get; private set; } = IntPtr.Zero;
        public bool IsMapped { get; private set; }

        public Win32MemoryMapper(IProcessTracker processTracker)
        {
            _processTracker = processTracker;
        }

        // --- IMemoryMapper Implementation (Linear Sweep Loops) ---
        public bool InitializeMapping(long lowerSizeTarget, long upperSizeTarget)
        {
            Reset();

            if (!_processTracker.IsAttached || _processTracker.TargetHandle == null)
            {
                this.Log(MuMuLogLevel.Error, "Unable to execute virtual memory scan. Active target process handle is null or unattached.");
                return false;
            }

            this.Log(MuMuLogLevel.Info, $"Commencing virtual sweep targeting dynamic pools. Bounds: Lower={lowerSizeTarget} bytes, Upper={upperSizeTarget} bytes");

            IntPtr currentAddress = IntPtr.Zero;
            int infoStructSize = Marshal.SizeOf<NativeWin32.MEMORY_BASIC_INFORMATION>();

            try
            {
                while ((long)currentAddress < NativeWin32.MAX_USER_ADDRESS_RANGE)
                {
                    var queryResult = NativeWin32.VirtualQueryEx(
                        _processTracker.TargetHandle,
                        currentAddress,
                        out NativeWin32.MEMORY_BASIC_INFORMATION memInfo,
                        (IntPtr)infoStructSize);

                    if (queryResult == IntPtr.Zero) break;

                    long pageSize = (long)memInfo.RegionSize;

                    if (memInfo.State == NativeWin32.MEM_COMMIT &&
                        memInfo.Protect == NativeWin32.PAGE_READWRITE &&
                        memInfo.Type == NativeWin32.MEM_PRIVATE)
                    {
                        // Match page dimensions dynamically using values extracted straight out of the nemu data model!
                        if (pageSize == lowerSizeTarget)
                        {
                            LowerBaseHva = memInfo.BaseAddress;
                            this.Log(MuMuLogLevel.Info, $"[Phase 1] Successfully isolated Lower Base HVA anchor coordinate: 0x{LowerBaseHva.ToString("X")}");
                        }
                        else if (pageSize == upperSizeTarget)
                        {
                            UpperBaseHva = memInfo.BaseAddress;
                            this.Log(MuMuLogLevel.Info, $"[Phase 1] Successfully isolated Upper Base HVA anchor coordinate: 0x{UpperBaseHva.ToString("X")}");
                        }
                    }

                    currentAddress = (IntPtr)((long)memInfo.BaseAddress + pageSize);

                    if (LowerBaseHva != IntPtr.Zero && UpperBaseHva != IntPtr.Zero)
                    {
                        IsMapped = true;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                this.Log(MuMuLogLevel.Fatal, $"Critical crash encountered during memory mapping query loops: {ex.Message}");
                Reset();
                return false;
            }

            if (IsMapped)
            {
                this.Log(MuMuLogLevel.Info, "Phase 1 environment verification matrix successfully anchored across system memory partitions.");
                return true;
            }

            this.Log(MuMuLogLevel.Warning, "Memory sweep complete, but one or more dynamic hypervisor allocation segments were missing.");
            return false;
        }

        // --- IMemoryReader Implementation (Safe Managed Gatekeepers) ---
        public bool ReadBytes(IntPtr hvaAddress, int bufferSize, byte[] outputBytes)
        {
            if (hvaAddress == IntPtr.Zero || outputBytes == null || outputBytes.Length < bufferSize)
            {
                return false;
            }

            if (!_processTracker.IsAttached || _processTracker.TargetHandle == null)
            {
                return false;
            }

            try
            {
                // Call raw bare-metal API cleanly inside a managed safety shield
                bool success = NativeWin32.ReadProcessMemory(
                    _processTracker.TargetHandle,
                    hvaAddress,
                    outputBytes,
                    (IntPtr)bufferSize,
                    out IntPtr bytesRead);

                return success && bytesRead.ToInt64() == bufferSize;
            }
            catch (Exception ex)
            {
                this.Log(MuMuLogLevel.Error, $"Memory proxy security exception encountered: {ex.Message}");
                return false;
            }
        }

        public T ReadStructure<T>(IntPtr hvaAddress) where T : struct
        {
            int structSize = Marshal.SizeOf<T>();
            byte[] rawAllocationBuffer = new byte[structSize];

            if (ReadBytes(hvaAddress, structSize, rawAllocationBuffer))
            {
                GCHandle pinnedBuffer = GCHandle.Alloc(rawAllocationBuffer, GCHandleType.Pinned);
                try
                {
                    IntPtr bufferAddress = pinnedBuffer.AddrOfPinnedObject();
                    return Marshal.PtrToStructure<T>(bufferAddress);
                }
                finally
                {
                    pinnedBuffer.Free();
                }
            }

            return default;
        }
        public void Reset()
        {
            LowerBaseHva = IntPtr.Zero;
            UpperBaseHva = IntPtr.Zero;
            IsMapped = false;
        }
    }
}
