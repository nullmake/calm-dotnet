using Calm.Core.Tests.TestClasses;
using Calm.Core.Tests.TestClasses.Messages;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Calm.Core.Tests.Messaging;

/// <summary>
/// Provides tests for manual handler registration using the [CalmHandler] attribute.
/// </summary>
public class ManualRegistrationTests() : TestBase(LogLevel.Trace)
{
    /// <summary>
    /// Verifies that manual method registration with [CalmHandler] attribute succeeds.
    /// </summary>
    /// <returns>A task representing the response from the handler.</returns>
    [Fact]
    public async Task ManualMethodRegistrationWithAttributeShouldSucceed()
    {
        var TestCtxCT = TestContext.Current.CancellationToken;

        // Arrange
        var mock = new Mock<ICalmErrorObserver>();
        await using (var engine = CreateCalmEngine(mock))
        {
            engine.Start();

            using var test = new TestClass(engine, Logger);

            // Act
            engine.Command.Register<TestCommand>(test.HandleTestCommandAsync);

            var command = new TestCommand { Input = "Test" };
            await engine.Command.SendAsync(command, TestCtxCT);

            // Wait for processing
            await test.WaitUntilNoActiveHandlersAsync(TestCtxCT);

            // Assert
            Assert.Equal(1, test.HandleCommandCount);
        }
        mock.Verify(x => x.OnUnhandledException(It.IsAny<Exception>()), Times.Never);
        mock.Verify(x => x.OnContextLeaked(), Times.Never);
    }

    /// <summary>
    /// Verifies that manual instance registration discovers all [CalmHandler] methods.
    /// </summary>
    /// <returns>A task representing the response from the handler.</returns>
    /// <param name="testClassType">The type of test class.</param>
    [Theory]
    [InlineData(typeof(TestClass))]
    [InlineData(typeof(TestDerivedClass))]
    [InlineData(typeof(TestPrivateHandlerClass))]
    public async Task ManualInstanceRegistrationShouldDiscoverAllAttributedMethods(Type testClassType)
    {
        var TestCtxCT = TestContext.Current.CancellationToken;

        // Arrange
        var mock = new Mock<ICalmErrorObserver>();
        await using (var engine = CreateCalmEngine(mock))
        {
            engine.Start();

            using var test = CreateInstance(testClassType, engine);

            // Act
            engine.Register(test);
            Assert.Contains(engine.Command.EnumerateMessageHandler(), h =>
               h.MessageType == typeof(TestCommand) && h.Matches(test.HandleTestCommandAsync));
            Assert.Contains(engine.Command.EnumerateRequestHandler(), h =>
               h.RequestType == typeof(TestCommandWithResponse) && h.Matches(test.HandleTestCommandWithResponseAsync));
            Assert.Contains(engine.Query.EnumerateRequestHandler(), h =>
               h.RequestType == typeof(TestQuery) && h.Matches(test.HandleTestQueryAsync));
            Assert.Contains(engine.Event.EnumerateMessageHandler(), h =>
               h.MessageType == typeof(TestEvent) && h.Matches(test.HandleTestEventAsync));

            // Send command
            var command = new TestCommand
            {
                Input = "Command"
            };
            await engine.Command.SendAsync(command, TestCtxCT);

            // Send command with response.
            var commandWithResponse = new TestCommandWithResponse
            {
                Output = "Result: 88"
            };
            var commandResponse = await engine.Command.SendAsync(commandWithResponse, TestCtxCT);

            // Send query
            var query = new TestQuery
            {
                Output = "Result: 42"
            };
            var queryResponse = await engine.Query.SendAsync(query, TestCtxCT);

            // Publish event
            var testEvent = new TestEvent
            {
                Message = "Event"
            };
            engine.Event.Publish(testEvent, TestCtxCT);

            // Wait for processing
            await test.WaitUntilNoActiveHandlersAsync(TestCtxCT);

            // Assert
            Assert.Equal(1, test.HandleCommandCount);
            Assert.Equal(1, test.HandleCommandWithResponseCount);
            Assert.Equal(commandWithResponse.Output, commandResponse.Output);
            Assert.Equal(1, test.HandleQueryCount);
            Assert.Equal(query.Output, queryResponse.Output);
            Assert.Equal(1, test.HandleEventCount);
        }
        mock.Verify(x => x.OnUnhandledException(It.IsAny<Exception>()), Times.Never);
        mock.Verify(x => x.OnContextLeaked(), Times.Never);
    }

    /// <summary>
    /// Verifies that manual instance registration discovers all [CalmHandler] methods.
    /// </summary>
    /// <returns>A task representing the response from the handler.</returns>
    [Fact]
    public async ValueTask ManualInstanceRegistrationShouldDiscoverAllAttributedStaticMethods()
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
                Assert.Contains(engine.Command.EnumerateMessageHandler(), h =>
                   h.MessageType == typeof(TestCommand) && h.Matches(TestStaticHandlerClass.HandleTestCommandAsync));
                Assert.Contains(engine.Command.EnumerateRequestHandler(), h =>
                   h.RequestType == typeof(TestCommandWithResponse) && h.Matches(TestStaticHandlerClass.HandleTestCommandWithResponseAsync));
                Assert.Contains(engine.Query.EnumerateRequestHandler(), h =>
                   h.RequestType == typeof(TestQuery) && h.Matches(TestStaticHandlerClass.HandleTestQueryAsync));
                Assert.Contains(engine.Event.EnumerateMessageHandler(), h =>
                   h.MessageType == typeof(TestEvent) && h.Matches(TestStaticHandlerClass.HandleTestEventAsync));

