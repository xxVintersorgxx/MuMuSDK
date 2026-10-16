# 15-Scenario Core Integration Verification Matrix

This document outlines the strict, sequential manual verification scenarios that must be executed and validated in order. No automated orchestration loops may be written until all 15 checkpoints pass cleanly with 100% predictable, documented data outputs.

---

## 🧭 The Progression Rule: No Skipping, No Guessing

!!! failure "Execution Mandate"
    If a test scenario returns an unaligned value, a null pointer, or an unexpected byte sequence, **development halts immediately.** You are completely forbidden from moving to the next tier or allowing an AI to propose a workaround. The underlying assumption must be corrected at the current failure site.

---

## 🧱 Phase 1: Host Process and Infrastructure Mapping

### 🟩 Scenario 1: Logging Extension Filtering and Short-Circuit Test
*   **Action:** Set the global log filter dropdown to `Info`. Trigger a manual loop that fires 10,000 `Trace` entries.
*   **Expected Output:** Zero console output, zero disk writes, and zero memory heap allocation spike (verifying the atomic short-circuit protection is working with zero CPU overhead).

### 🟩 Scenario 2: Downhill Log Formatter Extraction Test
*   **Action:** Trigger a deliberate `Info` entry inside your memory mapper class with two primitive arguments (`ulong`, `int`).
*   **Expected Output:** The downstream log formatter extension catches the record, correctly appends compile-time file placement signatures (`[MemoryMapper.cs:Line XX]`), and outputs a beautifully unified, text-rendered string to console/UI.

### 🟩 Scenario 3: Win32 Process Handle Verification
*   **Action:** Execute your P/Invoke process scan targeting the specific worker thread identifier: `MuMuNxDevice.exe`.
*   **Expected Output:** Returns a valid, non-null Windows process context handle with active memory query permissions.

### 🟩 Scenario 4: Lower System Space Allocation Sweep
*   **Action:** Run a linear allocation boundary scan across the process allocations filtering for `MEM_COMMIT`, `PAGE_READWRITE`, and `MEM_PRIVATE`.
*   **Expected Output:** Pinpoints an unmanaged memory block matching **exactly 1.5625 GB** in size. Capture its base Host Virtual Address (`lowerBaseHva`).

### 🟩 Scenario 5: Upper Application Space Allocation Sweep
*   **Action:** Continue the linear boundary scan across the host workspace allocations with the same filtering flags.
*   **Expected Output:** Pinpoints an unmanaged memory block matching **exactly 3.4375 GB** in size. Capture its base Host Virtual Address (`upperBaseHva`).

---

## 🧱 Phase 2: Guest Kernel & Task Chain Navigation

### 🟩 Scenario 6: High-Memory Kernel Signature Match
*   **Action:** Execute a flat linear byte array sweep across high guest virtual memory space (`0xFFFF...`) targeting the unique 8-byte compiled ASCII signature: `s w a p p e r \0`.
*   **Expected Output:** Locates the exact starting coordinate of the head kernel signature block inside your unmanaged host memory pool.

### 🟩 Scenario 7: `init_task` PID 0 Structural Validation
*   **Action:** Read a signed 4-byte integer exactly `0x4C8` bytes forward from your discovered `swapper` node base address.
*   **Expected Output:** Value reads exactly `0` (verifying that your `TaskDescriptor` sequential struct layout perfectly aligns with the active Android 15 kernel memory geometry).

### 🟩 Scenario 8: Circular Doubly-Linked List Step Test
*   **Action:** Read the 8-byte next pointer located at offset `0x30` of the `init_task` node, pass it through the `HypervisorRouter`, and step to the adjacent node.
*   **Expected Output:** Successfully lands on the next active system task descriptor node without throwing an memory reading exception or dropping into an infinite loop.

### 🟩 Scenario 9: Target Package Process Discovery
*   **Action:** Run your manual task traversal loop all the way through the circular chain, checking offset `0x6A0` on every task node for your specific game's string identifier (e.g., `com.Target.Game`).
*   **Expected Output:** Loop cleanly terminates and returns the valid Host Virtual Address pointer mapping the target game's unique `task_struct` node.

---

## 🧱 Phase 3: Hardware MMU Page Walking & Translation

### 🟩 Scenario 10: Memory Manager Descriptor Pointer Validation
*   **Action:** Read the 8-byte reference pointer located at offset `0x480` (`mm_map`) of your isolated target game task node.
*   **Expected Output:** Returns a non-null, valid coordinate value (verifying the target process owns an active user-mode memory map context).

### 🟩 Scenario 11: PGD Root Entry Extraction
*   **Action:** Follow the memory manager map pointer down to offset `0x40` inside the host RAM layout to extract the process's **Page Global Directory (PGD)** root.
*   **Expected Output:** Returns a valid Guest Physical Address coordinate mapping the structural head of that application's hardware paging hierarchies.

### 🟩 Scenario 12: Precision 48-Bit Virtual Address Disassembly
*   **Action:** Pass a known game pointer address into your `AddressDisassembler` utility routine.
*   **Expected Output:** Correctly cuts the 64-bit address into clean, 9-bit indexing slices and isolates the exact lower 12-bit page cell offset matching standard ARM64 architecture layout specs.

### 🟩 Scenario 13: Manual Multi-Tiered Page Table Walk (The Ultimate Core Proof)
*   **Action:** Step manually through the 4-level table hierarchy (PGD -> PUD -> PMD -> PTE) by applying index offsets and clearing hardware flags with your page alignment mask (`0x0000FFFFFFFFF000UL`).
*   **Expected Output:** Resolves the final table line to reveal the absolute **Guest Physical Address (GPA)** frame mapping for that variable.

### 🟩 Scenario 14: Hypervisor Route & MMIO Hole Verification
*   **Action:** Hand your resolved GPA frame over to your `HypervisorRouter` service.
*   **Expected Output:** Properly identifies if the coordinate sits above or below the virtual MMIO hardware gap boundary and returns the absolute, translated **Host Virtual Address (HVA)** pointer.

---

## 🧱 Phase 4: Dynamic Presentation and Plugin Binding

### 🟩 Scenario 15: Asynchronous Intent-Driven Presentation Sync
*   **Action:** Dispatch a `StartIntrospectionIntent` from your Avalonia UI view via your fire-and-forget message bus. Relax the master ticker engine to a steady 100ms delta-cadence.
*   **Expected Output:** The background worker picks up the intent, maps the process, loops the ticker, and pushes thread-safe string metrics back uphill across the bus channel—updating your desktop interface log stream and display elements uniformly without a single micro-stutter or cross-thread exception.
