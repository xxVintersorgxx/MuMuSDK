# Ch.2.3 - PGD Directory Resolution

## 1.1 Locating the Memory Manager Descriptor
Once the task loop isolates the specific process node for the target application, it reads the 64-bit reference address stored at offset **`0x480`** (`MemoryMapPointer`). If this value is completely null (`0x0`), it indicates a pure background kernel worker thread; user-mode environments will always return a valid memory manager pointer.

## 1.2 Resolving the Page Global Directory (PGD)
Following this pointer down into the memory block allows the engine to parse the application's underlying memory descriptors. Inside this sub-structure lies the key register mapping point:

| Field Reference | Data Type | Structural Offset | Architectural / Hardware Function |
| :--- | :--- | :--- | :--- |
| `users_count` | `atomic_t` | `0x00` | Tracks active shared threads referencing this map |
| `pgd_base` | `entry_t*` | `0x40` | Root pointer to the 4-level page table structure |

The 64-bit value located at offset **`0x40`** represents the **Page Global Directory (PGD)** root. On a physical phone, this exact coordinate maps to the hardware register index loaded into the CPU's **`TTBR0_EL1`** configuration space whenever that application is active.

!!! note "Hardware Root Mapping Matrix"
    ```mermaid
    graph LR
        subgraph Layer_C [Layer C: Process Descriptor]
            A["task_struct Node<br>Offset 0x480"] --> B["mm_struct Map Pointer<br>Offset 0x40"]
        end
        subgraph Layer_B [Layer B: Hypervisor EPT Base]
            B --> C["pgd_base Root Address<br>Guest Physical Address"]
        end
        subgraph Target [Translation Input]
            C --> D["Chapter 3 Paging Engine<br>Translation Table Target"]
        end
    ```

## 1.3 C# PGD Extraction Blueprint
Your SDK reads this value out-of-process to fetch the permanent anchor point needed to drive your bit-shifting address translation calculations:

!!! abstract "C# PGD Anchor Resolution Routines"
    ```csharp
    public static class PgdResolver
    {
        /// <summary>
        /// Extracts the raw Page Global Directory Guest Physical Address from an isolated task node.
        /// </summary>
        public static ulong ExtractPgdBaseAddress(IntPtr targetTaskNode, IntPtr upperBaseHva)
        {
            // 1. Fetch the mm_struct pointer from task_struct offset 0x480
            IntPtr mmStructGpa = ReadPointer(targetTaskNode + 0x480);
            if (mmStructGpa == IntPtr.Zero) return 0;

            // 2. Convert memory map GPA pointer handle to standard Host Virtual Address space
            IntPtr mmStructHva = HypervisorRouter.TranslateGpaToHva((ulong)mmStructGpa, IntPtr.Zero, upperBaseHva);

            // 3. Extract the master pgd_base address from mm_struct offset 0x40
            ulong rawPgdBaseGpa = ReadUInt64(mmStructHva + 0x40);
            
            // This resolved ulong is your invariant translation table root anchor!
            return rawPgdBaseGpa;
        }
    }
    ```
