using Calm.Core.Tests.TestClasses;
using Calm.Core.Tests.TestClasses.Messages;
using Microsoft.Extensions.Logging;
using Moq;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Calm.Core.Tests.Engines;

/// <summary>
/// Provides tests for engine startup behavior.
/// </summary>
public class TeardownTests() : TestBase(LogLevel.Trace)
{
    /// <summary>
    /// Processing to execute each message.
    /// </summary>
    /// <param name="engine">The Calm engine.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task ExecuteMessageHandlersAsync(CalmEngine engine)
    {
        var TestCtxCT = TestContext.Current.CancellationToken;

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
        _ = await engine.Command.SendAsync(commandWithResponse, TestCtxCT);

        // Send query
        var query = new TestQuery
        {
            Output = "Result: 42"
        };
        _ = await engine.Query.SendAsync(query, TestCtxCT);

        // Publish event
        var testEvent = new TestEvent
        {
            Message = "Event"
        };
        engine.Event.Publish(testEvent, TestCtxCT);
    }

    /// <summary>
    /// Verify whether it is acceptable for teardown methods to be called multiple times.
    /// </summary>
    /// <param name="dispose">The number of times to be called Dispose()</param>
    /// <param name="disposeAsync">The number of times to be called DisposeAsync()</param>
    /// <param name="stopAsync">The number of times to be called StopAsync()</param>
    /// <param name="waitForShutdownAsync">The number of times to be called WaitForShutdownAsync()</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Theory]
    [InlineData(1, 2, 0, 2)]
    [InlineData(2, 0, 1, 0)]
    [InlineData(1, 0, 2, 1)]
    [InlineData(2, 1, 0, 1)]
    [InlineData(0, 1, 2, 0)]
    [InlineData(0, 2, 0, 0)]
    [InlineData(0, 1, 1, 2)]
    [InlineData(2, 2, 2, 1)]
    [InlineData(1, 2, 1, 0)]
    [InlineData(0, 0, 0, 2)]
    [InlineData(0, 2, 1, 1)]
    [InlineData(1, 1, 2, 2)]
    [InlineData(2, 1, 0, 2)]
    [SuppressMessage("Performance", "CA1849:Call async methods when in an async method", Justification = "Test")]
    public async Task CallMultipleTimes(int dispose, int disposeAsync, int stopAsync, int waitForShutdownAsync)
    {
        var TestCtxCT = TestContext.Current.CancellationToken;

        // Arrange
        var mock = new Mock<ICalmErrorObserver>();
        await using (var engine = CreateCalmEngine(mock))
        {
            using var test = new TestClass(engine, Logger);
            engine.Register(test);
            engine.Start();
            await ExecuteMessageHandlersAsync(engine);

            // Act
            for (int i = 0; i < dispose; i++)
            {
#pragma warning disable S6966, MA0042, VSTHRD103 // Awaitable method should be used
                engine.Dispose();
#pragma warning restore S6966, MA0042, VSTHRD103 // Awaitable method should be used
            }

            for (int i = 0; i < disposeAsync; i++)
            {
                await engine.DisposeAsync();
            }

            for (int i = 0; i < stopAsync; i++)
            {
                await engine.StopAsync(TestCtxCT);
            }

            if (stopAsync is 0)
            {
                await engine.StopAsync(TestCtxCT);
            }
            for (int i = 0; i < waitForShutdownAsync; i++)
            {
                await engine.WaitForShutdownAsync(TestCtxCT);
            }
        }
        mock.Verify(x => x.OnUnhandledException(It.IsAny<Exception>()), Times.Never);
        mock.Verify(x => x.OnContextLeaked(), Times.Never);
    }

    /// <summary>
    /// Verify whether it is acceptable for teardown methods to be called the any order.
    /// </summary>
    /// <param name="order">The order to be teardown mehod called.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">The order parameter is null.</exception>
    /// <exception cref="NotSupportedException">Test parameters not supported.</exception>
    [Theory]
    [InlineData(0, 1, 2, 3)]
    [InlineData(0, 1, 3, 2)]
    [InlineData(0, 2, 1, 3)]
    [InlineData(0, 2, 3, 1)]
    [InlineData(0, 3, 1, 2)]
    [InlineData(0, 3, 2, 1)]
    [InlineData(1, 0, 2, 3)]
    [InlineData(1, 0, 3, 2)]
    [InlineData(1, 2, 0, 3)]
    [InlineData(1, 2, 3, 0)]
    [InlineData(1, 3, 0, 2)]
    [InlineData(1, 3, 2, 0)]
    [InlineData(2, 0, 1, 3)]
    [InlineData(2, 0, 3, 1)]
    [InlineData(2, 1, 0, 3)]
    [InlineData(2, 1, 3, 0)]
    [InlineData(2, 3, 0, 1)]
    [InlineData(2, 3, 1, 0)]
    [SuppressMessage("Performance", "CA1849:Call async methods when in an async method", Justification = "Test")]
    public async Task CallAnyOrder(params int[] order)
    {
        var TestCtxCT = TestContext.Current.CancellationToken;

        // Arrange
        var mock = new Mock<ICalmErrorObserver>();
        await using (var engine = CreateCalmEngine(mock))
        {
            using var test = new TestClass(engine, Logger);
            engine.Register(test);
            engine.Start();
            await ExecuteMessageHandlersAsync(engine);

            // Act
            foreach (var method in order ?? throw new ArgumentNullException(nameof(order)))
            {
                switch (method)
                {
                    case 0:
#pragma warning disable S6966, MA0042, VSTHRD103 // Awaitable method should be used
                        engine.Dispose();
#pragma warning restore S6966, MA0042, VSTHRD103 // Awaitable method should be used
                        break;
                    case 1:
                        await engine.DisposeAsync();
                        break;
                    case 2:
                        await engine.StopAsync(TestCtxCT);
                        break;
                    case 3:
                        await engine.WaitForShutdownAsync(TestCtxCT);
                        break;
                    default:
                        throw new NotSupportedException();
                }
            }
        }
        mock.Verify(x => x.OnUnhandledException(It.IsAny<Exception>()), Times.Never);
        mock.Verify(x => x.OnContextLeaked(), Times.Never);
    }
}
