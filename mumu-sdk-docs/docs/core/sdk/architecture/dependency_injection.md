\# Dependency Injection (DI) Core Registration Matrix



This document profiles the structural lifetimes and container registration boundaries enforced across the introspection platform.



\---



\## 🧭 Container Invariant Lifetimes



To prevent lifecycle leaks, state corruption, or memory pinning collisions, services are split into explicit operational tiers:



1\.  \*\*Singleton Layer:\*\* Global state configuration pools, disk persistence workers, and multi-threaded background synchronization clock ticker lines.

2\.  \*\*Transient / Scoped Layer:\*\* Standalone overlay popup views, dynamic plugin dialog panels, and user-initiated analytical diagnostic runners.



\---



\## 🗂️ Dependency Resolution Contract Tree



```mermaid

flowchart TD

&#x20;   %% Define Styles \& Colors for High-Contrast Dark Mode

&#x20;   classDef singleton fill:#2A3542,stroke:#3A4A5A,stroke-width:2px,color:#88CCFF;

&#x20;   classDef transient fill:#3F2A42,stroke:#5A3A5A,stroke-width:2px,color:#FF88FF;

&#x20;   classDef boot fill:#25252D,stroke:#3A3A45,stroke-width:2px,color:#FFFFFF;



&#x20;   Boot\[Application Boot] -->|ConfigureServices| Coll\[IServiceCollection]

&#x20;   Coll -->|BuildServiceProvider| Provider\[IServiceProvider]



&#x20;   subgraph Singleton Lifetimes \[Global Single Instance]

&#x20;       AppSettings\[AppSettings Model]:::singleton

&#x20;       MainViewModel\[MainViewModel Hub]:::singleton

&#x20;   end



&#x20;   subgraph Transient Lifetimes \[Fresh Instance Per Request]

&#x20;       MainWindow\[MainWindow Shell]:::transient

&#x20;       PluginManagerDialog\[PluginManagerDialog Box]:::transient

&#x20;   end



&#x20;   Provider -->|GetRequiredService| AppSettings

&#x20;   AppSettings -->|Injected Into| MainViewModel

&#x20;   Provider -->|Resolve Shell UI| MainWindow

&#x20;   MainWindow -->|DataBound Context| MainViewModel

&#x20;   

&#x20;   Boot:::boot

```

