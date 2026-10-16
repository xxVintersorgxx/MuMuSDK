# Dynamic Memory Allocation & Environment Size Mapping

This document specifies the extraction pipelines used to translate dynamic guest hardware parameters into unmanaged host page filters.

---

## 🧭 Invariant Design Principles

*   **Zero Constant Assumptions:** Host virtual allocation sizing parameters must derive entirely from verified `.nemu` asset profiles at runtime.
*   **Dynamic Range Scaling:** The linear page sweeper (`VirtualQueryEx`) must filter memory regions using variables provided dynamically by the pre-attachment discovery engine.
