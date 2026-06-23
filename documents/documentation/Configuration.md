---
title: Configuration Properties - CALM
description: Reference guide for configuring CALM, including key options like capacity and watchdog threshold, and how to apply them directly or via dependency injection.
---

# Configuration Properties

List of configuration properties for customizing CALM's behavior.

| Property | Type | Default | Description |
| :--- | :--- | :--- | :--- |
| **`Capacity`** | `int` | `10000` | The upper limit of the internal message queue. When reached, enqueuing from external threads is blocked to prevent deadlocks, **except for enqueuing from the engine thread itself.** |
| **`WatchdogThreshold`** | `TimeSpan` | `5.0s` | The threshold for determining when a task has monopolized (stalled) the engine thread. |
| **`TimeProvider`** | `TimeProvider` | `TimeProvider.System` | The time provider used for timers and timestamps, facilitating easy test mocking. |
| **`EnableLogger`** | `bool` | `true` | Whether to enable pipeline operations and structured log output. |
| **`ErrorObserver`** | `ICalmErrorObserver?` | `null` | An observer that receives notifications of exceptions, stalls, and context leaks. |

## How to Specify Configuration

### 1. Direct Instantiation

Pass a `CalmOptions` object to the `CalmEngine` constructor.

```csharp
var options = new CalmOptions 
{
    Capacity = 5000,
    WatchdogThreshold = TimeSpan.FromSeconds(10)
};
using var engine = new CalmEngine(options);
```

### 2. Using Dependency Injection (DI)

You can describe settings within the arguments of the `AddCalm` method.

```csharp
services.AddCalm(options => 
{
    options.Capacity = 5000;
    options.EnableLogger = true;
});
```
