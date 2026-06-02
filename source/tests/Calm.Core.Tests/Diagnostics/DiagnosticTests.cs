using Microsoft.Extensions.Logging;
using Moq;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Calm.Core.Tests.Diagnostics;

/// <summary>
/// Provides diagnostic and safety tests for the engine.
/// </summary>
[SuppressMessage("Design", "MA0042:Do not use blocking calls in an async method",
    Justification = "Test Patterns Using Dispose.")]
public class DiagnosticTests() : TestBase(LogLevel.Trace)
{
    /// <summary>
    /// Verifies that SwitchAsync completes synchronously if already on the engine thread.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task SwitchAsyncFastPathIfAlreadyOnThread()
    {
        var TestCtxCT = TestContext.Current.CancellationToken;

        // Arrange
        var mock = new Mock<ICalmErrorObserver>();
        await using (var engine = CreateCalmEngine(mock))
        {
            engine.Start();
            bool executedDirectly = false;

            // Act
            await engine.ExecuteAsync(async _ =>
            {
                await engine.SwitchAsync();
                executedDirectly = true;
                return Task.CompletedTask;
            }, TestCtxCT);

            // Assert
            Assert.True(executedDirectly);
        }
        mock.Verify(x => x.OnUnhandledException(It.IsAny<Exception>()), Times.Never);
        mock.Verify(x => x.OnContextLeaked(), Times.Never);
    }

    /// <summary>
    /// Verifies that SwitchAsync reschedules the continuation if not on the engine thread.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task SwitchAsyncReschedulesIfNotOnThread()
    {
        var TestCtxCT = TestContext.Current.CancellationToken;

        // Arrange
        var mock = new Mock<ICalmErrorObserver>();
        using (var engine = CreateCalmEngine(mock))
        {
            engine.Start();
            int? engineThreadId = null;
            int? continuationThreadId = null;

            await (await engine.ScheduleAsync(_ =>
            {
                engineThreadId = Environment.CurrentManagedThreadId;
                return Task.CompletedTask;
            }, TestCtxCT)).CompletedAwaitable;

            // Act
            await engine.SwitchAsync();
            continuationThreadId = Environment.CurrentManagedThreadId;

            // Assert
            Assert.Equal(engineThreadId, continuationThreadId);
        }
        mock.Verify(x => x.OnUnhandledException(It.IsAny<Exception>()), Times.Never);
        mock.Verify(x => x.OnContextLeaked(), Times.Never);
    }

    /// <summary>
    /// Verifies that unhandled exceptions in the pump are forwarded to the global error observer.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GlobalErrorHandlingForwardsToObserver()
    {
        var TestCtxCT = TestContext.Current.CancellationToken;

        // Arrange
        var mock = new Mock<ICalmErrorObserver>();
        using (var engine = CreateCalmEngine(mock))
        {
            engine.Start();
            var exception = new InvalidOperationException("Test exception");
            var tcs = new TaskCompletionSource<bool>();

            mock
                .Setup(x => x.OnUnhandledException(It.IsAny<Exception>()))
                .Callback((Exception ex) =>
                {
                    if (string.Equals(ex.Message, exception.Message, StringComparison.Ordinal))
                    {
                        tcs.TrySetResult(true);
                    }
                });

            // Act
            await engine.ScheduleAsync(_ => throw exception, TestCtxCT);

            var resultTask = await Task.WhenAny(tcs.Task, Task.Delay(5000, TestCtxCT));

            // Assert
            Assert.Same(tcs.Task, resultTask);
        }
        mock.Verify(x => x.OnUnhandledException(It.IsAny<InvalidOperationException>()), Times.AtLeastOnce());
        mock.Verify(x => x.OnContextLeaked(), Times.Never);
    }

    /// <summary>
    /// Verifies that long-running tasks are detected by the engine.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task LongRunningTaskShouldBeDetected()
    {
        var TestCtxCT = TestContext.Current.CancellationToken;

        // Arrange
        var mock = new Mock<ICalmErrorObserver>();
        var stallDetectedTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        mock
            .Setup(x => x.OnStall(It.IsAny<StallEventArgs>()))
            .Callback((StallEventArgs e) =>
            {
                if (e.Duration >= TimeSpan.FromMilliseconds(300))
                {
                    stallDetectedTcs.TrySetResult(true);
                }
            });

        await using (var engine = CreateCalmEngine(mock, opt =>
        {
            opt.WatchdogThreshold = TimeSpan.FromMilliseconds(300);
        }))
        {
            engine.Start();

            // Act: Deliberately block the engine thread for longer than the threshold
            await engine.ExecuteAsync(_ =>
            {
                Sleep(1000);
                return Task.CompletedTask;
            }, TestCtxCT);

            // Assert: Wait for long-running task to be detected
            await Task.WhenAny(
                stallDetectedTcs.Task,
                Task.Delay(3000, TestCtxCT));

            Assert.True(stallDetectedTcs.Task.IsCompleted);
        }
        mock.Verify(x => x.OnStall(It.IsAny<StallEventArgs>()), Times.AtLeastOnce());
        mock.Verify(x => x.OnContextLeaked(), Times.Never);
    }
}
