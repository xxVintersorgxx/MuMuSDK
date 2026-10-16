# Service Specification: Memory Mapping Engine

This module defines the structural requirements for scanning, filtering, and mapping the Windows host memory blocks assigned to the hypervisor runtime.

---

!!! abstract "Functional Objectives"
    *   Dynamically bind to the active host worker process context (`MuMuNxDevice.exe`) [^2].
    *   Isolate the raw unmanaged memory segments representing the guest phone's physical hardware RAM.
    *   Cache invariant Host Virtual Address (HVA) base anchors to eliminate redundant scanning overhead.

[^2]: Enforced via target system process scraping mechanisms.

---

## 🧱 Algorithmic Scan Filtering Requirements

```text
Host Memory Allocation Segment
 ├── State    == MEM_COMMIT      --> Filter out uncommitted virtual pages
 ├── Protect  == PAGE_READWRITE  --> Target mutable data execution streams
 └── Type     == MEM_PRIVATE     --> Target anonymous pagefile-backed components
```

## 📊 Dynamic Target Sizing Rules

To prevent tracking failures across customized player instances, the linear page-walking sweep must never rely on hardcoded allocation sizes or static system thresholds.

*   **Phase 1 Lower System RAM Boundary:** Anchored strictly at a baseline virtual memory allocation of `1,677,721,600` Bytes.
*   **Phase 2 Upper App RAM Boundary:** Determined dynamically at initialization by converting the active instance's `"vm_mem"` CLI string variable into an absolute byte scale factor matrix.


## ⚠️ Architectural Invariants

!!! failure "Dynamic Discovery and Footprint Rules"
    *   **No Hardcoded Base Offsets:** Pointers must be fetched dynamically on initialization to survive host address space layout randomization (ASLR).
    *   **Cache Allocation Lifecycle:** The base Host Virtual Addresses (HVA) remain completely static while an emulator session is alive. Cache these handles immediately at launch to bypass tracking validation overhead.
