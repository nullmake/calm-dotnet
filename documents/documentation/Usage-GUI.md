---
title: GUI Application Integration - CALM
description: Learn how to integrate CALM with WPF and Windows Forms GUI applications, handle thread synchronization, and manage handler lifecycles.
---

# GUI Application Integration

This section explains implementing patterns and precautions when using CALM in GUI applications such as WPF or Windows Forms.

## 1. Relationship between GUI and CALM Threads

GUI applications have a "UI Thread (Main Thread)" where all button clicks and screen drawing occur. Conversely, CALM operates on its own "Engine Thread."

- **Requests from UI to CALM**: Call `ICalm.Command.SendAsync`, etc., upon button clicks or other user interactions.
- **Notifications from CALM to UI**: Reflect results received in CALM handlers (Events) back to the screen.

## 2. Thread Considerations within Handlers

CALM handlers (methods marked with `[CalmHandler]`) are always executed on the **CALM Engine Thread**. To directly manipulate GUI controls, you must marshal the process back to the UI thread.

### Implementation Example in WPF

In WPF, for simple property updates via `INotifyPropertyChanged`, the WPF binding mechanism automatically absorbs thread inconsistencies. In many cases, simply assigning the value is sufficient.

```csharp
[CalmHandler]
private Task HandleUpdatedEvent(MyEvent @event, CancellationToken token)
{
    // Executed on CALM thread, but WPF Binding dispatches the notification to the UI thread
    this.StatusText = @event.Message;
    return Task.CompletedTask;
}
```

### Implementation Example in Windows Forms

In Windows Forms, manipulating control properties from a different thread is prohibited. Use `Invoke` or Reactive Extensions (Rx) `ObserveOn` to explicitly return to the UI thread.

```csharp
[CalmHandler]
private Task HandleUpdatedEvent(MyEvent @event, CancellationToken token)
{
    // Return to UI thread to safely update WinForms controls
    mainForm.Invoke(() => {
        lblStatus.Text = @event.Message;
    });
    return Task.CompletedTask;
}
```

## 3. Handler Registration and Lifecycle Management

In GUI applications, it's crucial to register and unregister handlers according to the lifetime of screens (Windows or Forms) or ViewModels.

### Lifecycle Management Patterns

#### 1. Management via DI Container (ViewModels, etc.)

If you use a DI container to register ViewModels, use `AddScopedCalmHandlersFromClass<T>`. In this case, **the DI container automatically performs the registration with CALM when the instance is created, so you do not need to call Register manually.**

#### 2. Manual Registration (Screen classes, etc.)

When defining handlers directly in screen classes (Window or Form), perform registration and unregistration manually.

**⚠️ IMPORTANT: Do NOT Register in Constructor**
Calling `_calm.Register(this)` within a constructor causes the object to be referenced externally (by CALM) before its initialization is complete, leading to a "this-reference leak." Always register in events like `Load` or in methods called after initialization.

```csharp
public partial class MyForm : Form
{
    private readonly ICalm _calm;

    public MyForm(ICalm calm)
    {
        InitializeComponent();
        _calm = calm;
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        // Register when the screen is displayed (after initialization is complete)
        _calm.Register(this);
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        // Always unregister when closing the screen (to prevent memory leaks)
        _calm.Unregister(this);
        base.OnFormClosing(e);
    }

    [CalmHandler]
    public Task OnSomethingHappened(MyEvent e, CancellationToken ct) { ... }
}
```

## 4. Reference Sample Code

For more advanced implementations (such as combining with Rx or DI integration), refer to the following sample projects:

- **WPF Sample ([Sample04](https://github.com/nullmake/calm-dotnet/tree/main/samples/Sample04))**:
  - Modern MVVM pattern integration using `CommunityToolkit.Mvvm`.
  - Automatic registration via `AddScopedCalmHandlersFromClass`.
- **WinForms Sample ([Calm.Sample.Winforms](https://github.com/nullmake/calm-dotnet/tree/main/samples/Calm.Sample.Winforms))**:
  - Advanced thread control and binding using ReactiveUI.
  - Synchronizing complex collections (`SourceCache`) with the UI thread.
