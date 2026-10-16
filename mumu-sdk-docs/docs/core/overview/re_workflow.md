# Reverse Engineering Workflow & Multi-Tool Provenance

This document establishes the strict multi-tool reverse engineering pipeline used to verify memory structures, cryptographic keys, and pointer offsets prior to code initialization.

---

## 🧭 The Core Mandate: Proved Reality Before Code

!!! failure "Operational Workflow Imperative"
    No project files, classes, or code block implementations may be written inside the C# solution workspace based on raw assumptions or unverified guesses. 
    
    Every structural offset, bit-shift mask, or decryption algorithm must pass through our multi-tool verification pipeline first. Once proven, the findings are permanently recorded in this documentation suite, which then serves as the absolute blueprint for our production code.

---

## 🛠️ The Multi-Tool Verification Workbench

Our reverse engineering pipeline leverages distinct software execution suites to cross-verify memory data completely independent of the main C# runtime environment:

```text
  [ Target App Binary & Assets ] ──► [ Ghidra Static Disassembly ] ──► Uncovers Structural Layouts & Keys
                                                                               │
                                                                               ▼ (Map to Live State)
  [ Live Hypervisor Memory    ] ──► [ Cheat Engine & Lua Scripts ] ──► Validates Pointers and Memory Sweeps
                                                                               │
                                                                               ▼ (Prototype Cryptography)
  [ Extracted Stream Buffers  ] ──► [ Python Script Prototyping ]  ──► Proves Brotli / De-obfuscation Math
                                                                               │
                                                                               ▼ (Lock Into Blueprint)
  [ Invariant Specifications  ] ──► [ MkDocs Material Documentation]──► Production C# Code Mirror
```

### 1. Static Binaries & Structural Reversing (Ghidra Layer)
*   **Tactical Focus:** Extracting `libil2cpp.so` and processing it alongside dumped metadata headers to discover static types. 
*   **Operational Output:** Identifies how the game binary obfuscates its data layouts, maps the HybridCLR interpreter execution tables, and uncovers cryptographic initialization keys used for raw file decryption.

### 2. Live Memory State Validation (Cheat Engine & Lua Pipeline)
*   **Tactical Focus:** Attaching directly to the host hypervisor process workspace (`MuMuNxDevice.exe`) to execute real-time address validation. 
*   **Operational Output:** Utilizing the internal Cheat Engine Lua engine to script manual 4-level page table walks and test pointer-crawling paths directly within CE. This proves our hardware mapping math matches live execution byte-for-byte before we ever write a C# memory driver loop.

### 3. Crypotographic & Compression Prototyping (Python Stream Layer)
*   **Tactical Focus:** Executing rapid data-manipulation passes over raw memory streams dumped by our reversing tools.
*   **Operational Output:** Python scripts test the parsing loops, handle bit-shifting equations, and execute Brotli stream decompression against actual raw game data packets. This eliminates marshalling or compiler quirks from masking our initial logic flaws.

---

## ⚠️ Architectural Invariants for AI Collaboration

!!! success "AI Directives for the Prototyping Phase"
    *   **The Documentation Invariant:** When tasked with analyzing an obfuscated structure or writing a parsing algorithm, the AI must base its logic entirely on verified inputs extracted from your Ghidra, Python, or Cheat Engine logs. 
    *   **Anti-Guessing Gatekeeper:** If a layout is unverified, the AI must help you construct a specific **Cheat Engine Lua snippet** or a **Python diagnostic script** to test the assumption against live bytes first, keeping your C# project workspace completely pristine and free of experimental clutter.
