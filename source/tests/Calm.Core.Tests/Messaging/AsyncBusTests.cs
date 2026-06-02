using Calm.Core.Tests.TestClasses;
using Calm.Core.Tests.TestClasses.Messages;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Calm.Core.Tests.Messaging;

/// <summary>
/// Verifies the PublishAsync and PostAsync methods.
/// </summary>
public class AsyncBusTests() : TestBase(LogLevel.Trace)
{
    /// <summary>
    /// Verifies that PublishAsync waits for all handlers to complete.
    /// </summary>
    /// <returns>A task.</returns>
    [Fact]
    public async Task PublishAsyncShouldWaitForHandlers()
    {
        var TestCtxCT = TestContext.Current.CancellationToken;

        // Arrange
        var mock = new Mock<ICalmErrorObserver>();
        await using (var engine = CreateCalmEngine(mock))
        {
            engine.Start();
            using var test = new TestClass(engine, Logger);
            engine.Register(test);

            var handlerStarted = false;
            var handlerFinished = false;

            var @event = new TestEvent
            {
                FuncAsync = async (_, ct) =>
                {
                    handlerStarted = true;
                    await Task.Delay(100, ct);
                    handlerFinished = true;
                }
            };

            // Act
            await engine.Event.PublishAsync(@event, TestCtxCT);

            // Assert
            Assert.True(handlerStarted, "Handler should have started");
            Assert.True(handlerFinished, "Handler should have finished because PublishAsync waits for it");
            Assert.Equal(1, test.HandleEventCount);
        }
        mock.Verify(x => x.OnUnhandledException(It.IsAny<Exception>()), Times.Never);
        mock.Verify(x => x.OnContextLeaked(), Times.Never);
    }

    /// <summary>
    /// Verifies that PostAsync waits for the handler to complete.
    /// </summary>
    /// <returns>A task.</returns>
    [Fact]
    public async Task PostAsyncShouldWaitForHandler()
    {
        var TestCtxCT = TestContext.Current.CancellationToken;

        // Arrange
        var mock = new Mock<ICalmErrorObserver>();
        await using (var engine = CreateCalmEngine(mock))
        {
            engine.Start();
            using var test = new TestClass(engine, Logger);
            engine.Register(test);

            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var handlerStarted = false;
            var handlerFinished = false;

            var command = new TestCommand
            {
                FuncAsync = async (_, ct) =>
                {
                    handlerStarted = true;
                    await Task.Delay(100, ct);
                    handlerFinished = true;
                    tcs.SetResult(true);
                }
            };

            // Act
            await engine.Command.PostAsync(command, TestCtxCT);
            await tcs.Task;

            // Assert
            Assert.True(handlerStarted, "Handler should have started");
            Assert.True(handlerFinished, "Handler should have finished because PostAsync waits for it");
            Assert.Equal(1, test.HandleCommandCount);
        }
        mock.Verify(x => x.OnUnhandledException(It.IsAny<Exception>()), Times.Never);
        mock.Verify(x => x.OnContextLeaked(), Times.Never);
    }

    /// <summary>
    /// Verifies that PostAsync with response waits for the handler to complete.
    /// </summary>
    /// <returns>A task.</returns>
    [Fact]
    public async Task PostAsyncWithResponseShouldWaitForHandler()
    {
        var TestCtxCT = TestContext.Current.CancellationToken;

        // Arrange
        var mock = new Mock<ICalmErrorObserver>();
        await using (var engine = CreateCalmEngine(mock))
        {
            engine.Start();
            using var test = new TestClass(engine, Logger);
            engine.Register(test);

            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var handlerStarted = false;
            var handlerFinished = false;

            var command = new TestCommandWithResponse
            {
                FuncAsync = async (_, ct) =>
                {
                    handlerStarted = true;
                    await Task.Delay(100, ct);
                    handlerFinished = true;
                    tcs.SetResult(true);
                }
            };

            // Act
            await engine.Command.PostAsync(command, TestCtxCT);
            await tcs.Task;

            // Assert
            Assert.True(handlerStarted, "Handler should have started");
            Assert.True(handlerFinished, "Handler should have finished because PostAsync waits for it");
            Assert.Equal(1, test.HandleCommandWithResponseCount);
        }
        mock.Verify(x => x.OnUnhandledException(It.IsAny<Exception>()), Times.Never);
        mock.Verify(x => x.OnContextLeaked(), Times.Never);
    }
}
