# Ch.3.1 - Paging Table Indices

## 1.1 Address Deconstruction
Under a standard 48-bit virtual address layout on an ARM64 Android kernel, a virtual address pointer functions as a collection of coordinates rather than a flat number. The processor cuts a 64-bit pointer into 9-bit indexing spans to navigate its translation hierarchies.

## 1.2 Bit Span Allocation Schema
Using native Material markdown features, the complete layout parsing matrix tracks coordinates across explicit bit positions:

| Segment Block Name | Targeted Bit Range | Coordinate Bit Width | Purpose / Table Destination |
| :--- | :--- | :--- | :--- |
| **Sign Extension** | `[63:48]` | 16 Bits | Distinguishes User-space (`0x0000`) vs Kernel-space (`0xFFFF`) maps |
| **PGD Index** | `[47:39]` | 9 Bits | Offset coordinate within the root Page Global Directory |
| **PUD Index** | `[38:30]` | 9 Bits | Offset coordinate within the Page Upper Directory |
| **PMD Index** | `[29:21]` | 9 Bits | Offset coordinate within the Page Middle Directory |
| **PTE Index** | `[20:12]` | 9 Bits | Offset coordinate within the final hardware Page Table Entry |
| **Page Offset** | `[11:0]` | 12 Bits | Direct relative byte address offset inside the 4096-byte target RAM cell |

---

## 1.3 C# Disassembly Logic Routines
Your core SDK disassembles these segments cleanly using high-performance bitwise right shifts combined with an extraction mask matching `0x1FF`:

```csharp
public static class AddressDisassembler
{
    private const ulong IndexMask = 0x1FF;   // Selects exactly 9 bits of precision (512 total slices)
    private const ulong OffsetMask = 0xFFF;  // Selects exactly 12 bits of lower page offset tracking

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

public struct DecoupledCoordinates
{
    public ulong PgdIndex;
    public ulong PudIndex;
    public ulong PmdIndex;
    public ulong PteIndex;
    public ulong PageOffset;
}
```
