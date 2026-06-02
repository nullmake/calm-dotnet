# Quickstart Guide

This guide describes the steps to introduce CALM into your project and safely launch your first single-threaded task.

## 1. Add Package

Install CALM via NuGet. CALM supports the following platforms:

- **.NET 8.0 / 10.0+**
- **.NET Standard 2.0 / 2.1**
- **.NET Framework 4.7.2+**

```bash
dotnet add package Calm.Core
```

## 2. Minimal Configuration (Hello World)

This is the simplest flow: initializing, starting, executing a task, and stopping the `CalmEngine`.

```csharp
using Calm.Core;
using System;
using System.Threading.Tasks;

// 1. Create engine instance
// CalmEngine is the standard entry point implementing the ICalm interface.
using var engine = new CalmEngine();

// 2. Start the message pump
// Calling Start() launches a dedicated background thread to begin processing enqueued tasks.
engine.Start();

// 3. Execute a task safely on the engine thread
// ExecuteAsync guarantees that the specified process runs on CALM's dedicated thread.
await engine.ExecuteAsync(async (cancellationToken) =>
{
    Console.WriteLine($"Thread ID: {Environment.CurrentManagedThreadId}");
    Console.WriteLine($"Is on Engine Thread?: {engine.IsOnEngineThread}"); // True
    await Task.CompletedTask;
});

// 4. Thread Switching (SwitchAsync Pattern)
// Awaiting SwitchAsync() automatically transfers subsequent code execution to the CALM thread.
Console.WriteLine($"Main Thread ID: {Environment.CurrentManagedThreadId}");
await engine.SwitchAsync();
Console.WriteLine($"Switched Thread ID: {Environment.CurrentManagedThreadId}");

// 5. Graceful Shutdown
// Wait for all tasks to complete before terminating the thread.
await engine.StopAsync();
```

## Why use ICalm / CalmEngine?

When using standard `Task.Run` or the thread pool, you must be careful of "Race Conditions" where multiple threads access the same data simultaneously.

By using CALM, it is guaranteed that **"all processing is executed sequentially on a single dedicated thread,"** allowing you to safely manipulate state (such as variables) without using complex `lock` statements.

## Learn with Samples

For more practical basic operations, refer to [Sample01](https://github.com/nullmake/calm-dotnet/tree/main/samples/Sample01).
