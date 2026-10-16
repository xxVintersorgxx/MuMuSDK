# Windows Native Interop Helper Class Layout

This document profiles the structural boundaries, unmanaged permission structures, and platform invocation signatures contained inside the isolated `NativeWin32` helper utility class.

---

## 🧭 Invariant Encapsulation Bounds

*   **File Isolation Mandate:** All P/Invoke bindings, structures (`MEMORY_BASIC_INFORMATION`), and access permission keys are strictly encapsulated within `NativeWin32.cs`.
*   **Zero Logic Pollution:** Upstream memory scanning engines do not declare raw interop constants locally. They interact exclusively via the static properties exposed by the helper utility class.
