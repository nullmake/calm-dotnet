---
title: Engine Operations & Thread Control - CALM
description: Master task execution, scheduling, thread affinity, and thread switching via SwitchAsync inside the CALM execution engine.
---

# Engine Operations & Thread Control

This section explains how to operate the `ICalm` interface, the core of the CALM engine, and the important rules for maintaining thread safety.

## 1. Task Execution on Engine (`ExecuteAsync`)

`ExecuteAsync` guarantees that the specified delegate (process) is executed on CALM's dedicated engine thread. Always access data (state) on the engine thread through this method.

```csharp
// Execute on the engine thread and wait for the result
var result = await engine.ExecuteAsync(async ct =>
{
    // This is the engine thread
    return "Done";
});
```

## 2. Task Scheduling (`Schedule`)

`Schedule` or `ScheduleAsync` is used to "reserve (schedule)" a task for the engine thread. It has the following characteristics:

- **Non-blocking**: It enqueues the task and returns control immediately without waiting for its completion.
- **Background Tasks**: By running a `while` loop within a scheduled task, you can easily implement persistent background processes.
- **Fire-and-Forget**: Allows "fire-and-forget" execution for processes that don't need to be awaited (e.g., logging, notifications), either immediately or after a specified delay.

### Background Loop Example

Ideal for tasks you want to keep running, such as periodic monitoring or data processing.

```csharp
// Reserve a task that runs a loop in the background (returns immediately)
engine.Schedule(async ct =>
{
    while (!ct.IsCancellationRequested)
    {
        // Wait 1 second (async wait to avoid blocking the engine thread)
        await Task.Delay(1000, ct);

        // Process to run periodically
        Console.WriteLine("Monitoring in background...");
    }
});
```

### Fire-and-Forget / Delayed Execution Example

Used for processes that don't need completion waiting or should run slightly later.

```csharp
// 1. Immediate execution (fire-and-forget)
engine.Schedule(async ct =>
{
    Log.Info("Processing accepted.");
});

// 2. Execution after a specified time (delayed execution)
engine.Schedule(async ct =>
{
    Console.WriteLine("Executed after 3 seconds.");
}, TimeSpan.FromSeconds(3));
```

## 3. Prohibition of ConfigureAwait(false)

In CALM's thread control, using `.ConfigureAwait(false)` within tasks is **strictly prohibited** in principle.

### Why is it prohibited?

While recommended for performance in standard .NET development, CALM ensures safety by "remaining on the engine thread."
Using `.ConfigureAwait(false)` may move subsequent processing to the general-purpose thread pool, losing thread affinity (the property where only a specific thread can touch specific data) and causing data inconsistency.

Always maintain the context (or use `.ConfigureAwait(true)`) within handlers and tasks.

## 4. Returning to Thread (`SwitchAsync`)

If the execution thread has left the engine thread due to external library calls (e.g., HttpClient or DB drivers), you can immediately return to the engine thread by calling `SwitchAsync()`.

```csharp
await someExternalLibrary.DoWorkAsync().ConfigureAwait(false); // Thread might change

// Return to CALM engine thread
await engine.SwitchAsync();

// Safely access engine-thread-only data again
```

## 5. Thread Affinity Verification (`VerifyContext`)

`VerifyContext()` is provided to check if a method is "truly being called from the engine thread." It is recommended to call this at the beginning of internal methods dedicated to the engine thread to discover inappropriate access early.

```csharp
private void UpdateInternalState()
{
    // Throws CalmAffinityException if called from outside the engine thread
    _engine.VerifyContext();

    _counter++;
}
```

## Learn with Samples

- Overview of basic operations: [Sample01](https://github.com/nullmake/calm-dotnet/tree/main/samples/Sample01)
