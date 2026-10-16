# Hypervisor Isolation & Hardware Validation Checkpoints

This document establishes the strict diagnostic validation rules required to verify the host-side hypervisor abstraction layer independently of game-specific metadata decryption or HybridCLR obfuscation hooks.

---

## 🧭 The Single Point of Truth Mandate

To isolate hypervisor mapping failures from game obfuscation failures, the Core SDK must provide explicit, native validation routines that prove the \(GVA \longrightarrow GPA \longrightarrow HVA\) mapping pipeline is working flawlessly using immutable Android system signatures. 

We do this by completely ignoring the target game binary and targeting the invariant **Linux Kernel Signature Node**.

---

## 🧱 Layered Verification Testing Matrix

Before executing a single game plugin decryption routine, the orchestrator application can run this sequential checklist to pinpoint the exact failure layer:

```text
Introspection Initialization Validation Pipeline
 │
 ├── [Checkpoint 1: Win32 Allocation Mapping] 
 │    └── Test: Locate MuMuNxDevice.exe process space and isolate 1.5625 GB / 3.4375 GB segments.
 │    └── Failure Meaning: Hypervisor base addresses shifted or ASLR rules altered. (Core SDK Error)
 │
 ├── [Checkpoint 2: Linux Kernel Signature Scan]
 │    └── Test: Sweep high memory for the invariant 'swapper\0' string and verify PID == 0.
 │    └── Failure Meaning: Android 15 kernel data layout or page table structure altered. (Core SDK Error)
 │
 └── [Checkpoint 3: Hardware MMU Table Walk Verification]
      └── Test: Read the init_task page structures manually using the 48-bit shift emulator.
      └── Failure Meaning: Page size scaling factor or table-entry masks are misaligned. (Core SDK Error)
```

---

## 🛡️ Boundary Control Isolation Rules

!!! abstract "Isolating Core SDK Layer Faults from Game Obfuscation Faults"

    === "If Checkpoints 1, 2, and 3 Pass Perfectly"
        *   **Diagnostic Verdict:** The hypervisor abstraction layer, your C# Win32 mapping engine, and your out-of-process ARM64 48-bit page table walker are **100% correct**. 
        *   **Action Plan:** If the output data stream returns corrupt bytes after this point, you can completely ignore your core SDK infrastructure. The bug resides strictly within the game plugin's IL2CPP v31 metadata decryption algorithm, the HybridCLR interpreter parsing loop, or a localized pointer bit-shift routine.

    === "If Checkpoint 2 or 3 Fails"
        *   **Diagnostic Verdict:** Your core infrastructure is blind or unaligned. 
        *   **Action Plan:** Do not touch your game plugin code. Lock down the game reversing files and focus entirely on auditing your `MemoryMapper` section sweep requirements or checking your `MmuEmulator` left-shift scaling mechanics against your third-party VMMap logs.

---

## ⚙️ Primitive Test Bench Enforcements

!!! success "Hardware Isolation Rules"
    *   **The Invariant Test Target:** The Core SDK should feature a compilation configuration tag named `#DEBUG_VMI_CORE`. When toggled, the application attempts to read out the native Android kernel build version string directly from its invariant system space. If this string resolves cleanly, it proves your host scanner has a perfect, clear window straight into live target memory space.
    *   **Complete Decoupling from Brotli/HybridCLR:** The hardware mapping engine must never import compression libraries or utilize runtime script parsing objects. It deals exclusively with flat, raw memory arrays.
