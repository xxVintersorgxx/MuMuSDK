# Logging & Diagnostic Infrastructure Specification

This document maps the architectural requirements for the unified core logging extension engine running natively within the .NET 10 introspection solution framework.

---

## 🧭 Functional Logging Objectives
*   Capture low-level hypervisor status updates, bit-shift milestones, and memory mapping exceptions cleanly across all projects.
*   Support a **Runtime Dynamically Settable Log Filter Level** via an on-the-fly dropdown selector (ranging from absolute blackout mode to high-frequency verbosity).
*   Provide an **Independent Source-Muting Matrix** allowing the user to uncheck and isolate specific code modules completely.
*   **Decouple Text Formatting Entirely:** Eliminate string interpolation at the call-site. Services emit raw data structures; formatting is handled downstream via a specialized extension module.
*   Capture detailed **Call-Site Metadata** automatically via compile-time injection attributes.

---

## 🧱 Non-Allocating Structured Payload Specification

To completely eliminate call-site string creation overhead, logging endpoints accept raw data message templates. The log event is transmitted as a thin primitive package:

```csharp
public readonly record struct LogEventRecord(
    DateTime Timestamp,          // Precise clock capture time
    LogLevel Level,              // Severity flag
    string SourceComponent,      // Assembly module or Namespace identity
    string MessageTemplate,      // Invariant string format skeleton (e.g., "Walked PGD index {0} to GPA {1}")
    object[] PayloadArgs,        // Raw, un-rendered data parameters (ulong, int, etc.)
    string ExceptionDetails,     // Optional stacktrace data for Error/Fatal lines
    string CallingFile,          // Filename via [CallerFilePath]
    string CallingMethod,        // Method name via [CallerMemberName]
    int CallingLine              // Line number via [CallerLineNumber]
);
```

---

## 🎛️ Decoupled Formatting Pipeline & Short-Circuit Rules

The text rendering block is extracted completely away from your real-time processing threads. String compilation runs downhill inside a **Dedicated Formatting Extension** only *after* filtering boundaries are cleared:

```text
  [ High-Frequency Page Walker ] ──► Emits MessageTemplate & Raw Primitives
                                            │
                                            ▼ (Step 1: Atomic Filter Validation)
                                   Is Level Checked & Source Unmuted?
                                            ├── NO  ──► Abort Instantly (Zero String Heap Allocations)
                                            └── YES ──► Route down to Log Broker
                                                            │
                                                            ▼ (Step 2: Downstream Isolated Extension)
                                                [ LogFormatter Extension ]
                                                Processes string.Format() on background worker thread
                                                            │
                                                            ▼ (Step 3: Render Destination)
                                                Pushes formatted text to Console, File, or UI
```

---

## ⚠️ Architectural Invariants & Performance Guardrails

!!! failure "Absolute Formatting & Performance Invariants"
    *   **The Invariant Call-Site Allocation Prohibition:** Developers and AI engines are strictly forbidden from writing string interpolation blocks (`$"GVA: {gva}"`) inside high-frequency processing methods. Messages must be passed as static templates, shifting string formatting tasks completely to the background.
    *   **Atomic Filter Short-Circuit:** If a component fires an entry that is blocked by the active global dropdown level or an unchecked source checkbox, the runtime must drop the execution stack **before** any array boxing or object allocation occurs.
    *   **Isolated Formatter Responsibility:** The text layout string formatting extension is completely self-contained. It takes the immutable `LogEventRecord` data, combines the message template with its primitive arguments, prepends the call-site source signatures (`[MmuEmulator.cs:Line 42]`), and pushes a single, finalized string line to your console or your Avalonia UI diagnostics monitor panel.
