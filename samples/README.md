# Samples

This directory contains a collection of sample applications designed to help you get started with CALM.

## Overview

Each sample focuses on a specific aspect or integration of the CALM engine. You can use these as a reference to understand how to structure your applications, handle threading, implement patterns, and integrate with external libraries.

---

## Recommended Learning Path

We recommend learning in the following order if you are new to CALM:

1. **Understand the Basics (Sample01):**
    Learn the fundamentals of the CALM engine's lifecycle and thread management. This is the foundation for everything else.
2. **Learn Role-based Messaging (Sample02):**
    Master message-driven design using Commands, Queries, and Events, and learn how to manage task execution safely.
3. **Adopt Modern Integration Techniques (Sample03 & Sample04):**
    Learn how to configure modern DI using .NET Generic Host and how to integrate with GUI applications like WPF.
4. **Enhance Robustness (Sample05):**
    Learn error handling and diagnostic techniques to build production-ready applications.
5. **Hands-on Practice (Calm.Sample.Winforms):**
    Combine everything you've learned to examine more practical application configurations using ReactiveUI in a Windows Forms environment.

---

## Sample Details

## Sample01: Basic Engine Usage

This sample demonstrates the fundamental operations of the CALM engine and the rules for maintaining thread safety.

- **Key Topics:**
  - **Engine Lifecycle:** Manually managing `CalmEngine` initialization via `Start()` and graceful shutdown via `StopAsync()`.
  - **Task Scheduling:** Using `Schedule()` to run long-running loops or fire-and-forget tasks on the dedicated CALM engine thread.
  - **Thread Synchronization:** Using `ExecuteAsync()` to enqueue work onto the engine thread and correctly using `SwitchAsync()` to return to the engine's execution context.
  - **Safety Rules:** Understanding why `.ConfigureAwait(false)` is prohibited and how to use `VerifyContext()` to detect thread affinity violations.

---

## Sample02: Role-based Messaging

This sample illustrates how to implement the Role-based Messaging pattern (Commands, Queries, and Events) within the CALM engine.

- **Key Topics:**
  - **Message Types:** Implementing `ICalmCommand`, `ICalmQuery`, and `ICalmEvent` for clear separation of concerns.
  - **Message Handling:** Using the `[CalmHandler]` attribute to define message handlers and understanding the composite `CancellationToken`.
  - **Lifecycle Management:** Preventing "this-reference leaks" by registering handlers after initialization and ensuring proper resource disposal using `Unregister(this)`.
  - **Outbox Pattern:** Understanding how CALM ensures atomic event publishing through the built-in Unit of Work.

---

## Sample03: .NET Generic Host Integration

This sample shows how to integrate CALM with the standard .NET Generic Host (`Microsoft.Extensions.Hosting`).

- **Key Topics:**
  - **DI Integration:** Using `Calm.Extensions.DependencyInjection` to register the CALM engine and handlers in the .NET DI container.
  - **Background Hosting:** Implementing `BackgroundService` to host a CALM-based service that automatically manages the engine's lifecycle and handler discovery.

---

## Sample04: WPF & MVVM Toolkit

This sample demonstrates a modern GUI application built on top of WPF.

- **Key Topics:**
  - **UI Integration:** Integrating the CALM engine into the WPF application lifecycle and managing thread marshaling between the UI and Engine threads.
  - **Architectural Patterns:** Utilizing the `CommunityToolkit.Mvvm` (MVVM Toolkit) for the presentation layer.
  - **Scoped Handlers:** Using `AddScopedCalmHandlersFromClass<T>` to manage ViewModel-based handlers via the DI container.

---

## Sample05: Error Handling & Diagnostics

This sample provides a comprehensive guide on error handling and diagnostics in CALM, focusing on the `ICalmErrorObserver` and logging.

- **Key Topics:**
  - **Error Observer:** Implementing `ICalmErrorObserver` to globally capture unhandled exceptions, engine stalls, and context leaks.
  - **Logging:** Configuring `Microsoft.Extensions.Logging` integration and using `[CalmSuppressLog]` to manage log verbosity.
  - **Metrics:** Understanding the OpenTelemetry metrics automatically collected by the engine.

---

## Calm.Sample.Winforms

A more complex WinForms sample illustrating the integration of reactive UI patterns with the CALM engine.

- **Key Topics:**
  - **Reactive UI:** Integrating `ReactiveUI` for property change notifications and event handling within a WinForms application.
  - **Advanced DI:** Combining complex service dependencies and manual handler registration within the WinForms infrastructure.
