# Unmanaged Virtual Memory Mapping Subsystem

This document profiles the low-level Win32 page-probing configurations used to locate and isolate hypervisor address pools.

---

## 🧭 Memory Probing & Safety Guardrails

*   **Handle Encapsulation Rule:** Virtual memory sweeps require an active `SafeProcessHandle` token passed directly from the `IProcessTracker` instance. Direct pointer retention is strictly prohibited.
*   **Structured Probing Loops:** The application must crawl the target process page map utilizing native `VirtualQueryEx` configurations. Speculative memory reads or hardcoded pointer offsets are banned.
*   **Iterative Protection Filters:** The scanner will analyze memory pages, filtering allocations down to explicit system requirements (`MEM_COMMIT` state combined with `PAGE_READWRITE` accessibility matrix keys).

---

## 📊 Allocation Sweeping Sequence Track

```mermaid
sequenceDiagram
    autonumber
    participant UI as MainViewModel (Stage 2)
    participant MM as Win32MemoryMapper
    participant PT as Win32ProcessTracker
    participant K32 as Windows kernel32.dll

    UI->>MM: Invokes InitializeMapping() Intent
    activate MM
    MM->>PT: Verifies TargetHandle is active and valid
    activate PT
    PT-->>MM: Handshake Confirmed (SafeProcessHandle Provided)
    deactivate PT

    Note over MM: Execute Page Allocation Probing Loop
    loop While CurrentBaseAddress < MaxUserAddress Space
        MM->>K32: Invokes VirtualQueryEx(Handle, Address, out MEMORY_BASIC_INFORMATION)
        K32-->>MM: Streams Back Virtual Page Descriptor Token
        MM->>MM: Evaluates Commited Allocation Page Size Limits
    end

    MM->>UI: Caches Base Handles and returns Success Boolean (TRUE)
    deactivate MM
```
