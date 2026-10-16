\# UI Architecture Component Matrix \& Staging Pipeline



This document defines the physical layout barriers, separate assembly user controls, and manual staging execution tracks for the .NET 10 Avalonia UI Introspection platform.



\---



\## 🧭 Design Principles \& Separation Boundaries



To prevent architectural drift and isolate heavy background processing threads from UI presentation loops, the layout is broken into completely decoupled, separate operational components. 



&#x20;  View-State Agnosticism Every primary graphical component must exist inside its own self-contained UserControl assembly file.

&#x20;  Zero Pointer Ownership Graphical elements are strictly forbidden from capturing, holding, or marshaling raw memory pointers (`HVA``GPA``GVA`). They communicate exclusively via text primitives, numeric metrics, and fire-and-forget message intents.

&#x20;  Explicit Action Tracking Automated orchestration lines are barred. Memory access steps are bound to the manual execution steps on the staging ribbon to ensure clarity during live diagnostic verification sweeps.



\---



\## 🏗️ UI Component Layout Blueprints



\### 1. Manual Staging Ribbon (`StagingRibbonControl.axaml`)

A horizontal control bar pinned to the top of the interface window. It hosts the environmental configuration options, persistent path indicators, and manual memory alignment buttons.



&#x20;  Persistent Path Block Visual display tracking the stored `MuMuPlayer` home directory path, complete with a platform native file explorer selection launcher.

&#x20;  Configuration Fields An alphanumeric input textbox for the destination package identity string (e.g., `com.Target.Game`) and a dropdown mapping the engine frame clock refresh speed constraint (`50ms` - `100ms`).

&#x20;  The Execution Staging Sequence Contains individual action elements to advance memory alignment steps manually

&#x20;   1.  Attach Process Invokes Win32 natively targeting `MuMuNxDevice.exe`.

&#x20;   2.  Scan Memory Pools Launches linear allocation sweeps to isolate the `1.5625 GB` and `3.4375 GB` private virtual pages.

&#x20;   3.  Isolate Kernel Anchor Triggers the high-memory search loop looking for the invariant `swapper0` ASCII token.

&#x20;  Plugin Manager Switch A dedicated ribbon utility item that spawns the un-opinionated modal dialog box window to profile compiled extension modules.



\### 2. Verification Dashboard Sidebar (`VerificationDashboardControl.axaml`)

A vertical panel situated on the left side of the window workspace. It tracks low-level virtual machine diagnostics independently of active game loops.

&#x20;  Base Allocation Address Badges Non-editable string containers displaying the active Host Virtual Address anchors cached by the driver engine.

&#x20;  Kernel Signature Matrix Tree High-contrast graphic checkbox indicators providing visual confirmation as each Phase 2 initialization threshold passes verification.



\### 3. Real-time Telemetry Monitor Grid (`TelemetryMonitorControl.axaml`)

The main body dashboard panel. It renders structural metrics fed dynamically by downstream background workers.

&#x20;  Data Translation Column Arrays Maps variable labels, target Guest Physical Address (GPA) translation entries, hex byte views, and finalized string representations in high-contrast data rows.



\### 4. Low-Overhead Log Stream Terminal (`LogTerminalControl.axaml`)

The baseline terminal component designed to handle fast system messages with zero call-site allocation overhead.

&#x20;  Atomic Filter Dropdowns Changes severity filtering thresholds globally on the fly.

&#x20;  Namespace Muting Matrix Individual toggles to uncheck and silence whole core assembly libraries.

&#x20;  Text Viewing Canvas A high-performance text canvas area displaying cleanly formatted strings appended with compile-time source metadata.



\---



\## 🪟 Standalone Layout Modals Dynamic Plugin Dialog (`PluginManagerDialog.axaml`)



To keep the application hull lightweight and clean, plugin indexing operations run inside a decoupled dialog box shell.

&#x20;  Discovered Module Data Rows A grid listing all compiled target game modules loaded via dynamic assembly reflection. It parses information from the underlying `IGamePlugin` contract properties (`TargetProcessCommName`, `PluginVersionSignature`).

&#x20;  Hot Re-Sweep Trigger A button element allowing developers to execute quick folder sweeps if a new game plugin library is dropped straight into the target runtime space without having to restart the central desktop executable shell.



