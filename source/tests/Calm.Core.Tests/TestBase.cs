using Calm.Core.Engines;
using Calm.Core.Tests.TestClasses;
using Microsoft.Extensions.Logging;
using Moq;
using SharedTestCode;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Calm.Core.Tests;

/// <summary>
/// The base class for all tests in this project, providing common setup and utilities.
/// </summary>
[SuppressMessage("Maintainability", "CA1515:Consider making public types internal",
    Justification = "Test classes and related classes must be public.")]
public abstract class TestBase
{
    /// <summary>
    /// The test output helper used to write test output during execution.
    /// </summary>
    protected ILogger Logger { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TestBase"/> class.
    /// </summary>
    /// <param name="level">The test output log level</param>
    protected TestBase(LogLevel level)
    {
        Logger = LoggerHelper.CreateLogger(level, GetType().Name);
    }

    /// <summary>
    /// Logs output at the start and end of a block.
    /// </summary>
    /// <param name="blockName">The block name.</param>
    /// <returns>The log output object.</returns>
    protected IDisposable BlockLog(string blockName)
    {
        Logger.LogInformation("({Name}) -->", blockName);
        return new ActionDisposable(() => Logger.LogInformation("<-- ({Name})", blockName));
    }

    /// <summary>
    /// Creates a standard <see cref="CalmEngine"/> with a mocked error observer for testing.
    /// </summary>
    /// <param name="mock">The mock for the error observer.</param>
    /// <returns>A new instance of <see cref="CalmEngine"/>.</returns>
    protected CalmEngine CreateCalmEngine(Mock<ICalmErrorObserver> mock)
        => CreateCalmEngine(mock, null);

    /// <summary>
    /// Creates a standard <see cref="CalmEngine"/> with a mocked error observer for testing.
    /// </summary>
    /// <param name="mock">The mock for the error observer.</param>
    /// <param name="configure">An optional action to configure the <see cref="CalmOptions"/>.</param>
    /// <returns>A new instance of <see cref="CalmEngine"/>.</returns>
    protected CalmEngine CreateCalmEngine(Mock<ICalmErrorObserver> mock, Action<CalmOptions>? configure)
    {
        var options = new CalmOptions
        {
            ErrorObserver = mock?.Object,
        };

        configure?.Invoke(options);

        return new CalmEngine(options, Logger);
    }

    /// <summary>
    /// Wait for the engine is idle that is no tasks are currently executing.
    /// </summary>
    /// <param name="engine">The calm engine.</param>
    /// <param name="token">The cancel token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="InvalidOperationException">Failed to get Calm non-public instance.</exception>
    /// <exception cref="ArgumentNullException">The engine parameter is null.</exception>
    protected async Task WaitForIdleAsync(CalmEngine engine, CancellationToken token)
    {
        _ = engine ?? throw new ArgumentNullException(nameof(engine));

        Logger.LogTrace("[WaitForIdleAsync] Starting.");

        // Wait for the posted action to be processed.
        await engine.ExecuteAsync(async _ =>
        {
            var operation = await engine.ScheduleAsync(_ => Task.CompletedTask, token);
            await operation.StartedAwaitable;
        }, token);

        // Wait until the calm tasks are finished.
        var pump = typeof(CalmEngine).GetField("_pump", BindingFlags.NonPublic | BindingFlags.Instance)?
            .GetValue(engine)
            ?? throw new InvalidOperationException();

        var result = typeof(CalmPump)
            .GetMethod("WaitForShutdownCalmTaskAsync", BindingFlags.NonPublic | BindingFlags.Instance)?
            .Invoke(pump, [token])
            ?? throw new InvalidOperationException();

        await (Task)result;
        Logger.LogTrace("[WaitForIdleAsync] Finished.");
    }

    /// <summary>
    /// Create instance of the given type.
    /// </summary>
    /// <param name="type">The type of instance to be create.</param>
    /// <param name="engine">The calm engine.</param>
    /// <returns>The instance of the given type.</returns>
    /// <exception cref="InvalidOperationException">Failed to create instance.</exception>
    private protected ITestClass CreateInstance(Type type, CalmEngine engine)
        => Activator.CreateInstance(type, engine, Logger) as ITestClass
            ?? throw new InvalidOperationException($"Failed to create instance of the `{type?.Name}`");

    /// <summary>
    /// Suspends the current thread for the specified amount of time.
    /// </summary>
    /// <param name="millisecondsTimeout">The number of milliseconds for which the thread is suspended. </param>
    protected static void Sleep(int millisecondsTimeout)
    {
        Thread.Sleep(millisecondsTimeout);
    }

    /// <summary>
    /// A helper pattern to ensure <see cref="SynchronizationContext.Current"/> and <see cref="AsyncLocal{T}"/> values are restored.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">The action parameter is null.</exception>
    protected async Task AssertContextRestorationAsync(Func<Task> action)
    {
        _ = action ?? throw new ArgumentNullException(nameof(action));

        var originalContext = SynchronizationContext.Current;
        var testLocal = new AsyncLocal<string> { Value = "before" };

        try
        {
            Logger.LogInformation("Before thread:{Id}", Environment.CurrentManagedThreadId);

            await action();
        }
        finally
        {
            Logger.LogInformation("After thread:{Id}", Environment.CurrentManagedThreadId);

            // Verify SynchronizationContext is restored exactly
            Xunit.Assert.Same(originalContext, SynchronizationContext.Current);

            // Verify AsyncLocal value is preserved (not leaked or lost)
            Xunit.Assert.Equal("before", testLocal.Value);
        }
    }
}
