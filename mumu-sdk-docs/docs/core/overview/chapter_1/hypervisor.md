!!! abstract "VMI Architectural Anchor"
    **Core SDK Responsibility:** The \(GPA \longrightarrow HVA\) conditional translation logic defined here is handled explicitly by the `HypervisorRouter` service. It bridges the hardware MMIO hole before guest kernel structures are parsed.

# Ch.1.2 - Layer B: Hypervisor EPT Map

## 1.1 Second Level Address Translation (SLAT / EPT)
Layer B is the hardware translation bridge enforced at the silicon layer of the CPU socket. Modern processors run hardware virtual machines using **Second Level Address Translation (SLAT)**, which Intel designates as **EPT (Extended Page Tables)**. 

The CPU splits memory translation into two entirely independent hierarchies managed concurrently inside the silicon chip:

!!! info "Hardware CPU Paging Separation"
    *   **CR3 / TTBR0_EL1:** Handles Guest Virtual Address to Guest Physical Address conversions ($GVA \longrightarrow GPA$) managed inside the guest operating system.
    *   **EPT Pointer:** Handles Guest Physical Address to Host Physical Address conversions ($GPA \longrightarrow HPA$) managed by the Windows Kernel.

## 1.2 Layer B ──> Layer A Routing Mathematical Logic
The relationship between what the Guest OS sees as a Physical Address and how it maps to your Windows Host addresses follows a predictable structure due to the virtual MMIO (Memory-Mapped I/O) Gap. This gap splits the Guest Physical Address (GPA) space into two segments to prevent guest operating system memory boundaries from overlapping with underlying virtualized hardware registers.

!!! note "Memory Architecture Block Mapping"
    ```mermaid
    graph TD
        subgraph GPA [Layer B: Guest Physical Address Space]
            A[0x00000000<br>Lower System Space<br>Size: 1.5625 GB] --> L1["Offset: 0x64000000"] --> B[Virtual Hardware / MMIO Hole<br>Trapped by Hypervisor]
            B --> L2["Offset: 0x80000000"] --> C[Upper Main App Space<br>Size: 3.4375 GB]
        end

        subgraph HVA [Layer A: Windows Host RAM Blocks]
            D[(1.5625 GB Host Buffer Pointer)]
            E[(3.4375 GB Host Buffer Pointer)]
        end

        A -->|"[Rule A: Direct Map]"| D
        C -->|"[Rule B: GPA - 0x80000000]"| E
    ```


Because of this architectural gap, the SDK applies an internal conditional branch logic model to route GPA lookups to the correct Windows memory buffer:

!!! abstract "Layer B ──> Layer A Translation Algorithm"
    ```csharp
    public static class HypervisorRouter
    {
        // Core Hardware Boundary Memory Offsets
        private const ulong MmioHoleStart = 0x64000000;
        private const ulong UpperRamStart = 0x80000000;

        /// <summary>
        /// Translates a Guest Physical Address (GPA) to a readable Host Virtual Address (HVA).
        /// </summary>
        public static IntPtr TranslateGpaToHva(ulong gpa, IntPtr lowerBaseHva, IntPtr upperBaseHva)
        {
            // CASE 1: Lower System Memory Range Translation
            if (gpa < MmioHoleStart)
            {
                return new IntPtr(lowerBaseHva.ToInt64() + (long)gpa);
            }

            // CASE 2: Upper Application Space Translation (Active Engine Pool)
            if (gpa >= UpperRamStart)
            {
                ulong memoryOffset = gpa - UpperRamStart;
                return new IntPtr(upperBaseHva.ToInt64() + (long)memoryOffset);
            }

            // CASE 3: Hardware Exception Catch Boundary
            throw new InvalidOperationException(
                \$"Access violation: GPA 0x{gpa:X} trapped inside MMIO hardware gap."
            );
        }
    }
    ```
