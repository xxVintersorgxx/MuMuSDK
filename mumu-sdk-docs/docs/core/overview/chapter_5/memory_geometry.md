# Ch.5.1 - Memory Geometry Analysis

## 1.1 The Out-of-Process Task Discovery Hurdle
Because the Linux kernel shifts its structural storage boundaries slightly upon every clean boot sequence, the engine implements a signature routine to isolate the root execution context dynamically.

## 1.2 Targeting the Process Chain Anchor
The root of all scheduling records inside a Linux system maps to process ID 0, the `swapper` task/`init_task`. This specific descriptor is statically compiled directly inside the high-memory kernel data sections.

!!! hint "Isolating the Anchor Node completely from Windows"
    The SDK sweeps the high-memory boundary space (`0xFFFF...`) looking for the unique 8-byte ASCII character tracking array: `s w a p p e r \0`. 
    Once located, it verifies that the signed integer tracking the process ID reads exactly `0`, locking onto the head node of the system process chain.

## 1.3 C# Process Search Engine Iterations
Once the `init_task` block anchor is locked, your C# SDK code loops through the doubly-linked process array lists out-of-process to find your target target package framework:

```csharp
public static class TargetProcessLocator
{
    /// <summary>
    /// Audits the active kernel process matrix lists to locate specific execution package references.
    /// </summary>
    public static IntPtr FindActiveAppNode(IntPtr initTaskHva, string packageCommName, IntPtr upperBaseHva)
    {
        IntPtr currentTask = initTaskHva;

        while (currentTask != IntPtr.Zero)
        {
            // 1. Extract Process Comm Tag Name string from current task node
            string commName = ReadCommString(currentTask + 0x6A0);
            
            if (commName.Equals(packageCommName, StringComparison.OrdinalIgnoreCase))
            {
                // Target discovered! Return the active task descriptor block address.
                return currentTask;
            }

            // 2. Fetch adjacent task node link from the linked-list tracker at offset 0x30
            ulong nextTaskGpa = MemoryReader.ReadUInt64(currentTask + 0x30);
            
            // 3. Resolve extracted pointer back to target Host Virtual Address mapping coordinates
            currentTask = HypervisorRouter.TranslateGpaToHva(nextTaskGpa, IntPtr.Zero, upperBaseHva) - 0x30;
        }

        return IntPtr.Zero;
    }
}
```
