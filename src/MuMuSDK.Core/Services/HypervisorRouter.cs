using System;

namespace MuMuSDK.Core.Services
{
    public static class HypervisorRouter
    {
        private const ulong MmioHoleStart = 0x64000000;
        private const ulong UpperRamStart = 0x80000000;

        public static IntPtr TranslateGpaToHva(ulong gpa, IntPtr lowerBaseHva, IntPtr upperBaseHva)
        {
            if (gpa < MmioHoleStart)
            {
                return new IntPtr(lowerBaseHva.ToInt64() + (long)gpa);
            }
            if (gpa >= UpperRamStart)
            {
                ulong memoryOffset = gpa - UpperRamStart;
                return new IntPtr(upperBaseHva.ToInt64() + (long)memoryOffset);
            }
            throw new InvalidOperationException($"Access violation: GPA 0x{gpa:X} trapped inside MMIO hardware gap.");
        }
    }
}