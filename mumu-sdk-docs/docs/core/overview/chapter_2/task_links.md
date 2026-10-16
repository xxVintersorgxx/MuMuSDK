# Ch.2.2 - Task Link Enumeration

## 1.1 Walking the Task Array List
The `task_list` field handles scheduling execution metrics via a standard circular doubly-linked list (`struct list_head`). Because the entries form an unbroken loop, the engine must look for a static system anchor to start searching without encountering infinite loop index limits.

## 1.2 Finding the Kernel Signature (The "Swapper" Task Search)
Process ID 0 (known as the `swapper` task or `init_task`) is statically compiled directly into the kernel binary space. To isolate it dynamically entirely from the Windows host layer, the scanner sweeps high-memory blocks (`0xFFFF...`) for a unique identifier string signature:

!!! note "Kernel Signature Scanner Pipeline"
    ```mermaid
    graph TD
        A["Scan High-Memory Regions"] --> B["Verify Offset 0x6A0<br>Matches 'swapper\0'?"]
        B --> C["Verify Offset 0x4C8<br>Matches ProcessId == 0?"]
        C --> D["Isolate Anchor Node Pointer<br>Return Target GVA Anchor"]
    ```

## 1.3 The C# Task Enumeration Engine Logic
Once your C# framework locks onto this structural anchor node, it uses a non-intrusive loop to traverse the active process chains and identify target runtimes.

!!! abstract "C# Out-of-Process Task Chain Enumeration"
    ```csharp
    using System;
    using MuMuSDK.Core.Services; // (1)

    namespace MuMuSDK.Core.Services
    {
        public static class TaskChainEnumerator
        {
            /// <summary>
            /// Traverses the guest kernel doubly-linked task list to extract a target process node pointer.
            /// </summary>
            public static IntPtr FindTargetProcessNode(IntPtr anchorNode, string targetName, IntPtr upperBaseHva)
            {
                IntPtr currentNode = anchorNode;
                
                do
                {
                    // 1. Read process name character string buffer metrics
                    string currentCommName = ReadProcessCommName(currentNode);
                    
                    if (currentCommName.Equals(targetName, StringComparison.OrdinalIgnoreCase))
                    {
                        // Success: Target identifier discovered cleanly
                        return currentNode;
                    }

                    // 2. Extract next circular task entry pointer handle from offset 0x30
                    IntPtr guestPhysicalNext = ReadPointer(currentNode + 0x30);
                    
                    // 3. Convert extracted GPA handle to high-level Host Virtual Address via our native core service helper
                    currentNode = HypervisorRouter.TranslateGpaToHva((ulong)guestPhysicalNext, IntPtr.Zero, upperBaseHva);

                } while (currentNode != anchorNode);

                return IntPtr.Zero; // Context not discovered inside the current scheduling cycle
            }

            // Primitive placeholders matching your low-level verification drivers
            private static string ReadProcessCommName(IntPtr nodeHva) => throw new NotImplementedException();
            private static IntPtr ReadPointer(IntPtr addressHva) => throw new NotImplementedException();
        }
    }
    ```

    1. 🔍 Structural resolution helpers like `HypervisorRouter` reside safely in the internal invariant `MuMuSDK.Core.Services` namespace wrapper.
