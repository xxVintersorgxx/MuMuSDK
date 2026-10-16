# AI Engineering Guardrails & Prompt Instructions

This page serves as an invariant context anchor for artificial intelligence collaborators. When generating, refactoring, or evaluating code blocks for this repository, the AI must strictly adhere to these operational boundaries.

---

## 🚫 Absolute Refusal Thresholds

!!! failure "Forbidden AI Actions"
    *   **Do Not Collapse Layers:** Never merge, nest, or embed game-specific decryption algorithms or IL2CPP signature configurations inside the `MuMuSDK.Core` or `MuMuSDK.CLI` namespaces.
    *   **Do Not Inject Win32 into Plugins:** If requested to optimize or write a pointer-solving engine inside a game-specific plugin library, the AI must refuse to introduce P/Invoke signatures (`OpenProcess`, `ReadProcessMemory`, `VirtualQueryEx`) into that assembly space.
    *   **Do Not Autocomplete Hardware Masks:** Do not invent, guess, or modify the physical page-walking masks (`0x0000FFFFFFFFF000UL`) or memory boundary constants (`0x64000000`) without explicit confirmation that the underlying Android 15 kernel layout has shifted.

---

## 🏗️ Companion Tooling and Standalone Solution Boundaries

To maintain maximum execution stability, the tools tracking static disk assets must remain completely separate from the real-time Virtual Machine Introspection (VMI) pipeline.

!!! success "Static vs. Dynamic Workspace Separation"
    *   **The Standalone File Decryptor:** Any utility built to unpack, decrypt, or parse local file assets (`global-metadata.dat` / `libil2cpp.so`) must be maintained in a **completely separate Visual Studio solution workspace**. 
    *   **The Communication Pipeline:** The standalone file tool executes entirely offline. It extracts obfuscated string pools or header magic byte layers, and outputs a flat, readable data payload (e.g., a serialized `offsets.json` configuration block).
    *   **The Runtime Plugin Consumption:** The target game plugin in the main solution references *only* this serialized output file. It reads the clean offsets at startup, completely freeing your live memory scanning loop from carrying heavy static file parsing or desktop disassembly dependencies.

---

## 🤖 Code Generation & Context Protocols (.NET 10)

!!! info "Mandatory Implementation Rules"
    *   **Type Safety Rule:** Always prioritize explicit C# sequential marshaling configurations (`[StructLayout(LayoutKind.Sequential, Pack = 1)]`) when representing Linux guest kernel structures. Never permit the compiler to calculate dynamic field offsets.
    *   **Memory Context Inheritance:** Ensure all proposed processing streams accept the host memory base references passed down through the unified interface contract (`IGamePlugin`), rather than instantiating independent tracking states.
    *   **Defensive Service Mapping:** When writing service registrations for the cross-project inversion container, always ensure services are mapped under their respective abstract interface boundaries to prevent concrete class coupling.
