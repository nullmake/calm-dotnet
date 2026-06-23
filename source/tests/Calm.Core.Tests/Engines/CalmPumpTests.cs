using Calm.Core.Engines.Contexts;
using Calm.Core.Engines.SynchronizationContexts;
using Microsoft.Extensions.Logging;
using Moq;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Calm.Core.Tests.Engines;

/// <summary>
/// Provides tests for the core engine execution pump.
/// </summary>
[SuppressMessage("Design", "MA0042:Do not use blocking calls in an async method",
    Justification = "Test Patterns Using Dispose.")]
public class CalmPumpTests() : TestBase(LogLevel.Trace)
{
    /// <summary>
    /// Verifies that Isolated mode executes tasks on a dedicated thread.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task IsolatedModeExecutesOnDedicatedThread()
    {
        var TestCtxCT = TestContext.Current.CancellationToken;

        // Arrange
        var mock = new Mock<ICalmErrorObserver>();
        using (var engine = CreateCalmEngine(mock))
        {
            engine.Start();
            int? executionThreadId = null;

            // Act
            await engine.ExecuteAsync(_ =>
            {
                executionThreadId = Environment.CurrentManagedThreadId;
                return Task.CompletedTask;
            }, TestCtxCT);

            // Assert
            Assert.NotNull(executionThreadId);
            Assert.NotEqual(Environment.CurrentManagedThreadId, (int)executionThreadId);
        }
        mock.Verify(x => x.OnUnhandledException(It.IsAny<Exception>()), Times.Never);
        mock.Verify(x => x.OnContextLeaked(), Times.Never);
    }

    /// <summary>
    /// Verifies that sequential execution maintains the order of posted tasks.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task SequentialExecutionMaintainsOrder()
    {
        var TestCtxCT = TestContext.Current.CancellationToken;

        // Arrange
        var mock = new Mock<ICalmErrorObserver>();
        using (var engine = CreateCalmEngine(mock))
        {
            engine.Start();
            var results = new List<int>();
            const int count = 100;

            // Act
            var tasks = new List<Task>();
            for (int i = 0; i < count; i++)
            {
                int val = i;
                tasks.Add(engine.ExecuteAsync(_ =>
                {
                    results.Add(val);
                    return Task.CompletedTask;
                }, TestCtxCT));
            }

            await Task.WhenAll(tasks);

            // Assert
            Assert.Equal(count, results.Count);
            for (int i = 0; i < count; i++)
            {
                Assert.Equal(i, results[i]);
            }
        }
        mock.Verify(x => x.OnUnhandledException(It.IsAny<Exception>()), Times.Never);
        mock.Verify(x => x.OnContextLeaked(), Times.Never);
    }

    /// <summary>
    /// Verifies that the engine sets a custom SynchronizationContext on its execution thread.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task SetsCustomSynchronizationContext()
    {
        var TestCtxCT = TestContext.Current.CancellationToken;

        // Arrange
        var mock = new Mock<ICalmErrorObserver>();
        using (var engine = CreateCalmEngine(mock))
        {
            engine.Start();
            SynchronizationContext? capturedContext = null;

            // Act
            await engine.ExecuteAsync(_ =>
            {
                capturedContext = SynchronizationContext.Current;
                return Task.CompletedTask;
            }, TestCtxCT);

            // Assert
            Assert.NotNull(capturedContext);
            Assert.IsType<CalmSynchronizationContext>(capturedContext);
        }
        mock.Verify(x => x.OnUnhandledException(It.IsAny<Exception>()), Times.Never);
        mock.Verify(x => x.OnContextLeaked(), Times.Never);
    }

