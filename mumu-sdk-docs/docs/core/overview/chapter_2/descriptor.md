# Ch.2.1 - Process Descriptor Layout

## 1.1 Out-of-Process Introspection Strategy
When performing un-opinionated memory diagnostics from the host side, the SDK targets the guest operating system's internal process scheduling arrays. Because user-space address maps shift dynamically at runtime, the application must identify target environments programmatically using the kernel's native task layouts.

## 1.2 The 64-Bit Process Descriptor Layout
Every execution thread, worker task, and application running inside the Linux guest environment is represented by a central process descriptor block. For a standard 64-bit ARM Linux kernel configuration, the fields tracked during a memory audit are aligned to predictable byte offsets relative to the structure's base address:

| Field Name | Type / Structure | Byte Offset | Architectural Purpose |
| :--- | :--- | :--- | :--- |
| `task_state` | `volatile long` | `0x00` | Tracks active scheduler execution state flags |
| `task_list` | `struct list_head` | `0x30` | Circular doubly-linked list nodes (`next`/`prev` pointers) |
| `mm_map` | `struct mm_struct*` | `0x480` | Pointer to the process's active user-mode memory descriptor |
| `process_id` | `int` | `0x4C8` | The unique Linux Process ID (PID) tracking token |
| `process_comm` | `char[16]` | `0x6A0` | Alphanumeric string tag tracking the executable package name |

---

## 1.3 C# Structural Representation
To read these variables out-of-process without violating type safety, the SDK core mirrors this geometry using explicit sequential structure fields:

```csharp
using System;
using System.Runtime.InteropServices;

namespace MuMuSDK.Core.Models
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TaskDescriptor
    {
        // Padded layout tracking offsets up to list handles
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x30)]
        private byte[] _pad0;

        // Linked list handle: Offset 0x30
        public IntPtr NextTask;
        public IntPtr PrevTask;

        // Padded layout tracking list descriptors to memory mapping handles
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x438)]
        private byte[] _pad1;

        // Pointer to active memory descriptor: Offset 0x480
        public IntPtr MemoryMapPointer;

        // Process Identifier field: Offset 0x4C8
        public int ProcessId;

        // Padded layout tracking process ID to the communication label string
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x1D4)]
        private byte[] _pad2;

        // Alphanumeric process executable tag: Offset 0x6A0
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] ProcessCommName;
    }
}
```