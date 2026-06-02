using Calm.Core.Messaging;
using Calm.Core.Tests.TestClasses;
using Calm.Core.Tests.TestClasses.Messages;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Calm.Core.Tests.Messaging;

/// <summary>
/// Provides exhaustive tests for the <see cref="CalmBus"/> implementation.
/// </summary>
public class CalmBusTests() : TestBase(LogLevel.Trace)
{
    /// <summary>
    /// Verifies that Register(instance) routes messages correctly.
    /// </summary>
    /// <returns>A task.</returns>
    [Fact]
    public async Task RegisterInstanceShouldRouteMessages()
    {
        var TestCtxCT = TestContext.Current.CancellationToken;

        // Arrange
        var mock = new Mock<ICalmErrorObserver>();
        await using (var engine = CreateCalmEngine(mock))
        {
            engine.Start();
            using var test = new TestClass(engine, Logger);
            engine.Register(test);

            // Act
            await engine.Command.SendAsync(new TestCommand(), TestCtxCT);
            var queryResult = await engine.Query.SendAsync(new TestQuery(0, "Result"), TestCtxCT);
            engine.Event.Publish(new TestEvent(), TestCtxCT);
            await WaitForIdleAsync(engine, TestCtxCT);

            // Assert
            Assert.Equal(1, test.HandleCommandCount);
            Assert.Equal(1, test.HandleQueryCount);
            Assert.Equal(1, test.HandleEventCount);
            Assert.Equal("Result", queryResult.Output);
        }
        mock.Verify(x => x.OnUnhandledException(It.IsAny<Exception>()), Times.Never);
        mock.Verify(x => x.OnContextLeaked(), Times.Never);
    }

    /// <summary>
    /// Verifies that Register(type) routes messages to static handlers.
    /// </summary>
    /// <returns>A task.</returns>
    [Fact]
    public async Task RegisterTypeShouldRouteMessagesToStaticHandlers()
    {
        var TestCtxCT = TestContext.Current.CancellationToken;

        // Arrange
        var mock = new Mock<ICalmErrorObserver>();
        await using (var engine = CreateCalmEngine(mock))
        {
            engine.Start();

            try
            {
                await TestStaticHandlerClass.SetupAsync(engine, Logger);
                engine.Register(typeof(TestStaticHandlerClass));

                // Act
                await engine.Command.SendAsync(new TestCommand(), TestCtxCT);

                // Assert
                Assert.Equal(1, TestStaticHandlerClass.HandleCommandCount);
            }
            finally
            {
                TestStaticHandlerClass.Teardown();
            }
        }
        mock.Verify(x => x.OnUnhandledException(It.IsAny<Exception>()), Times.Never);
        mock.Verify(x => x.OnContextLeaked(), Times.Never);
    }

    /// <summary>
    /// Verifies that Unregister stops routing messages.
    /// </summary>
    /// <returns>A task.</returns>
    [Fact]
    public async Task UnregisterShouldStopRoutingMessages()
    {
        var TestCtxCT = TestContext.Current.CancellationToken;

        // Arrange
        var mock = new Mock<ICalmErrorObserver>();
        await using (var engine = CreateCalmEngine(mock))
        {
            engine.Start();
            using var test = new TestClass(engine, Logger);
            engine.Register(test);
            engine.Unregister(test);

            // Act & Assert
            await Assert.ThrowsAsync<CalmNoHandlerRegisteredException>(() =>
                engine.Command.SendAsync(new TestCommand(), TestCtxCT));
        }
        mock.Verify(x => x.OnUnhandledException(It.IsAny<Exception>()), Times.Never);
        mock.Verify(x => x.OnContextLeaked(), Times.Never);
    }

    /// <summary>
    /// Verifies that filters are respected during registration.
    /// </summary>
    /// <returns>A task.</returns>
    [Fact]
    public async Task RegisterWithFilterShouldRespectFilter()
    {
        var TestCtxCT = TestContext.Current.CancellationToken;

        // Arrange
        var mock = new Mock<ICalmErrorObserver>();
        await using (var engine = CreateCalmEngine(mock))
        {
            engine.Start();
            using var test = new TestClass(engine, Logger);
            // Ignore non-events (only register if category is event)
            engine.Register(test, info => info.Category is not CalmMessageCategory.Event);

            // Act
            engine.Event.Publish(new TestEvent(), TestCtxCT);
            await WaitForIdleAsync(engine, TestCtxCT);

            // Assert
            Assert.Equal(0, test.HandleEventCount);
            await engine.Command.SendAsync(new TestCommand(), TestCtxCT);
            Assert.Equal(1, test.HandleCommandCount);
        }
        mock.Verify(x => x.OnUnhandledException(It.IsAny<Exception>()), Times.Never);
        mock.Verify(x => x.OnContextLeaked(), Times.Never);
    }

    /// <summary>
    /// Verifies that double registration not throws.
    /// </summary>
    [Fact]
    public void RegisterSameHandlerTwiceShouldNotThrow()
    {
        // Arrange
        var mock = new Mock<ICalmErrorObserver>();
        using (var engine = CreateCalmEngine(mock))
        {
            engine.Start();
            using var test = new TestClass(engine, Logger);
            engine.Register(test);

            // This should not throw because Command handler for TestCommand is already registered
            // via the same handler instance
            Assert.Null(Record.Exception(() => engine.Register(test)));
        }
        mock.Verify(x => x.OnUnhandledException(It.IsAny<Exception>()), Times.Never);
        mock.Verify(x => x.OnContextLeaked(), Times.Never);
    }

