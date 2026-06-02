using Microsoft.Extensions.Logging;
using Moq;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Calm.Core.Tests.Engines;

/// <summary>
/// Provides tests for engine backpressure behavior.
/// </summary>
[SuppressMessage("Design", "MA0042:Do not use blocking calls in an async method",
    Justification = "Test Patterns Using Dispose.")]
[SuppressMessage("Roslynator", "RCS1046:Asynchronous method name should end with 'Async'",
    Justification = "Test case methods are excluded.")]
public class BackpressureTests() : TestBase(LogLevel.Trace)
{
    /// <summary>
    /// Verifies that Post from an external thread blocks when the engine queue is full.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task ExternalPostBlocksWhenQueueFull()
    {
        var TestCtxCT = TestContext.Current.CancellationToken;

        // Arrange
        var mock = new Mock<ICalmErrorObserver>();
        using (var engine = CreateCalmEngine(mock, options => { options.Capacity = 10; }))
        {
            engine.Start();

            // Fill the queue
            using var blockSignal = new ManualResetEventSlim(false);
            var cancellationToken = TestCtxCT;
            engine.Schedule(_ =>
            {
                blockSignal.Wait(cancellationToken);
                return Task.CompletedTask;
            }, TestCtxCT); // This item blocks the pump

            for (int i = 0; i < engine.Options.Capacity; i++)
            {
                engine.Schedule(_ => Task.CompletedTask, TestCtxCT); // Fill the remaining slots
            }

            // Act & Assert
            var postTask = Task.Run(
                () => engine.Schedule(_ => Task.CompletedTask, TestCtxCT),
                TestCtxCT);

            // Should be blocked
            await Task.Delay(200, TestCtxCT);
            Assert.False(postTask.IsCompleted);

            // Release
            blockSignal.Set();
            await postTask;
            Assert.True(postTask.IsCompleted);
        }
        mock.Verify(x => x.OnUnhandledException(It.IsAny<Exception>()), Times.Never);
        mock.Verify(x => x.OnContextLeaked(), Times.Never);
    }

    /// <summary>
    /// Verifies that Post from an internal thread does not throw an exception when the engine queue is full.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Necessary for testing unexpected exceptions")]
    public async Task InternalPostDoesNotThrowWhenQueueFull()
    {
        var TestCtxCT = TestContext.Current.CancellationToken;

        // Arrange
        var mock = new Mock<ICalmErrorObserver>();
        const int capacity = 10;
        using (var engine = CreateCalmEngine(mock, options => { options.Capacity = capacity; }))
        {
            engine.Start();

            int executedCount = 0;

            // Act
            var operation = engine.Schedule(async _ =>
            {
                // Fill the queue and exceed it from inside the engine thread.
                // This should not throw CalmQueueFullException.
                for (int i = 0; i < capacity + 5; i++)
                {
                    engine.Schedule(_ =>
                    {
                        Interlocked.Increment(ref executedCount);
                        return Task.CompletedTask;
                    }, TestCtxCT);
                }
                await Task.CompletedTask;
            }, TestCtxCT);

            await operation.CompletedAwaitable;

            // Assert
            // Wait for all enqueued tasks to execute.
            var start = DateTimeOffset.UtcNow;
            while (Volatile.Read(ref executedCount) < capacity + 5 && (DateTimeOffset.UtcNow - start).TotalSeconds < 5)
            {
                await Task.Delay(100, TestCtxCT);
            }

            Assert.Equal(capacity + 5, Volatile.Read(ref executedCount));
        }
        mock.Verify(x => x.OnUnhandledException(It.IsAny<Exception>()), Times.Never);
        mock.Verify(x => x.OnContextLeaked(), Times.Never);
    }

    /// <summary>
    /// Verifies that VerifyContext throws an exception when called from the wrong thread.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Necessary for testing unexpected exceptions")]
    public async Task VerifyContextThrowsOnWrongThread()
    {
        var TestCtxCT = TestContext.Current.CancellationToken;

        // Arrange
        var mock = new Mock<ICalmErrorObserver>();
        using (var engine = CreateCalmEngine(mock))
        {
            engine.Start();

            // Act & Assert
            Assert.Throws<CalmAffinityException>(() => engine.VerifyContext());

            var tcs = new TaskCompletionSource<bool>();
            engine.Schedule(_ =>
            {
                try
                {
                    engine.VerifyContext();
                    tcs.SetResult(true);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
                return Task.CompletedTask;
            }, TestCtxCT);

            await tcs.Task; // Should not throw
        }
        mock.Verify(x => x.OnUnhandledException(It.IsAny<Exception>()), Times.Never);
        mock.Verify(x => x.OnContextLeaked(), Times.Never);
    }
}
