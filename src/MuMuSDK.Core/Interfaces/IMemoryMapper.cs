using System;

namespace MuMuSDK.Core.Interfaces
{
    public interface IMemoryMapper
    {
        IntPtr LowerBaseHva { get; }
        IntPtr UpperBaseHva { get; }
        bool IsMapped { get; }

        /// <summary>
        /// Executes a step-by-step linear VirtualQueryEx sweep to isolate dynamic hypervisor allocations.
        /// </summary>
        /// <param name="lowerSizeTarget">Dynamic memory byte count threshold for Phase 1 base structures.</param>
        /// <param name="upperSizeTarget">Dynamic memory byte count threshold for Phase 2 memory spaces.</param>
        bool InitializeMapping(long lowerSizeTarget, long upperSizeTarget);

        void Reset();
    }
}
