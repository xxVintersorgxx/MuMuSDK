!!! abstract "VMI Architectural Anchor"
    **Core SDK Responsibility:** This host layer is mapped exclusively by the `MemoryMapper` service. The isolated 1.5625 GB and 3.4375 GB segments discovered here are cached immediately to anchor all downstream ARM64 page-table walking routines.

# Ch.1.1 - Layer A: Host Virtual Workspace

## 1.1 The Execution Root
Layer A represents the absolute execution root of our introspection engine. The entire target virtual phone environment is spawned, sustained, and maintained by a standard Windows user-mode worker process named `MuMuNxDevice.exe`.

## 1.2 Unmanaged Memory Allocations
To feed raw physical RAM data bytes down to the processor's hardware execution pipelines, this worker process commits massive, unmanaged memory blocks within its private host process workspace. 

!!! note "The Host Memory Mapping Reality"
    While the guest operating system believes it is reading raw electrical storage cells, those bytes actually reside inside standard memory pages allocated directly to your PC's physical RAM sticks. If the SDK can programmatically identify the exact **Host Virtual Address (HVA)** base where this container sits, we obtain a direct, invisible window straight into the live target memory space.

## 1.3 Target Boundary Isolation Rules
To ensure the SDK relies on zero hardcoded sizes or temporary offsets that break across individual software version updates, the mapping module evaluates the host workspace dynamically using two strict execution parameters:

*   **Boundary State Analysis:** The SDK runs a linear sweep across the active process allocations. It instantly filters out normal application binaries (`MEM_IMAGE`) and active web-overlay rendering heaps by isolating blocks where `State == MEM_COMMIT` and `Protect == PAGE_READWRITE`.
*   **Allocation Type Profiling:** The guest hardware blocks are generated as anonymous virtual memory sections. The engine profiles the mapping descriptor signatures, targeting regions flagged specifically as `MEM_PRIVATE` (Private Data) or anonymous pagefile backing layers.
