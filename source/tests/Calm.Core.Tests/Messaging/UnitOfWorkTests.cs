using Calm.Core.Tests.TestClasses;
using Calm.Core.Tests.TestClasses.Messages;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Calm.Core.Tests.Messaging;

/// <summary>
/// Provides tests for Unit of Work (UoW) behavior in complex scenarios.
/// </summary>
public class UnitOfWorkTests() : TestBase(LogLevel.Trace)
{
    /// <summary>
    /// Verifies that Schedule called from a message handler starts a new independent Unit of Work.
    /// </summary>
    /// <returns>A task.</returns>
    [Fact]
    public async Task ScheduleFromHandlerShouldStartNewUnitOfWork()
    {
        var TestCtxCT = TestContext.Current.CancellationToken;

        // Arrange
        var mock = new Mock<ICalmErrorObserver>();
        await using (var engine = CreateCalmEngine(mock))
        {
            engine.Start();
            using var handler = new TestClass(engine, Logger);
            engine.Register(handler);

            var eventReceived = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            // Act
            // 1. Send initial command
            await engine.Command.SendAsync(new TestCommand("Root", (instance, ct) =>
            {
                _ = instance;

                // 2. Inside the handler, schedule a task
                engine.Schedule(async innerCt =>
                {
                    // 3. Inside the scheduled task, send another command that publishes an event
                    await engine.Command.SendAsync(new TestCommand("Scheduled", (innerInstance, innerInnerCt) =>
                    {
                        _ = innerInstance;

                        // 4. Publish an event. 
                        // If this is correctly in a NEW Root UoW, it should be flushed when this command finishes.
                        // If it's incorrectly nested in the "Root" command's UoW, it will NEVER be flushed 
                        // because the "Root" command has already finished.
                        engine.Event.Publish(new TestEvent("FromScheduled", (eventInstance, eventCt) =>
                        {
                            _ = eventInstance;
                            _ = eventCt;
                            eventReceived.TrySetResult(true);
                            return Task.CompletedTask;
                        }), innerInnerCt);
                        return Task.CompletedTask;
                    }), innerCt);
                }, ct);
                return Task.CompletedTask;
            }), TestCtxCT);

            // Wait for the event to be received with a timeout
            var completedTask = await Task.WhenAny(
                eventReceived.Task,
                Task.Delay(2000, TestCtxCT));

            // Assert
            Assert.True(completedTask == eventReceived.Task, "Event was not received from Schedule. Outbox was likely not flushed.");
        }
        // Verify that no unhandled exceptions were reported to the observer
        mock.Verify(x => x.OnUnhandledException(It.IsAny<Exception>()), Times.Never);
        mock.Verify(x => x.OnContextLeaked(), Times.Never);
    }

    /// <summary>
    /// Verifies that ExecuteAsync called from a message handler starts a new independent Unit of Work.
    /// </summary>
    /// <returns>A task.</returns>
    [Fact]
    public async Task ExecuteAsyncFromHandlerShouldStartNewUnitOfWork()
    {
        var TestCtxCT = TestContext.Current.CancellationToken;

        // Arrange
        var mock = new Mock<ICalmErrorObserver>();
        await using (var engine = CreateCalmEngine(mock))
        {
            engine.Start();
            using var handler = new TestClass(engine, Logger);
            engine.Register(handler);

            var eventReceived = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            // Act
            await engine.Command.SendAsync(new TestCommand("Root", async (instance, ct) =>
            {
                _ = instance;
                await engine.ExecuteAsync(async innerCt =>
                {
                    await engine.Command.SendAsync(new TestCommand("Executed", (innerInstance, innerInnerCt) =>
                    {
                        _ = innerInstance;
                        engine.Event.Publish(new TestEvent("FromExecuted", (eventInstance, eventCt) =>
                        {
                            _ = eventInstance;
                            _ = eventCt;
                            eventReceived.TrySetResult(true);
                            return Task.CompletedTask;
                        }), innerInnerCt);
                        return Task.CompletedTask;
                    }), innerCt);
                }, ct);
            }), TestCtxCT);

            var completedTask = await Task.WhenAny(
                eventReceived.Task,
                Task.Delay(2000, TestCtxCT));

            // Assert
            Assert.True(completedTask == eventReceived.Task, "Event was not received from ExecuteAsync. Outbox was likely not flushed.");
        }
        // Verify that no unhandled exceptions were reported to the observer
        mock.Verify(x => x.OnUnhandledException(It.IsAny<Exception>()), Times.Never);
        mock.Verify(x => x.OnContextLeaked(), Times.Never);
    }

    /// <summary>
    /// Verifies that resetting UoW state in a scheduled child task does not affect the parent's Outbox.
    /// </summary>
    /// <returns>A task.</returns>
    [Fact]
    public async Task ScheduleShouldNotAffectParentOutbox()
    {
        var TestCtxCT = TestContext.Current.CancellationToken;

        // Arrange
        var mock = new Mock<ICalmErrorObserver>();
        await using (var engine = CreateCalmEngine(mock))
        {
            engine.Start();
            using var handler = new TestClass(engine, Logger);
            engine.Register(handler);

            var parentEventReceived = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var childEventReceived = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            // Act
            await engine.Command.SendAsync(new TestCommand("Root", async (instance, ct) =>
            {
                _ = instance;
                // 1. Publish event in Parent UoW
                engine.Event.Publish(new TestEvent("FromParent", (eventInstance, eventCt) =>
                {
                    _ = eventInstance;
                    _ = eventCt;
                    parentEventReceived.TrySetResult(true);
                    return Task.CompletedTask;
                }), ct);

                // 2. Schedule child task (which will reset its local UoW state)
                engine.Schedule(async innerCt =>
                {
                    // 3. Publish event in Child (New Root) UoW
                    await engine.Command.SendAsync(new TestCommand("ChildCommand", (innerInstance, innerInnerCt) =>
                    {
                        _ = innerInstance;
                        engine.Event.Publish(new TestEvent("FromChild", (eventInstance, eventCt) =>
                        {
                            _ = eventInstance;
                            _ = eventCt;
                            childEventReceived.TrySetResult(true);
                            return Task.CompletedTask;
                        }), innerInnerCt);
                        return Task.CompletedTask;
                    }), innerCt);
                }, ct);

                await Task.CompletedTask;
            }), TestCtxCT);

            // Assert
            var finishedTask = Task.WhenAll(parentEventReceived.Task, childEventReceived.Task);
            var timeoutTask = Task.Delay(2000, TestCtxCT);

            var completedTask = await Task.WhenAny(finishedTask, timeoutTask);

            Assert.True(completedTask == finishedTask, "One or both events were not received. Parent Outbox might have been affected.");
            Assert.True(parentEventReceived.Task.IsCompleted, "Parent event was not received.");
            Assert.True(childEventReceived.Task.IsCompleted, "Child event was not received.");
        }
        mock.Verify(x => x.OnUnhandledException(It.IsAny<Exception>()), Times.Never);
    }
}
