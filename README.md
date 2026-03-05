# CALM.NET (Cooperative Async Lock-free Messaging)

CALM.NET introduces a cooperative single-thread model to the .NET ecosystem, designed specifically to eliminate race conditions and concurrency hazards in asynchronous programming. By enforcing a serialized execution boundary, the library provides a high-performance environment for managing complex state without the cognitive load or performance overhead associated with manual synchronization primitives.

## 🚀 Features

* **Single-thread guarantee**:
  Enables thread-safe state mutation without synchronization primitives. By confining logic to a single-threaded execution context, developers can eliminate locks and avoid the deadlocks or contested resources typical of multi-threaded shared state.

* **Execution Modes**:
  Offers granular control over the execution context. Developers can choose to run handlers on a dedicated background thread, the current execution thread, or the UI thread, ensuring compatibility with various application architectures.

* **Zero-Configuration**:
  Leverages C# Source Generators to automate infrastructure concerns. By decorating classes and methods with specific attributes, the library handles metadata discovery and registration at compile time, reducing boilerplate and runtime reflection.

* **CQRS Messaging**:
  Enforces a clear architectural separation between Commands, Queries, and Events. This feature includes an in-memory outbox to ensure consistency; events are only dispatched upon the successful execution of a command, preventing partial failures where state is updated but subsequent notifications are lost.

* **Robust Monitoring**:
  Includes a built-in Watchdog service for proactive stall detection. The system provides detailed diagnostic information, allowing architects to identify and resolve processing bottlenecks or blocked execution contexts effectively.

## 🛠 Usage

### 1. Handler Definition

To define a message handler, decorate the target class with the [CalmAutoRegister] attribute and the specific processing method with the [CalmHandler] attribute. This metadata allows the Source Generator to identify and wire the business logic into the messaging pipeline.

```csharp
[CalmAutoRegister]
public class AccountHandler
{
    [CalmHandler]
    public async Task HandleAsync(DepositCommand command)
    {
        // Business logic execution within a single-threaded boundary.
        // State mutation is safe here without 'lock' statements.
        await _repository.UpdateBalance(command.AccountId, command.Amount);
    }
}
```

### 2. Setup (DI Integration)

Integration with the .NET Dependency Injection container is streamlined. The Source Generator produces extension methods for IServiceCollection that automatically register all attributed handlers identified during compilation. To configure the container, invoke the generated registration hook during the application startup phase.

```csharp
var builder = Host.CreateApplicationBuilder();
builder.Services.AddCalm();
builder.Services.AddCalmGeneratedHandlers(); // 自動登録

var host = builder.Build();
await host.RunAsync();
```

### 3. Sending Messages

Once the handlers are registered, developers interact with the system via a central dispatcher or bus interface. This component acts as the mediator, routing Commands, Queries, and Events to the appropriate execution mode and handler. The dispatcher ensures that messages are queued and processed according to the single-thread guarantee, maintaining the integrity of the application state.

```csharp
// Command
await messaging.CommandAsync(new DepositCommand(100));

// Query
var balance = await messaging.QueryAsync(new GetBalanceQuery());

// Event
await messaging.PublishAsync(new BalanceUpdatedEvent(balance));
```

## License

This project is licensed under the Apache License 2.0. See the LICENSE file for details.

## Acknowledgments

This project is developed with the assistance of AI (Google Gemini).