    /// <summary>
    /// Verifies that ExecuteAsync returns the result from the function.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task ExecuteAsyncShouldReturnResult()
    {
        var TestCtxCT = TestContext.Current.CancellationToken;

        // Arrange
        var mock = new Mock<ICalmErrorObserver>();
        using (var engine = CreateCalmEngine(mock))
        {
            engine.Start();

            // Act
            var result = await engine.ExecuteAsync(_ => Task.FromResult(42), TestCtxCT);

            // Assert
            Assert.Equal(42, result);
        }
        mock.Verify(x => x.OnUnhandledException(It.IsAny<Exception>()), Times.Never);
        mock.Verify(x => x.OnContextLeaked(), Times.Never);
    }

    /// <summary>
    /// Verifies that ExecuteAsync (Task-returning) returns the result from the function.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task ExecuteAsyncWithTaskShouldReturnResult()
    {
        var TestCtxCT = TestContext.Current.CancellationToken;

        // Arrange
        var mock = new Mock<ICalmErrorObserver>();
        using (var engine = CreateCalmEngine(mock))
        {
            engine.Start();

            // Act
            var result = await engine.ExecuteAsync(async _ =>
            {
                await Task.Yield();
                return 100;
            }, TestCtxCT);

            // Assert
            Assert.Equal(100, result);
        }
        mock.Verify(x => x.OnUnhandledException(It.IsAny<Exception>()), Times.Never);
        mock.Verify(x => x.OnContextLeaked(), Times.Never);
    }

    /// <summary>
    /// Verifies that ExecuteAsync (Task-returning, no result) can be awaited.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task ExecuteAsyncWithTaskShouldComplete()
    {
        var TestCtxCT = TestContext.Current.CancellationToken;

        // Arrange
        var mock = new Mock<ICalmErrorObserver>();
        using (var engine = CreateCalmEngine(mock))
        {
            engine.Start();
            bool executed = false;

            // Act
            await engine.ExecuteAsync(async _ =>
            {
                await Task.Yield();
                executed = true;
            }, TestCtxCT);

            // Assert
            Assert.True(executed);
        }
        mock.Verify(x => x.OnUnhandledException(It.IsAny<Exception>()), Times.Never);
        mock.Verify(x => x.OnContextLeaked(), Times.Never);
    }

