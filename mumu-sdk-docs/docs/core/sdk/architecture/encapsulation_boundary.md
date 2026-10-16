# Strict Kernel Encapsulation Boundary Matrix

This document defines the strict horizontal decoupling guarantees that isolate external game plugin modules from host platform kernel infrastructure.

---

## 🧭 Invariant Boundary Constraints

To defend the system architecture from platform drift, runtime crashes, and compilation vulnerabilities:
*   **The Downstream Isolation Mandate:** Game plugins and presentation elements are strictly barred from importing `System.Runtime.InteropServices` or interacting with bare-metal elements.
*   **The Inversion Proxy Rule:** Any low-level action required by a module (e.g., reading target virtual addresses, checking hardware signatures) must be exposed via abstract interfaces inside `MuMuSDK.Core`. 
*   **Zero Raw Handle Exposure:** External assemblies must never hold or request raw memory pointer references (`IntPtr`). They communicate exclusively using managed arrays, primitive integers, and safe hex data strings.

---

## 📊 Structural Interop Proxy Flow

```mermaid
graph TD
    %% Define Dark Mode High-Contrast Styles
    classDef plugin fill:#3F2A42,stroke:#5A3A5A,color:#FF88FF;
    classDef core fill:#2A3542,stroke:#3A4A5A,color:#88CCFF;
    classDef win32 fill:#111115,stroke:#3A3A45,color:#A1A1AA;

    Plugin[MuMuSDK.Plugins.GameModule]:::plugin -->|1. Requests Memory Operation| Contract[IMemoryInterface Contract]:::core
    Contract -->|2. Resolves Through DI| Service[Win32MemoryMapper Service]:::core
    Service -->|3. Executes Isolated PInvoke| NativeClass[NativeWin32 Boilerplate Helper]:::win32
    NativeClass -->|4. Native Call| Kernel[Windows kernel32.dll]
```
