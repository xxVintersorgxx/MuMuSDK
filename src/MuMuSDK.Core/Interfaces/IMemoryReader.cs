using System;

namespace MuMuSDK.Core.Interfaces
{
    public interface IMemoryReader
    {
        bool ReadBytes(IntPtr hvaAddress, int bufferSize, byte[] outputBytes);
        T ReadStructure<T>(IntPtr hvaAddress) where T : struct;
    }
}