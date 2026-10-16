# SDK Multi-Project Decoupling Specification

This document defines the physical compilation barriers, interface contract properties, and cross-assembly communication boundaries for the .NET 10 Virtual Machine Introspection architecture.

---

<div class="grid cards" markdown>

-   ### 🧭 Strategy
    Isolate the hypervisor memory access mechanics completely from game-specific reversing routines to prevent architectural pollution.

-   ### 🛡️ Type Safety
    Enforce compile-time constraints that prevent temporary game plugins from directly invoking Win32 native memory APIs.

</div>

---

## 🧱 The Decoupled Project-Boundary Contract

=== "Architectural Design"
    The core SDK establishes an invariant contract interface (`IGamePlugin`) that all separate game plugin dynamic link libraries (DLLs) must implement. This ensures the main orchestrator can interface with any target plugin without compilation dependencies.

    !!! info "Dynamic Interface Component Requirements"

        | Interface Property / Method | Returns / Parameters | Target Architectural Purpose |
        | :--- | :--- | :--- |
        | `TargetProcessCommName` | `string` | Matches the Linux kernel's 16-byte `process_comm` tag array dynamically. |
        | `PluginVersionSignature` | `string` | Pins build versions to avoid out-of-date pointer execution crashes. |
        | `ExecuteIntrospection` | `void` | Accepts primitive virtualization base coordinates passed downward from the Core engine. |

=== "Interface Contract Signature (.NET 10)"
    ```csharp
    namespace MuMuSDK.Core.Interfaces
    {
        public interface IGamePlugin
        {
            string TargetProcessCommName { get; } // (1)
            string PluginVersionSignature { get; } // (2)
            void ExecuteIntrospection(ulong pgdBaseGpa, IntPtr upperRamHva); // (3)
        }
    }
    ```

    1. 🔍 Explicit process tag used by the dynamic runtime loader to match active running packages.
    2. 🛡️ Verification fingerprint used to block execution paths if the underlying game pushes a new update.
    3. ⚡ The primary execution entry hook invoked by the host presentation orchestrator.

---

## 🎛️ Operational Domain Division

To ensure compile-time isolation, developers and AI collaborators must restrict code blocks to their designated assembly boundaries:

```text
  [ ARM64 Game GVA Pointer ] ──► [ Game Plugin Space ] ──► Handles Brotli, HybridCLR, Obfuscation
                                         │
                                         ▼ (Primitive Request Pipeline)
  [ Hypervisor Translation ] ──► [ Core SDK Framework ] ──► Handles Win32, Task Loops, MMU Page Walking
                                         │
                                         ▼ (Hardware Read Mirror)
  [ Physical PC RAM Cells  ] ──► [ Windows Host OS ]   ──► Root execution anchor point
```

### 1. Invariant Infrastructure (The Core SDK Assembly Domain)
*   **Win32 Process Mapping:** Locates hypervisor host process wrappers dynamically via native API sweeps.
*   **Kernel Traversal:** Discovers guest task descriptor chains out-of-process without modifying internal states.
*   **Hardware MMU Walk:** Replicates multi-tiered ARM64 bitmask shifting equations dynamically.
*   **Data Transport:** Delivers flat, raw byte stream pools up to the boundary interface layer.
*   *Strict Constraint:* Must contain zero knowledge of IL2CPP metadata arrays, Brotli decompression, or game offsets.

### 2. Volatile Target Logic (The Game Plugin Assembly Domain)
*   **Pointer De-obfuscation:** Solves runtime bitwise string scrambles and dynamic virtual pointer changes.
*   **Interpreter Navigation:** Decodes proprietary HybridCLR metadata tables and execution layers.
*   **Payload Unpacking:** Decompresses Brotli asset components streaming from live execution blocks.
*   **Metric Materialization:** Packages raw primitives into strong-typed structs for display in the Avalonia UI.
*   *Strict Constraint:* Completely forbidden from using Win32 memory access handles. It can only see data via the Core SDK's translation primitives.

---

## ⚠️ Architectural Invariants & Guardrails

!!! failure "API Isolation Constraints"
    ```mermaid
    graph TD
        subgraph Core_Library [MuMuSDK.Core Assembly]
            A[Win32 API Bindings] --> B[MMU Page Walker]
        end
        subgraph Plugin_Space [Isolated Plugin Assembly]
            C[Game Pointer Solver] --> D[Metadata Extractor]
        end
        B -->|Provides Primitive Address Values Only| C
        style Plugin_Space fill:#1e222b,stroke:#444444
        style Core_Library fill:#1e222b,stroke:#66ccff
    ```
    *   **No Direct Win32 Access:** Plugin components are completely isolated from raw Windows handle manipulation functions.
    *   **State Agnosticism:** The Core library space must remain 100% agnostic to game-specific offsets, scrambled virtual loops, or decryption schemas.

---

## 🛠️ Companion Tooling Ecosystem: Standalone IL2CPP Decryptor

To maintain structural isolation, reverse-engineering static analysis tools must exist outside the live introspection lifecycle.

!!! success "Decoupled IL2CPP Extraction Pipeline"
    
    ```text
    Obfuscated Game APK Assets ──► [ Standalone Decryption Tool ] ──► Decrypted Metadata Structures
                                                                              │
                                                                              ▼ (Port Rules / Signatures)
    Live Memory Space Map     ──► [ Core SDK Introspection Engine ] ──► [ Game-Specific DLL Plugin ]
    ```

*   **Operational Mandate:** The companion decryption utility executes completely offline against extracted application binaries (`libil2cpp.so` and `global-metadata.dat`).
*   **Plugin Synergy:** The cryptographic keys, header unscrambling offsets, and structural reordering tables discovered by the standalone tool are compiled directly into the localized game plugin layer, keeping the Core SDK clean and performant.
