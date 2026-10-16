# Service Specification: Hardware MMU Emulator

This module defines the bitmask geometries and multi-tiered table traversal algorithms required to emulate an ARM64 Memory Management Unit (MMU) completely out-of-process.

---

!!! abstract "Functional Objectives"
    *   Deconstruct standard 64-bit application virtual memory pointers into precise architectural coordinate slices.
    *   Manually traverse the 4-level paging directory arrays inside the unmanaged host buffer stream.
    *   Translate dynamic Guest Virtual Addresses (GVA) into invariant Guest Physical Addresses (GPA).

---

## 🧱 Mathematical Page Table Walking Rules

Under a standard 48-bit virtual address indexing map, an incoming pointer functions as an array of structured coordinate indices. The emulator cuts the pointer into 9-bit indexing slices to resolve physical destination cells:

*   **Index Selection Mask (\(\text{Mask}_{\text{idx}}\)):** `0x1FF` 
*   **Page Offset Selection Mask (\(\text{Mask}_{\text{offset}}\)):** `0xFFF`

\[\text{Index} = (\text{GVA} \gg \text{Shift}) \ \& \ \text{0x1FF}\]

```text
64-Bit Guest Virtual Address (GVA) Pointer Input
 ├── Bits [47:39] ──► PGD Index (Page Global Directory Root Offset)
 ├── Bits [38:30] ──► PUD Index (Page Upper Directory Table Offset)
 ├── Bits [29:21] ──► PMD Index (Page Middle Directory Table Offset)
 ├── Bits [20:12] ──► PTE Index (Hardware Page Table Entry Base Offset)
 └── Bits [11:0]  ──► Page Offset (Absolute Byte Target Inside 4096-Byte Page Frame)
```

---

## ⚠️ Architectural Invariants

!!! failure "Scale Precision and Alignment Verification"
    *   **Descriptor Scale Factor:** Every directory entry consumed within an ARM64 architecture table requires exactly 8 bytes of storage [^3]. The indices must be shifted left by 3 bits (\(\ll 3\)) before appending them to structural base locations.
    *   **Flag Bit Clearing:** All raw pointers pulled from intermediate directory lines must clear flag markers by executing bitwise masking alongside the hardware frame boundary mask `0x0000FFFFFFFFF000UL` before traversing down to subsequent directory tiers.

[^3]: Equivalent to standard 64-bit reference address elements.
