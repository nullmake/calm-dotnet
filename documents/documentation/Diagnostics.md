---
title: Diagnostics & Exceptions - CALM
description: Guide to diagnostics, logging levels, OpenTelemetry metrics, exception hierarchy, and implementing the ICalmErrorObserver in CALM.
---

# Diagnostics & Exceptions

This section explains the exception hierarchy in CALM and diagnostic methods through the implementation of an error observer.

## 1. System Exception Hierarchy (`CalmException`)

All CALM-specific exceptions inherit from `CalmException`.

* **`CalmAffinityException`**:
  Detected by `VerifyContext()`. Thrown when accessing engine-thread-only data from an unsafe context (another thread).
* **`CalmEngineStoppingException`**:
  Occurs when attempting to schedule a new external task during the shutdown sequence (during `StopAsync`).
* **`CalmSchemaException`**:
  Thrown during the startup registration process if a `[CalmHandler]` method is generic, has an invalid signature (e.g., second argument is not `CancellationToken`), or an inappropriate return type.
* **`CalmHandlerAlreadyRegisteredException`**:
  Occurs when a handler for the same message is already registered for a Command or Query, violating uniqueness constraints.

## 2. Logging & Diagnostics

CALM outputs engine status and message throughput via `Microsoft.Extensions.Logging`.

### Log Content by Level

* **`LogLevel.Information` (Info)**:
  Outputs engine startup, graceful shutdown, and key lifecycle events. Production environments are typically set to this level or higher.
* **`LogLevel.Trace` (Verbose)**:
  Outputs detailed task scheduling, handler execution start/end, and dispatch information for every message. Extremely useful for debugging during development.

### Suppressing Logs (`[CalmSuppressLog]`)

To prevent high-frequency events (like dozens of progress notifications per second) from flooding your logs, you can suppress log output for a specific message by applying the `[CalmSuppressLog]` attribute to its definition.

```csharp
[CalmSuppressLog]
public record HeartbeatEvent(DateTime Timestamp) : ICalmEvent;
```

### Logger Configuration

#### Automatic Activation via DI

When registering the engine using `services.AddCalm()`, the `ILogger` within the DI container is automatically bound to the CALM engine.

#### Manual Configuration

If not using DI, pass an `ILogger` instance directly to the `CalmEngine` constructor.

```csharp
ILogger logger = loggerFactory.CreateLogger<CalmEngine>();
using var engine = new CalmEngine(options, logger);
```

#### Disabling Logs

To stop log output completely, disable it via `CalmOptions`.

```csharp
var options = new CalmOptions { EnableLogger = false };
```

## 3. OpenTelemetry Metrics Specification

The library automatically measures the following metrics via the `CalmTelemetry` class:

* **`calm.engine.queue_depth`** (UpDownCounter): Total number of tasks currently waiting in the message pump.
* **`calm.engine.processing_duration`** (Histogram/ms): Actual time taken to process individual task segments on the engine thread.
* **`calm.engine.messages_processed`** (Counter): Total number of messages successfully processed.

## 4. Error Observer Implementation Example

Implement `ICalmErrorObserver` in production environments to capture unexpected errors or long-term stalls.

```csharp
public class CalmErrorLogger : ICalmErrorObserver
{
    public void OnUnhandledException(Exception exception)
    {
        // Log critical unhandled handler exceptions
        Console.WriteLine($" Unhandled Exception: {exception.Message}");
    }

    public void OnStall(StallEventArgs e)
    {
        // Detect long-running tasks (suspected deadlock or heavy blocking I/O) exceeding threshold
        Console.WriteLine($" Task '{e.Task?.Name}' stalled. Duration: {e.Duration}");
    }

    public void OnContextLeaked()
    {
        // Detect misuse of ConfigureAwait(false), etc.
        Console.WriteLine($" SynchronizationContext leaked.");
    }
}
```

## Learn with Samples

* Details on error handling: [Sample05](https://github.com/nullmake/calm-dotnet/tree/main/samples/Sample05)