                // Send command
                var command = new TestCommand
                {
                    Input = "Command"
                };
                await engine.Command.SendAsync(command, TestCtxCT);

                // Send command with response.
                var commandWithResponse = new TestCommandWithResponse
                {
                    Output = "Result: 88"
                };
                var commandResponse = await engine.Command.SendAsync(
                    commandWithResponse, TestCtxCT);

                // Send query
                var query = new TestQuery
                {
                    Output = "Result: 42"
                };
                var queryResponse = await engine.Query.SendAsync(query, TestCtxCT);

                // Publish event
                var testEvent = new TestEvent
                {
                    Message = "Event"
                };
                engine.Event.Publish(testEvent, TestCtxCT);

                // Wait for processing
                await TestStaticHandlerClass.WaitUntilNoActiveHandlersAsync(TestCtxCT);

                // Assert
                Assert.Equal(1, TestStaticHandlerClass.HandleCommandCount);
                Assert.Equal(1, TestStaticHandlerClass.HandleCommandWithResponseCount);
                Assert.Equal(commandWithResponse.Output, commandResponse.Output);
                Assert.Equal(1, TestStaticHandlerClass.HandleQueryCount);
                Assert.Equal(query.Output, queryResponse.Output);
                Assert.Equal(1, TestStaticHandlerClass.HandleEventCount);
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
    /// Verifies that manual method registration without [CalmHandler] attribute throws exception.
    /// </summary>
    [Fact]
    public void ManualMethodRegistrationWithoutAttributeShouldThrow()
    {
        // Arrange
        var mock = new Mock<ICalmErrorObserver>();
        using (var engine = CreateCalmEngine(mock))
        {
            // Don't start the engine - we're just testing registration validation

            var test = new TestNoAttributeHandlerClass();

            // Act & Assert
            foreach (var action in new Action[]
            {
                () => engine.Command.Register<TestCommand>(test.HandleTestCommandAsync),
                () => engine.Command.Register<TestCommandWithResponse, TestResponse>(
                    test.HandleTestCommandWithResponseAsync),
                () => engine.Query.Register<TestQuery, TestResponse>(test.HandleTestQueryAsync),
                () => engine.Event.Register<TestEvent>(test.HandleTestEventAsync)
            })
            {
                var ex = Assert.Throws<CalmSchemaException>(action);
                Assert.Contains("[CalmHandler]", ex.Message, StringComparison.Ordinal);
            }
        }
        mock.Verify(x => x.OnUnhandledException(It.IsAny<Exception>()), Times.Never);
        mock.Verify(x => x.OnContextLeaked(), Times.Never);
    }

    /// <summary>
    /// Verifies that generic methods are skipped during instance registration.
    /// </summary>
    /// <returns>A task representing the response from the handler.</returns>
    [Fact]
    public async Task ManualRegistrationShouldSkipGenericMethods()
    {
        // Arrange
        var mock = new Mock<ICalmErrorObserver>();
        await using (var engine = CreateCalmEngine(mock))
        {
            engine.Start();

            var test = new TestGenericHandlerClass();

            foreach (var action in new Action[]
            {
                () => engine.Command.Register<TestCommand>(test.HandleGenericAsync),
                () => engine.Command.Register<TestCommandWithResponse, TestResponse>(
                    test.HandleGenericWithResponseAsync),
                () => engine.Query.Register<TestQuery, TestResponse>(test.HandleGenericWithResponseAsync),
                () => engine.Event.Register<TestEvent>(test.HandleGenericAsync)
            })
            {
                var ex = Assert.Throws<CalmSchemaException>(action);
                Assert.Contains("must not be generic method", ex.Message, StringComparison.OrdinalIgnoreCase);
            }
        }
        mock.Verify(x => x.OnUnhandledException(It.IsAny<Exception>()), Times.Never);
        mock.Verify(x => x.OnContextLeaked(), Times.Never);
    }

    /// <summary>
    /// Verifies that generic methods are skipped during instance registration.
    /// </summary>
    /// <returns>A task representing the response from the handler.</returns>
    [Fact]
    public async Task ManualInstanceRegistrationShouldSkipGenericMethods()
    {
        var TestCtxCT = TestContext.Current.CancellationToken;

        // Arrange
        var mock = new Mock<ICalmErrorObserver>();
        await using (var engine = CreateCalmEngine(mock))
        {
            engine.Start();

            var test = new TestGenericHandlerClass();

            // Act
            engine.Register(test);

            // Only the non-generic method should be registered
            var command = new TestCommand
            {
                Input = "Test"
            };
            await Assert.ThrowsAsync<CalmNoHandlerRegisteredException>(
                () => engine.Command.SendAsync(command, TestCtxCT));

            // Assert
            Assert.False(test.GenericMethodCalled);
        }
        mock.Verify(x => x.OnUnhandledException(It.IsAny<Exception>()), Times.Never);
        mock.Verify(x => x.OnContextLeaked(), Times.Never);
    }
}