    /// <summary>
    /// Verifies that double registration throws.
    /// </summary>
    [Fact]
    public void RegisterSameHandlerTwiceShouldThrow()
    {
        // Arrange
        var mock = new Mock<ICalmErrorObserver>();
        using (var engine = CreateCalmEngine(mock))
        {
            engine.Start();
            using var test1 = new TestClass(engine, Logger);
            engine.Register(test1);

            // This should not throw because Command handler for TestCommand is already registered
            // via the same handler instance
            using var test2 = new TestClass(engine, Logger);
            Assert.Throws<CalmHandlerAlreadyRegisteredException>(() => engine.Register(test2));
        }
        mock.Verify(x => x.OnUnhandledException(It.IsAny<Exception>()), Times.Never);
        mock.Verify(x => x.OnContextLeaked(), Times.Never);
    }

    /// <summary>
    /// Verifies that generic SendCommand routes correctly.
    /// </summary>
    /// <returns>A task.</returns>
    [Fact]
    public async Task SendCommandGenericShouldRouteCorrectly()
    {
        var TestCtxCT = TestContext.Current.CancellationToken;

        // Arrange
        var mock = new Mock<ICalmErrorObserver>();
        await using (var engine = CreateCalmEngine(mock))
        {
            engine.Start();
            using var test = new TestClass(engine, Logger);
            engine.Register(test);

            // Act
            await engine.Command.SendAsync(new TestCommand(), TestCtxCT);

            // Assert
            Assert.Equal(1, test.HandleCommandCount);
        }
        mock.Verify(x => x.OnUnhandledException(It.IsAny<Exception>()), Times.Never);
        mock.Verify(x => x.OnContextLeaked(), Times.Never);
    }

    /// <summary>
    /// Verifies that multiple handlers receive the same event.
    /// </summary>
    /// <returns>A task.</returns>
    [Fact]
    public async Task PublishEventMultipleHandlersShouldAllBeCalled()
    {
        var TestCtxCT = TestContext.Current.CancellationToken;

        // Arrange
        var mock = new Mock<ICalmErrorObserver>();
        await using (var engine = CreateCalmEngine(mock))
        {
            engine.Start();
            using var handler1 = new TestClass(engine, Logger);
            using var handler2 = new TestClass(engine, Logger);
            engine.Event.Register<TestEvent>(handler1.HandleTestEventAsync);
            engine.Event.Register<TestEvent>(handler2.HandleTestEventAsync);

            // Act
            engine.Event.Publish(new TestEvent(), TestCtxCT);
            await WaitForIdleAsync(engine, TestCtxCT);

            // Assert
            Assert.Equal(1, handler1.HandleEventCount);
            Assert.Equal(1, handler2.HandleEventCount);
        }
        mock.Verify(x => x.OnUnhandledException(It.IsAny<Exception>()), Times.Never);
        mock.Verify(x => x.OnContextLeaked(), Times.Never);
    }

    /// <summary>
    /// Verifies that ICalmCommandBus.Post works correctly as a fire-and-forget mechanism.
    /// </summary>
    /// <returns>A task.</returns>
    [Fact]
    public async Task PostShouldExecuteCommandAsFireAndForget()
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
            // Post the command, not awaiting its completion
            engine.Command.Post(new TestCommand("Posted", (instance, ct) =>
            {
                _ = instance;
                engine.Event.Publish(new TestEvent("FromPost", (eventInstance, eventCt) =>
                {
                    _ = eventInstance;
                    _ = eventCt;
                    eventReceived.TrySetResult(true);
                    return Task.CompletedTask;
                }), ct);
                return Task.CompletedTask;
            }), TestCtxCT);

            // Assert
            var completedTask = await Task.WhenAny(
                eventReceived.Task,
                Task.Delay(2000, TestCtxCT));

            Assert.True(completedTask == eventReceived.Task, "Event was not received from Post.");
        }
        mock.Verify(x => x.OnUnhandledException(It.IsAny<Exception>()), Times.Never);
    }

    /// <summary>
    /// Verifies that ICalmCommandBus.Post works correctly as a fire-and-forget mechanism.
    /// </summary>
    /// <returns>A task.</returns>
    [Fact]
    public async Task PostShouldExecuteCommandWithResponseAsFireAndForget()
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
            // Post the command, not awaiting its completion
            engine.Command.Post(new TestCommandWithResponse("Posted", "Response", (instance, ct) =>
            {
                _ = instance;
                engine.Event.Publish(new TestEvent("FromPost", (eventInstance, eventCt) =>
                {
                    _ = eventInstance;
                    _ = eventCt;
                    eventReceived.TrySetResult(true);
                    return Task.CompletedTask;
                }), ct);
                return Task.CompletedTask;
            }), TestCtxCT);

            // Assert
            var completedTask = await Task.WhenAny(
                eventReceived.Task,
                Task.Delay(2000, TestCtxCT));

            Assert.True(completedTask == eventReceived.Task, "Event was not received from Post.");
        }
        mock.Verify(x => x.OnUnhandledException(It.IsAny<Exception>()), Times.Never);
    }
}