    /// <summary>
    /// Verifies that exceptions in ExecuteAsync (Task-returning) are propagated.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task ExecuteAsyncWithTaskShouldPropagateException()
    {
        var TestCtxCT = TestContext.Current.CancellationToken;

        // Arrange
        var mock = new Mock<ICalmErrorObserver>();
        using (var engine = CreateCalmEngine(mock))
        {
            engine.Start();

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => engine.ExecuteAsync(async _ =>
            {
                await Task.Yield();
                throw new InvalidOperationException("Async failure");
            }, TestCtxCT));
        }
        mock.Verify(x => x.OnUnhandledException(It.IsAny<Exception>()), Times.Never);
        mock.Verify(x => x.OnContextLeaked(), Times.Never);
    }

    /// <summary>
    /// Verifies that the current task metadata is correctly captured.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task MetadataShouldBeCaptured()
    {
        var TestCtxCT = TestContext.Current.CancellationToken;

        // Arrange
        var mock = new Mock<ICalmErrorObserver>();
        using (var engine = CreateCalmEngine(mock))
        {
            engine.Start();
            CalmTaskInfo? capturedMetadata = null;

            // Act
            await engine.ExecuteAsync(_ =>
            {
                capturedMetadata = CalmContext.CurrentTask;
                return Task.CompletedTask;
            }, TestCtxCT);

            // Assert
            Assert.NotNull(capturedMetadata);
            Assert.Equal(nameof(MetadataShouldBeCaptured), capturedMetadata.Name);
            Assert.EndsWith("CalmPumpTests.cs", capturedMetadata.FilePath, StringComparison.Ordinal);
            Assert.True(capturedMetadata.LineNumber > 0);
        }
        mock.Verify(x => x.OnUnhandledException(It.IsAny<Exception>()), Times.Never);
        mock.Verify(x => x.OnContextLeaked(), Times.Never);
    }

    /// <summary>
    /// Verifies that Schedule executes the task.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task ScheduleShouldExecuteTask()
    {
        var TestCtxCT = TestContext.Current.CancellationToken;

        // Arrange
        var mock = new Mock<ICalmErrorObserver>();
        using (var engine = CreateCalmEngine(mock))
        {
            engine.Start();
            var tcs = new TaskCompletionSource<bool>();

            // Act
            engine.Schedule(_ =>
            {
                tcs.SetResult(true);
                return Task.CompletedTask;
            }, TestCtxCT);

            // Assert
            await tcs.Task;
            Assert.True(await tcs.Task);
        }
        mock.Verify(x => x.OnUnhandledException(It.IsAny<Exception>()), Times.Never);
        mock.Verify(x => x.OnContextLeaked(), Times.Never);
    }

    /// <summary>
    /// Verifies that Schedule with delay executes the task after the delay.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task ScheduleWithDelayShouldExecuteTask()
    {
        var TestCtxCT = TestContext.Current.CancellationToken;

        // Arrange
        var mock = new Mock<ICalmErrorObserver>();
        using (var engine = CreateCalmEngine(mock))
        {
            engine.Start();
            var start = DateTimeOffset.UtcNow;

            // Act
            var operation = engine.Schedule(
                _ => Task.CompletedTask,
                TimeSpan.FromMilliseconds(100),
                TestCtxCT);

            // Assert
            await operation.CompletedAwaitable.ConfigureAwait(true);
            var duration = DateTimeOffset.UtcNow - start;
            Assert.True(duration >= TimeSpan.FromMilliseconds(100));
        }
        mock.Verify(x => x.OnUnhandledException(It.IsAny<Exception>()), Times.Never);
        mock.Verify(x => x.OnContextLeaked(), Times.Never);
    }

    /// <summary>
    /// Verifies that SwitchAsync correctly switches to the engine thread.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task SwitchAsyncShouldSwitchToEngineThread()
    {
        // Arrange
        var mock = new Mock<ICalmErrorObserver>();
        using (var engine = CreateCalmEngine(mock))
        {
            engine.Start();

            // Act
            Assert.False(engine.IsOnEngineThread);
            await engine.SwitchAsync();

            // Assert
            Assert.True(engine.IsOnEngineThread);
        }
        mock.Verify(x => x.OnUnhandledException(It.IsAny<Exception>()), Times.Never);
        mock.Verify(x => x.OnContextLeaked(), Times.Never);
    }

    /// <summary>
    /// Verifies that VerifyContext throws exception when called from wrong thread.
    /// </summary>
    [Fact]
    public void VerifyContextShouldThrowOnWrongThread()
    {
        // Arrange
        var mock = new Mock<ICalmErrorObserver>();
        using (var engine = CreateCalmEngine(mock))
        {
            engine.Start();

            // Act & Assert
            Assert.Throws<CalmAffinityException>(() => engine.VerifyContext());
        }
        mock.Verify(x => x.OnUnhandledException(It.IsAny<Exception>()), Times.Never);
        mock.Verify(x => x.OnContextLeaked(), Times.Never);
    }

    /// <summary>
    /// Verifies that VerifyContext does not throw when called from engine thread.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task VerifyContextShouldNotThrowOnCorrectThread()
    {
        var TestCtxCT = TestContext.Current.CancellationToken;

        // Arrange
        var mock = new Mock<ICalmErrorObserver>();
        using (var engine = CreateCalmEngine(mock))
        {
            engine.Start();

            // Act & Assert
            await engine.ExecuteAsync(_ =>
            {
                engine.VerifyContext();
                return Task.CompletedTask;
            }, TestCtxCT);
        }
        mock.Verify(x => x.OnUnhandledException(It.IsAny<Exception>()), Times.Never);
        mock.Verify(x => x.OnContextLeaked(), Times.Never);
    }

    /// <summary>
    /// Verifies that reaching capacity triggers backpressure (Wait in WriteAsync).
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task BackpressureTest()
    {
        var TestCtxCT = TestContext.Current.CancellationToken;

        // Arrange
        var mock = new Mock<ICalmErrorObserver>();
        // Use a small capacity to trigger backpressure quickly.
        using (var engine = CreateCalmEngine(mock, o => o.Capacity = 1))
        {
            engine.Start();

            // Block the engine thread.
            engine.Schedule(async _ =>
            {
                using (BlockLog("1"))
                {
                    Sleep(5000);
                }
            }, TestCtxCT);

            // Queue second task.
            engine.Schedule(_ =>
            {
                using (BlockLog("2"))
                {
                    return Task.CompletedTask;
                }
            }, TestCtxCT);

            // Third task should block.
            var tcs = new TaskCompletionSource<bool>();
            var postTask = Task.Run(() =>
            {
                tcs.SetResult(true);
                engine.Schedule(_ =>
                {
                    using (BlockLog("3"))
                    {
                        return Task.CompletedTask;
                    }
                }, TestCtxCT);

            }, TestCtxCT);

            // Wait a bit to ensure it's blocked.
            await tcs.Task;
            await Task.Delay(1000, TestCtxCT);
            Assert.False(postTask.IsCompleted);
            await postTask;
        }
        mock.Verify(x => x.OnUnhandledException(It.IsAny<Exception>()), Times.Never);
        mock.Verify(x => x.OnContextLeaked(), Times.Never);
    }

    /// <summary>
    /// Verifies that ExecuteWithContextAsync preserves CalmContext.
    /// CurrentTask across await points (fixes AsyncLocal leakage).
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task ExecuteAsync_WithContextAsyncLocalLeak_Reproduce()
    {
        var mock = new Mock<ICalmErrorObserver>();
        await using (var engine = CreateCalmEngine(mock))
        {
            engine.Start();
            CalmTaskInfo? contextBeforeAwait = null;
            CalmTaskInfo? contextAfterAwait = null;

            await engine.ExecuteAsync(async _ =>
            {
                contextBeforeAwait = CalmContext.CurrentTask;
                await Task.Yield(); // Forces continuation segment
                contextAfterAwait = CalmContext.CurrentTask;
            }, TestContext.Current.CancellationToken);

            Assert.NotNull(contextBeforeAwait);
            Assert.NotNull(contextAfterAwait);
            Assert.Equal(contextBeforeAwait.Id, contextAfterAwait.Id);
        }
        mock.Verify(x => x.OnUnhandledException(It.IsAny<Exception>()), Times.Never);
        mock.Verify(x => x.OnContextLeaked(), Times.Never);
    }

    /// <summary>
    /// Verifies that ScheduleOperation.CompletedAwaitable completes
    /// (propagating the exception) when an exception occurs inside the task.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Schedule_ExceptionDoesNotCompleteAwaitable_Reproduce()
    {
        var mock = new Mock<ICalmErrorObserver>();
        await using (var engine = CreateCalmEngine(mock))
        {
            engine.Start();

            // Act
            var operation = engine.Schedule(
                _ => throw new InvalidOperationException("Test exception"),
                TestContext.Current.CancellationToken);

            // Assert
            // We expect the await on CompletedAwaitable to throw the exception.
            // If the bug is present, it will hang indefinitely, so we use a timeout of 2 seconds to fail fast.
            var awaitTask = Task.Run(async () => await operation.CompletedAwaitable, TestContext.Current.CancellationToken);
            var completed = await Task.WhenAny(awaitTask, Task.Delay(2000, TestContext.Current.CancellationToken));

            if (completed == awaitTask)
            {
                var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () => await awaitTask);
                Assert.Equal("Test exception", exception.Message);
            }
            else
            {
                Assert.Fail("CompletedAwaitable hung and timed out because the exception was not propagated.");
            }
        }
        mock.Verify(x => x.OnUnhandledException(It.IsAny<InvalidOperationException>()), Times.Once);
        mock.Verify(x => x.OnContextLeaked(), Times.Never);
    }
}
