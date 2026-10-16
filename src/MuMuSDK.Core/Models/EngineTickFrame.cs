using System;

namespace MuMuSDK.Core.Models
{
    public readonly record struct EngineTickFrame(
        ulong FrameIndex,
        double DeltaTime,
        double TotalRuntime
    );

    public struct DecoupledCoordinates
    {
        public ulong PgdIndex;
        public ulong PudIndex;
        public ulong PmdIndex;
        public ulong PteIndex;
        public ulong PageOffset;
    }
}