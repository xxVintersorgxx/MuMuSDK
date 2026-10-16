using System;
using MuMuSDK.Core.Models;

namespace MuMuSDK.Core.Services
{
    public static class AddressDisassembler
    {
        private const ulong IndexMask = 0x1FF;
        private const ulong OffsetMask = 0xFFF;

        public static DecoupledCoordinates SplitVirtualAddress(ulong gva)
        {
            return new DecoupledCoordinates
            {
                PgdIndex   = (gva >> 39) & IndexMask,
                PudIndex   = (gva >> 30) & IndexMask,
                PmdIndex   = (gva >> 21) & IndexMask,
                PteIndex   = (gva >> 12) & IndexMask,
                PageOffset = gva & OffsetMask
            };
        }
    }
}