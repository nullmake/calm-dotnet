[![License](https://img.shields.io/badge/License-Apache_2.0-blue.svg)](LICENSE)
[![GitHub Actions](https://img.shields.io/github/actions/workflow/status/nullmake/calm-dotnet/documents.yml?label=Docs)](https://nullmake.github.io/calm-dotnet/)
[![CodeQL](https://github.com/nullmake/calm-dotnet/actions/workflows/github-code-scanning/codeql/badge.svg)](https://github.com/nullmake/calm-dotnet/actions/workflows/github-code-scanning/codeql)

![CALM Logo](./documents/images/calm-logo-light.svg)

# CALM

CALM (**C**ooperative **A**sync **L**ock-free **M**essaging) is a high-performance, lightweight execution engine for .NET designed to simplify concurrent programming.

## 💡 Motivation

Standard `async/await` in C# is powerful, but it doesn't automatically prevent deadlocks or race conditions. These bugs are notoriously hard to reproduce and debug. 

Previously, I tried to solve this by running `yield return` coroutines on a single thread. While it ensured thread safety, it hit two major walls:
1. **Structural limitations**: `try/catch` blocks didn't work across yield points.
2. **Concurrency limitations**: It couldn't efficiently handle IO-bound work concurrently (interleaving).

**CALM solves these challenges** by running standard `async` methods on a single dedicated message pump.
- **Lock-free Safety**: By serializing all execution, it eliminates race conditions and deadlocks.
- **Resource Efficient**: No thread pool churn; no need for complex semaphore management to limit resource contention.
- **Predictable**: It provides a "Calm" development experience where you focus on logic, not synchronization.

## 🚀 Key Features

- **Guaranteed Single-Threading**: All operations within an engine instance are serialized, providing an actor-like thread-safety model.
- **Resource Efficiency**: Minimizes thread pool consumption by processing all tasks on a single dedicated thread, preventing thread pool starvation even under high task volumes.
- **Deadlock Elimination**: By serializing all tasks through a high-performance `Channel`-based message pump, it removes the need for traditional locks.
- **Role-based Messaging**: Built-in support for Commands, Queries, and Events with a simple, attribute-based handler discovery.
- **DI Ready**: Seamlessly integrates with `Microsoft.Extensions.DependencyInjection` and supports automatic handler registration from assemblies.
- **Multi-Target Support**: Compatible with .NET Framework 4.7.2, .NET Standard 2.0/2.1, and .NET 8/10.

## 📦 Packages

| Package | Nuget |
| :------ | :---- |
|**Calm.Core**<br/>The core high-performance execution engine and messaging abstractions.|[![NuGet](https://img.shields.io/nuget/v/Calm.Core)](https://www.nuget.org/packages/Calm.Core)|
|**Calm.Extensions.DependencyInjection**<br/>Seamless integration with `Microsoft.Extensions.DependencyInjection`.|[![NuGet](https://img.shields.io/nuget/v/Calm.Extensions.DependencyInjection)](https://www.nuget.org/packages/Calm.Extensions.DependencyInjection)|

## 📦 Installation

Install the core package via NuGet:

```bash
dotnet add package Calm.Core
```

If you are using Dependency Injection (recommended), install the extensions:

```bash
dotnet add package Calm.Extensions.DependencyInjection
```

## ⚡ Quick Start

### 1. Define your Messages

Implement `ICalmCommand`, `ICalmQuery<T>`, or `ICalmEvent`.

```csharp
public record GreetCommand(string Name) : ICalmCommand;
public record GetVersionQuery() : ICalmQuery<string>;
```

### 2. Implement Handlers

Decorate your methods with the `[CalmHandler]` attribute.

```csharp
public class GreetingService
{
    [CalmHandler]
    public Task HandleAsync(GreetCommand command, CancellationToken ct)
    {
        Console.WriteLine($"Hello, {command.Name}!");
        return Task.CompletedTask;
    }

    [CalmHandler]
    public Task<string> HandleAsync(GetVersionQuery query, CancellationToken ct)
    {
        return Task.FromResult("1.0.0");
    }
}
```

### 3. Setup and Execution (with DI)

```csharp
var services = new ServiceCollection();

// Add CALM and register handlers from the assembly
services.AddCalm();
services.AddCalmHandlersFromAssembly(ServiceLifetime.Singleton, typeof(GreetingService).Assembly);

var provider = services.BuildServiceProvider();

// Start the engine
var calm = provider.GetRequiredService<ICalm>();
calm.Start();

// Send a command
await calm.Command.SendAsync(new GreetCommand("World"));

// Send a query
var version = await calm.Query.SendAsync(new GetVersionQuery());
```

## ⚠️ Important Safety Rules

To maintain the safety guarantees provided by CALM, please adhere to these rules:

1. **Avoid `.ConfigureAwait(false)`**: Within CALM handlers or tasks, always stay on the engine thread. Using `false` can leak the execution to the thread pool, leading to race conditions.
2. **Do NOT Register in Constructors**: Never call `Register(this)` in a constructor. This leads to a "this-reference leak" where an uninitialized object is exposed to the engine. Use a `Load` event or an explicit initialization method instead.
3. **Always Unregister**: To prevent memory leaks, ensure that instances registered with the engine are `Unregister()`ed or disposed of when no longer needed.

## 🎓 Learning Path with Samples

To help you get started with CALM, we have prepared a series of samples that guide you through its features step-by-step. You can find them in the [samples directory](./samples/).

| Step | Sample | Focus Area |
| :--- | :--- | :--- |
| 1 | **[Sample01](./samples/Sample01)** | **Basics**: Engine lifecycle, scheduling, and thread safety. |
| 2 | **[Sample02](./samples/Sample02)** | **Messaging**: Commands, Queries, Events, and Role-based Messaging. |
| 3 | **[Sample03](./samples/Sample03)** | **Integration**: .NET Generic Host and Dependency Injection. |
| 4 | **[Sample04](./samples/Sample04)** | **GUI**: WPF integration and MVVM pattern. |
| 5 | **[Sample05](./samples/Sample05)** | **Robustness**: Error handling, observers, and diagnostics. |
| 6 | **[WinForms](./samples/Calm.Sample.Winforms)** | **Real-world**: Practical usage with ReactiveUI and WinForms. |

See the [Samples README](./samples/README.md) for a detailed breakdown of each sample.

## 📚 Documentation

For detailed guides, advanced configurations, and architectural deep-dives, please refer to our [Documentation Site](https://nullmake.github.io/calm-dotnet/).

- [Quickstart Guide](https://nullmake.github.io/calm-dotnet/documentation/Quickstart.html)
- [Engine Operations & Thread Control](https://nullmake.github.io/calm-dotnet/documentation/Engine-Usage.html)
- [Role-based Messaging](https://nullmake.github.io/calm-dotnet/documentation/Usage-Messaging.html)
- [GUI Application Integration](https://nullmake.github.io/calm-dotnet/documentation/Usage-GUI.html)

## 📄 License

- **Documentation (`/documents`):** Licensed under the [Creative Commons Attribution 4.0 International License (CC BY 4.0)](doc/LICENSE.md).
- **All other contents:** Licensed under the [Apache License 2.0](LICENSE).
