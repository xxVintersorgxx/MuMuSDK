# Reactive Intent-Driven Presentation Architecture

This document defines the structural requirements for an asynchronous, message-driven Avalonia UI platform utilizing an Intent/Publish-Subscribe communication loop.

---

## 🧭 The Fire-and-Forget Intent Pipeline

To achieve complete isolation, the presentation layer utilizes an **Event-Driven Messaging Pattern**. The user interface never invokes low-level core execution tasks directly; instead, it acts as an un-opinionated message terminal that broadcasts structural intents and updates its display grids exclusively based on inbound background telemetry packets.

```text
  [ AVALONIA UI WINDOW ] ─── (Dispatches Fire-and-Forget) ───► [ SYSTEM MESSAGE BUS ]
           ▲                                                            │
           │ (Listens / Data-Binds Stream)                              ▼ (Triggers Worker)
  [ UI Thread State ]    ◄─── (Pushes Safe UI Strings) ───────── [ BACKGROUND WORKER ]
```

---

## 🧱 Intent and Event Message Contracts

The communication loop between the presentation window and the backend background thread workers is governed by explicit message models defined inside `MuMuSDK.Core.Interfaces`:

### 1. The Intent Channel (Uphill to Downhill)
Intents represent user actions dispatched from the UI view. They are immutable, fire-and-forget message payloads:
*   `StartIntrospectionIntent(string packageCommName)`: Requests initialization of the hypervisor layout scan.
*   `ExecutePointerCrawlIntent(ulong anchorGva, uint depth)`: Triggers the background brute-force crawler engine loop.

### 2. The Telemetry Stream (Downhill to Uphill)
Workers process intents completely out-of-process on background threads, transforming memory cells into text primitives, and streaming them back up to the UI:
*   `HypervisorStateEvent(bool isConnected, IntPtr lowerBaseHva, IntPtr upperBaseHva)`: Signals layout synchronization status.
*   `GameMetricPayload(string metricName, string formattedValue)`: Delivers real-time de-obfuscated values (e.g., Health, Coordinates) formatted safely as string primitives ready for instant UI data-binding.

---

## 🛡️ Presentation Safety Invariants

!!! failure "Absolute Reactive UI Invariants"
    *   **The Command Dispatch Isolation Rule:** Avalonia click event handlers (`ReactiveCommand`) are strictly restricted to calling `.Publish()` or `.Send()` on the underlying system message bus. They are forbidden from awaiting a long-running memory translation task.
    *   **The Thread Context Dispatcher Mandate:** Inbound data events streaming from background workers must pass explicitly through the Avalonia thread scheduler (`Dispatcher.UIThread.Post`) before touching UI-bound properties to prevent cross-thread rendering exceptions.
