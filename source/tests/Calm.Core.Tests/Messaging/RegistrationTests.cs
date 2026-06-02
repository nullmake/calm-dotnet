using Calm.Core.Tests.TestClasses;
using Calm.Core.Tests.TestClasses.Messages;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Calm.Core.Tests.Messaging;

/// <summary>
/// Provides unit tests for handler registration and unregistration.
/// </summary>
public class RegistrationTests() : TestBase(LogLevel.Trace)
{
    /// <summary>
    /// Verifies that registering and unregistering a message handler works correctly.
    /// </summary>
    /// <param name="testClassType">The type of test class.</param>
    [Theory]
    [InlineData(typeof(TestClass))]
    [InlineData(typeof(TestDerivedClass))]
    [InlineData(typeof(TestPrivateHandlerClass))]
    public void RegisterAndUnregisterHandlerWork(Type testClassType)
    {
        // Arrange
        var mock = new Mock<ICalmErrorObserver>();
        using (var engine = CreateCalmEngine(mock))
        {
            engine.Start();

            using var test = CreateInstance(testClassType, engine);

            // Act & Assert
            // Registration
            engine.Command.Register((Func<TestCommand, CancellationToken, Task>)test.HandleTestCommandAsync);

            var handlers = engine.Command.EnumerateMessageHandler().ToArray();
            Assert.Single(handlers);
            Assert.Contains(handlers, h =>
                h.MessageType == typeof(TestCommand) && h.Matches(test.HandleTestCommandAsync));

            // Unregistration
            engine.Command.Unregister((Func<TestCommand, CancellationToken, Task>)test.HandleTestCommandAsync);

            Assert.Empty(engine.Command.EnumerateMessageHandler());
        }
        // Verify that no unhandled exceptions were reported to the observer
        mock.Verify(x => x.OnUnhandledException(It.IsAny<Exception>()), Times.Never);
        mock.Verify(x => x.OnContextLeaked(), Times.Never);
    }

    /// <summary>
    /// Verifies that registering and unregistering a message handler works correctly.
    /// </summary>
    /// <returns>A task representing the response from the handler.</returns>
    [Fact]
    public async ValueTask RegisterAndUnregisterStaticHandlerWork()
    {
        // Arrange
        var mock = new Mock<ICalmErrorObserver>();
        await using (var engine = CreateCalmEngine(mock))
        {
            engine.Start();

            try
            {
                await TestStaticHandlerClass.SetupAsync(engine, Logger);

                // Act & Assert
                // Registration
                engine.Command.Register<TestCommand>(TestStaticHandlerClass.HandleTestCommandAsync);

                var handlers = engine.Command.EnumerateMessageHandler().ToArray();
                Assert.Single(handlers);
                Assert.Contains(handlers, h =>
                    h.MessageType == typeof(TestCommand) && h.Matches(TestStaticHandlerClass.HandleTestCommandAsync));

                // Unregistration
                engine.Command.Unregister<TestCommand>(TestStaticHandlerClass.HandleTestCommandAsync);

                Assert.Empty(engine.Command.EnumerateMessageHandler());
            }
            finally
            {
                TestStaticHandlerClass.Teardown();
            }
        }
        // Verify that no unhandled exceptions were reported to the observer
        mock.Verify(x => x.OnUnhandledException(It.IsAny<Exception>()), Times.Never);
        mock.Verify(x => x.OnContextLeaked(), Times.Never);
    }

    /// <summary>
    /// Verifies that registering and unregistering a message handler works correctly.
    /// </summary>
    /// <param name="testClassType">The type of test class.</param>
    [Theory]
    [InlineData(typeof(TestClass))]
    [InlineData(typeof(TestDerivedClass))]
    [InlineData(typeof(TestPrivateHandlerClass))]
    public void RegisterInstanceHandlerTwiceWork(Type testClassType)
    {
        // Arrange
        var mock = new Mock<ICalmErrorObserver>();
        using (var engine = CreateCalmEngine(mock))
        {
            engine.Start();

            using var test = CreateInstance(testClassType, engine);

            // Act & Assert
            engine.Register(test);
            engine.Register(test);

            Assert.Contains(engine.Command.EnumerateMessageHandler(), h =>
               h.MessageType == typeof(TestCommand) && h.Matches(test.HandleTestCommandAsync));
            Assert.Contains(engine.Command.EnumerateRequestHandler(), h =>
               h.RequestType == typeof(TestCommandWithResponse) && h.Matches(test.HandleTestCommandWithResponseAsync));
            Assert.Contains(engine.Query.EnumerateRequestHandler(), h =>
               h.RequestType == typeof(TestQuery) && h.Matches(test.HandleTestQueryAsync));
            Assert.Contains(engine.Event.EnumerateMessageHandler(), h =>
               h.MessageType == typeof(TestEvent) && h.Matches(test.HandleTestEventAsync));
        }
        // Verify that no unhandled exceptions were reported to the observer
        mock.Verify(x => x.OnUnhandledException(It.IsAny<Exception>()), Times.Never);
        mock.Verify(x => x.OnContextLeaked(), Times.Never);
    }

    /// <summary>
    /// Verifies that registering message handlers works correctly.
    /// </summary>
    [Fact]
    public void RegisterTheSameHandlerTwiceWork()
    {
        // Arrange
        var mock = new Mock<ICalmErrorObserver>();
        using (var engine = CreateCalmEngine(mock))
        {
            engine.Start();

            using var test1 = new TestClass(engine, Logger);
            using var test2 = new TestDerivedClass(engine, Logger);

            // Act & Assert
            engine.Command.Register<TestCommand>(test1.HandleTestCommandAsync);
            engine.Command.Register<TestCommand>(test1.HandleTestCommandAsync);
            Assert.Throws<CalmHandlerAlreadyRegisteredException>(() =>
                engine.Command.Register<TestCommand>(test2.HandleTestCommandAsync));

            engine.Command.Register<TestCommandWithResponse, TestResponse>(
                test1.HandleTestCommandWithResponseAsync);
            engine.Command.Register<TestCommandWithResponse, TestResponse>(
                test1.HandleTestCommandWithResponseAsync);
            Assert.Throws<CalmHandlerAlreadyRegisteredException>(() =>
                engine.Command.Register<TestCommandWithResponse, TestResponse>(
                    test2.HandleTestCommandWithResponseAsync));

            engine.Query.Register<TestQuery, TestResponse>(test1.HandleTestQueryAsync);
            engine.Query.Register<TestQuery, TestResponse>(test1.HandleTestQueryAsync);
            Assert.Throws<CalmHandlerAlreadyRegisteredException>(() =>
                engine.Query.Register<TestQuery, TestResponse>(test2.HandleTestQueryAsync));

            engine.Event.Register<TestEvent>(test1.HandleTestEventAsync);
            engine.Event.Register<TestEvent>(test1.HandleTestEventAsync);
            engine.Event.Register<TestEvent>(test2.HandleTestEventAsync);

            Assert.Single(engine.Command.EnumerateMessageHandler());
            Assert.Single(engine.Command.EnumerateRequestHandler());
            Assert.Single(engine.Query.EnumerateRequestHandler());
            Assert.Equal(2, engine.Event.EnumerateMessageHandler().Count());
        }
        // Verify that no unhandled exceptions were reported to the observer
        mock.Verify(x => x.OnUnhandledException(It.IsAny<Exception>()), Times.Never);
        mock.Verify(x => x.OnContextLeaked(), Times.Never);
    }
}
