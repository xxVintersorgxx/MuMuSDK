# Ch.1.3 - Layer C: Guest Android Kernel Physical Space

## 1.1 The Virtual-to-Physical Kernel Map
Layer C marks the boundary of the simulated operating system. The Android Linux kernel treats the hardware partition block it receives from the hypervisor as its entire pool of physical RAM.

Inside this space, the kernel manages a virtual memory configuration to isolate low-level system tracking files from user-mode applications:

!!! info "Guest Operating System Virtual Segmentation"
    ```mermaid
    graph LR
        subgraph GVA [Layer C: Guest Virtual Address Space Range]
            A["0x0000000000000000<br>User Space Area<br>Applications & Native Games"] --> L1["Hardware Virtual Gap Boundary"]
            L1 --> B["0x0000008000000000<br>Unmapped Space<br>Unallocated Memory Pool"]
            B --> L2["High Kernel Space Map"]
            L2 --> C["0xFFFF000000000000<br>Kernel Space Area<br>Drivers & Linked Task Lists"]
        end
    ```

## 1.2 Contiguous Aggregate Consolidation
Because an operating system requires a flat physical layout to initialize its hardware kernel, the guest RAM splits will consistently present as the single largest, contiguous allocations in the entire target process workspace. By grouping these massive, adjacent segments dynamically at launch, your SDK calculates any user memory modifications (whether the emulator is configured to 3GB, 5GB, or 8GB of RAM) completely automatically.
