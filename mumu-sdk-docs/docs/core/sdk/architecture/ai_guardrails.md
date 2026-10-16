# 🛡️ AI Engineering Guardrails & Prompt Instructions

This profile serves as an invariant context anchor for artificial intelligence collaborators. When generating, refactoring, or evaluating code blocks for this repository, the AI must strictly adhere to these architectural boundaries and non-negotiable compilation invariants.

---

## 🚫 Absolute Refusal Thresholds

!!! failure "Forbidden AI Actions"
    *   **Do Not Collapse Layers:** Never merge, nest, or embed game-specific decryption algorithms or IL2CPP signature configurations inside the `MuMuSDK.Core` or `MuMuSDK.CLI` namespaces.
    *   **Do Not Inject Win32 into Plugins:** If requested to optimize or write a pointer-solving engine inside a game-specific plugin library, the AI must refuse to introduce P/Invoke signatures (`OpenProcess`, `ReadProcessMemory`, `VirtualQueryEx`) into that assembly space.
    *   **Do Not Autocomplete Hardware Masks:** Do not invent, guess, or modify physical page-walking masks (`0x0000FFFFFFFFF000UL`) or memory boundary constants (`0x64000000`) without explicit confirmation that the underlying Android 15 kernel layout has shifted.

---

## 🛑 The Strict Zero-Fallback Policy

To prevent false positives, defend unmanaged scanning stability, and ensure instant failure visibility:

!!! danger "Zero-Fallback & Fail-Fast Mandates"
    *   **No Magic Assumptions:** All hardware memory allocation sizing parameters, host Process IDs, network bridge port assignments, and directory locations must derive entirely from verified live runtime streams or physical system assets. Hardcoding arbitrary default constants (e.g., standard 5GB memory boundaries) is strictly prohibited.
    *   **Fail-Fast Initialization:** If an orchestration command pipe (`mumu-cli.exe`), file parser, or native process attachment handle loop throws an exception or returns partial/malformed data, the sub-system must fail immediately and aggressively.
    *   **Explicit Error Signatures:** Failed execution paths must leave state parameters strictly at zero or explicitly invalid indicators (`IsValid = false`, `PID = 0`, `MemorySize = 0`). They must push a highly descriptive error string to the diagnostic terminal log output rather than returning a legacy placeholder object.

---

## 🏗️ Architectural & Workspace Class Separation

!!! success "Decoupling Boundaries"
    *   **Unmanaged Boundary Quarantine:** Direct `[DllImport]` signatures and raw unmanaged structure definitions must exist exclusively within `NativeWin32.cs`. Higher-level core tracking layers use clean, managed proxy wrappers (like `ReadBytes()`). Everything outside of Core must call these managed gatekeepers.
    *   **Static vs. Dynamic Workspace Separation:** Any utility built to unpack, decrypt, or parse local file assets (`global-metadata.dat` / `libil2cpp.so`) must be maintained in a **completely separate Visual Studio solution workspace**. 
    *   **The Communication Pipeline:** The standalone file tool executes entirely offline, outputting a flat, readable data payload (e.g., a serialized `offsets.json` configuration block). The target game plugin in the main solution references *only* this serialized output file at startup, freeing your live memory scanning loop from carrying heavy static file parsing or desktop disassembly dependencies.
    *   **Strict Dependency Injection Execution:** ViewModels and background workers must decouple state allocations by requesting global singletons via constructor arrays, preventing memory leaks and tracking collisions.
    *   **Zero-Allocation Core Logging:** Logging inside high-frequency introspection loops must route exclusively via the custom non-allocating static extension wrapper (`this.Log()`).

---

## 🤖 Code Generation & Context Protocols (.NET 10)

!!! info "Mandatory Implementation Rules"
    *   **Type Safety Rule:** Always prioritize explicit C# sequential marshaling configurations (`[StructLayout(LayoutKind.Sequential, Pack = 1)]`) when representing Linux guest kernel structures. Never permit the compiler to calculate dynamic field offsets.
    *   **Memory Context Inheritance:** Ensure all proposed processing streams accept the host memory base references passed down through the unified interface contract (`IGamePlugin`), rather than instantiating independent tracking states.
    *   **Defensive Service Mapping:** When writing service registrations for the cross-project inversion container, always ensure services are mapped under their respective abstract interface boundaries to prevent concrete class coupling.


    *   **The Runtime Process Mutation Clause:** The handle tracking layers must actively assume that the target host Process ID (`PID`) and underlying Windows graphical handles (`main_wnd`, `render_wnd`) will mutate completely across emulator restart cycles or multi-instance warm-reboots. The attachment subsystem is barred from caching legacy process tokens across separate operational run sequences; it must execute a fresh configuration-driven lookup via the CLI on every tracking cycle.
