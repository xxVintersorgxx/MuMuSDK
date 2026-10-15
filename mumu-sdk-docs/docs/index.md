# Out-of-Process Introspection Engine Architecture Specification

This document establishes the technical design requirements, memory mapping matrices, and validation logs for building a host-side memory extraction platform targeting hardware-assisted virtualization layers.

---

## 🧭 Strategic Intent & Principles

### Strategic Goals
*   **Zero Guest-Side Footprint:** Maintain absolute isolation by operating entirely from the Windows Host layer. The target operating system must remain completely unmodified. 
*   **Immune to Guest Attestation:** Avoid using user-mode function hooks, guest-side debugging pipelines (`Frida`/`GDB`), or modifying guest-mode storage files. This ensures total immunity against internal guest application integrity checks and hardware attestation frameworks.
*   **Hardware-Assisted Introspection:** Bypass software-layer scanning by leveraging native processor execution tables and operating system object descriptors to locate live targets.

### Architectural Summary
Virtualization systems enforce hardware-level isolation barriers to convince the guest operating system it owns physical processing silicon. Rather than attempting to break this barrier from the inside, this SDK treats the entire running virtual machine as a raw, sequential pool of host-allocated bytes. 

By emulating a hardware Memory Management Unit (MMU) directly inside our host application, we can read, translate, and verify live execution threads completely out-of-process.

### SDK Overview: Structural Architecture
The instrumentation engine is split across three distinct operational layers wrapped in a modular C# / Rust software development kit:

!!! note "Core Engine Pipeline Mapping"
    ```mermaid
    graph TD
        subgraph Host_SDK [External Core SDK Module Workspace]
            M1["TargetMonitor Module<br>CLI Orchestration"] --> M2["MemoryMapper Module<br>Boundary Isolation"]
            M2 --> M3["MmuEmulator Module<br>Page Table Walker"]
        end

        subgraph Windows_Kernel [Layer A: Host Operating System]
            M2 --> L1["Query: VirtualQueryEx"] --> HVA["Isolate Host Virtual Address Blocks"]
        end

        subgraph Hypervisor [Layer B: Silicon Hardware]
            M3 --> L2["Query: Manual PTW Walk"] --> GPA["Resolve Guest Physical Address"]
        end
    ```

1.  **`TargetMonitor`:** Automates system token parsing and coordinates instance diagnostics to scrape active configurations.
2.  **`MemoryMapper`:** Performs un-opinionated allocation sweeps to dynamically isolate hypervisor hardware memory segments.
3.  **`MmuEmulator`:** Re-creates multi-level paging tables out-of-process to translate virtual runtime metrics into invariant host coordinates.
