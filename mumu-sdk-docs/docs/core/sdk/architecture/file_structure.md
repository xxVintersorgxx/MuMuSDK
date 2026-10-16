# C# SDK Solution Layout and Compilation Matrix

This blueprint defines the structural filesystem layout, assembly namespaces, and validation checklists required to initialize the unified .NET 10 development workspace.

---

!!! info "📁 Parent Solution File Tree"
    ```text
    MuMuSDK/
    ├── src/                               # Development Solution Root Workspace
        └── MuMuSDK.sln                    # Unified Visual Studio Parent Solution File
            │
            ├── 1_Core/                    # SOLUTION FOLDER: Invariant Abstraction Layers
            │   └── MuMuSDK.Core/          # Immutable Introspection Class Assembly
            │       ├── Crawler/           # INCLUDED NATIVELY: Brute-force pointer sweep & lookup engines
            │       ├── Drivers/           # Win32 OS-Level Memory / P/Invoke Handles
            │       ├── Extensions/        # INCLUDED NATIVELY: Shared Brotli stream utilities & memory pinning
            │       ├── Interfaces/        # Decoupling Boundary Bridges (IGamePlugin)
            │       ├── Models/            # Explicit Memory Hardware Representations
            │       └── Services/          # Core Hardware Translators (MMU / Mapper)
            │
            ├── 2_Apps/                    # SOLUTION FOLDER: Orchestration & UI Runtimes
            │   └── MuMuSDK.Desktop/       # Cross-Platform Avalonia UI MVVM Application
            │
            └── 3_Plugins/                 # SOLUTION FOLDER: Obfuscation Isolation Spaces
                └── MuMuSDK.Plugins.Game/  # Isolated Target Game Decoder Module DLLs
    ```

---

## 🏗️ Internal Infrastructure Consolidations (.NET 10)

To completely eliminate project boilerplate bloat and prevent code duplication, universal primitives are integrated directly into the core library space:

### 1. Unified Brotli Compression Infrastructure (`MuMuSDK.Core.Extensions`)
*   **Operational Integration:** Rather than forcing independent game plugins to implement local stream processing hooks or copy-pasting array allocations, the core library exposes a non-allocating, span-based `BrotliStreamHelper`. 
*   **Plugin Consumption:** Individual target game plugins natively inherit access to this helper via their baseline `MuMuSDK.Core` namespace reference, allowing them to decompress game strings seamlessly via a single function execution.

### 2. Native Crawler Engine Integration (`MuMuSDK.Core.Crawler`)
*   **Operational Integration:** The brute-force address lookup loop, memory boundary filtering, and pointer validation engines are baked right into the core abstraction assembly.
*   **Strategic Advantage:** This removes the need for a separate pointer crawler project file framework. The scanner can communicate directly with the local `MmuEmulator` memory table parsing tracks at maximum hardware performance. When the presentation UI demands a pointer crawl, it invokes this core service layer directly.

---

## 🛠️ Verification Checklist for Workspace Initialization

<div class="grid cards" markdown>

-   ### 🚀 Target Framework
    All projects must compile against **`.NET 10`** to utilize native compiler JIT enhancements, optimized hardware intrinsics, and clean unmanaged array pinning.

-   ### 🔒 Code Context
    The `MuMuSDK.Core` assembly properties must explicitly toggle `<AllowUnsafeBlocks>true</AllowUnsafeBlocks>` to allow native pointer memory arithmetic over raw byte stream segments.

</div>

!!! success "Assembly Reference Invariants"
    *   **Downhill Reference Chain:** `MuMuSDK.Core` remains completely self-contained with zero external dependencies. Your application (`MuMuSDK.Desktop`) and your modular dynamic plugins reference `MuMuSDK.Core`, cleanly consolidating your entire runtime stack into a unified, high-performance workspace ecosystem.
