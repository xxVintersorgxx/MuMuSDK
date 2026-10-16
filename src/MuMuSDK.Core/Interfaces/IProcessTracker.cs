using Microsoft.Win32.SafeHandles;

namespace MuMuSDK.Core.Interfaces
{
    public interface IProcessTracker
    {
        int ProcessId { get; }
        bool IsAttached { get; }
        SafeProcessHandle? TargetHandle { get; }
        bool TryAttach(string processName);
        void Detach();
    }
}