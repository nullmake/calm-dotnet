---
title: Role-based Messaging - CALM
description: Comprehensive guide to defining messages (Commands, Queries, Events) and registering handlers within the CALM messaging system.
---

# Role-based Messaging

Learn the practical usage of the CALM messaging system (CalmBus), a core component of CALM.

## 1. Message Types and Roles

CALM uses three types of messages depending on their role. CALM acts as a **message broker** that mediates these messages and dispatches them appropriately to registered handlers.

| Type | Role | Dispatch Model | Return Value |
| :--- | :--- | :--- | :--- |
| **Command** | An instruction to "do something." Changes the persistent state of the system. | **1:1** (Point-to-Point) | None / Yes |
| **Query** | An inquiry to "tell me something." Retrieves data without changing state. | **1:1** (Request-Response) | Yes |
| **Event** | Notification of a fact that "something happened." Conveys information to multiple interested parties. | **1:Many** (Pub/Sub) | None |

### CALM as a Pub/Sub Broker

CALM decouples message senders (Publishers) from receivers (Subscribers/Handlers) by acting as an intermediary.

- Senders don't need to know "who processes the message." They just throw it to CALM.
- Receivers "register" themselves with CALM to be automatically invoked when a specific message arrives.

## 2. Defining Messages

Messages are defined as classes or records (recommended) implementing specific interfaces.

```csharp
using Calm.Core;
using Calm.Core.Messaging;

// Command (no return value)
public record CreateUserCommand(string Username, string Email) : ICalmCommand;

// Command (with return value)
public record ProcessPaymentCommand(decimal Amount) : ICalmCommand<PaymentResult>;
public record PaymentResult(bool Success, string TransactionId);

// Query (with return value, read-only)
public record GetUserQuery(string UserId) : ICalmQuery<UserDto>;
public record UserDto(string Username, string Email);

// Event (1:Many notification)
public record UserCreatedEvent(string Email) : ICalmEvent;
```

## 3. Creating Handlers and Attribute Definitions

A handler is implemented as a method that takes a message object as its first argument, a `CancellationToken` as its second, and returns `Task` or `Task<T>`. Always apply the `[CalmHandler]` attribute.

> **Note:** Lambda expressions cannot be used because attributes cannot be defined on them. They must be implemented as methods.

### CancellationToken Received by Handlers

The `CancellationToken` received as the second argument is a **composite of "the token passed by the user during the call" and "the CALM engine's own shutdown token."**
This ensures that asynchronous processing within the handler can be safely interrupted during either an application shutdown or a specific cancellation request.

```csharp
[CalmHandler]
public async Task HandleAsync(MyCommand command, CancellationToken ct)
{
    // This ct also becomes cancelled when the engine stops.
    await Task.Delay(1000, ct);
}
```

## 4. Handler Registration (Register) and Unregistration (Unregister)

Handlers must be registered with the engine to be active. Unused handlers should be explicitly unregistered.

### Won't be invoked without registration

While CALM uses reflection for automatic handler discovery, instance methods won't respond to messages unless the instance is `Register()`ed.

### Failure to unregister causes memory leaks

Registered instances are held internally by the CALM engine. If you don't call `Unregister()` when they are no longer needed (e.g., when a screen is closed), they won't be collected by the Garbage Collector (GC), leading to memory leaks or unexpected behavior.

### ⚠️ WARNING: Do NOT Register in Constructor (this-reference leak)

Calling `_engine.Register(this)` within a constructor causes the object to be referenced externally (by CALM) before its initialization is complete, leading to "this-reference leak" bugs.

Always register either from an external call after object creation/initialization or by utilizing DI container features.

**Recommended Implementation Pattern:**

```csharp
public class MyHandler : IDisposable
{
    private readonly ICalm _engine;

    public MyHandler(ICalm engine)
    {
        _engine = engine;
        // ⚠️ Do NOT call Register(this) here
    }

    // Call explicitly after initialization is complete
    public void Initialize()
    {
        _engine.Register(this);
    }

    [CalmHandler]
    public Task OnEvent(MyEvent e, CancellationToken ct) => Task.CompletedTask;

    public void Dispose()
    {
        // Unregister from the engine (ensure this is called)
        _engine.Unregister(this);
    }
}
```

## 5. Integration with Dependency Injection (DI)

In modern .NET applications, it's common to manage CALM using `Microsoft.Extensions.DependencyInjection`.

### Registering Services

```csharp
// 1. Register the CALM engine
services.AddCalm();

// 2. Automatically register classes with handlers from specified assemblies
// * Typically, services and repositories are registered en masse as Singleton or Scoped.
services.AddCalmHandlersFromAssembly(ServiceLifetime.Singleton, typeof(Program).Assembly);
```

### Considerations for UI (WPF/WinForms)

While `AddCalmHandlersFromAssembly` registers all classes in an assembly, **"Screens (Windows or Forms)"** might not be suitable for automatic DI registration.

For UI components using CALM handlers, we recommend individual `Register()` calls in each screen's constructor (after initialization) and `Unregister()` (or `Dispose`) when the screen is closed, rather than batch registration.

- Reference: [Concrete implementation example in WPF (Sample04)](https://github.com/nullmake/calm-dotnet/tree/main/samples/Sample04)

## 6. Outbox Pattern (Unit of Work) Behavior

CALM employs the **Outbox Pattern (Unit of Work)** to maintain data integrity.

### Background: Why is the Outbox necessary?

When performing "database updates" and "issuing completion events" within a command handler, if the DB update succeeds but the event fails, or if only the event is sent and the DB update is rolled back, the entire system becomes inconsistent.

In CALM, calling `Event.Publish()` within a handler doesn't send the event immediately; it is buffered and sent **immediately after the handler completes successfully (returns).** If an exception occurs within the handler, all events accumulated in the meantime are discarded, ensuring atomic behavior.

### Immediate Publishing (`[CalmImmediate]`)

For events that "must be notified immediately during processing" and "regardless of the overall success or failure" (such as progress updates or real-time logs), you may need to bypass the Outbox.

Applying the `[CalmImmediate]` attribute to an event definition ensures that handlers are executed as soon as `Publish()` is called.

```csharp
// This event is delivered immediately upon Publish()
[CalmImmediate]
public record ProgressUpdatedEvent(int Percent) : ICalmEvent;
```

## Learn with Samples

- Basics of role-based messaging: [Sample02](https://github.com/nullmake/calm-dotnet/tree/main/samples/Sample02)
- Integration into .NET Generic Host: [Sample03](https://github.com/nullmake/calm-dotnet/tree/main/samples/Sample03)
