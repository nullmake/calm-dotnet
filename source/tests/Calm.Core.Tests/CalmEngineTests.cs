using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Calm.Core.Tests;

/// <summary>
/// Provides exhaustive tests for <see cref="CalmEngine"/> facade and lifecycle.
/// </summary>
public class CalmEngineTests() : TestBase(LogLevel.Trace)
{
    /// <summary>
    /// Verifies that the engine shuts down gracefully after processing all items.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task EngineShutsDownGracefully()
    {
        var TestCtxCT = TestContext.Current.CancellationToken;

        // Arrange
        var mock = new Mock<ICalmErrorObserver>();
        await using (var engine = CreateCalmEngine(mock))
        {
            engine.Start();
            bool itemProcessed = false;

            // Act
            await engine.ScheduleAsync(_ =>
            {
                itemProcessed = true;
                return Task.CompletedTask;
            }, TestCtxCT);
            await engine.StopAsync(TestCtxCT);

            // Assert
            Assert.True(itemProcessed, "Engine should finish remaining items before shutting down.");
        }
        mock.Verify(x => x.OnUnhandledException(It.IsAny<Exception>()), Times.Never);
        mock.Verify(x => x.OnContextLeaked(), Times.Never);
    }

    /// <summary>
    /// Verifies that posting tasks after calling StopAsync throws CalmEngineStoppingException.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task PostingAfterStopThrows()
    {
        var TestCtxCT = TestContext.Current.CancellationToken;

        // Arrange
        var mock = new Mock<ICalmErrorObserver>();
        await using (var engine = CreateCalmEngine(mock))
        {
            engine.Start();

            // Act
            await engine.StopAsync(TestCtxCT);

            // Assert
            Assert.Throws<CalmEngineStoppingException>(() => engine.Schedule(_ => Task.CompletedTask, TestCtxCT));
        }
        mock.Verify(x => x.OnUnhandledException(It.IsAny<Exception>()), Times.Never);
        mock.Verify(x => x.OnContextLeaked(), Times.Never);
    }

    /// <summary>
    /// Verifies that starting the engine twice does not throw.
    /// </summary>
    [Fact]
    public void StartTwiceShouldNotThrow()
    {
        // Arrange
        var mock = new Mock<ICalmErrorObserver>();
        using (var engine = CreateCalmEngine(mock))
        {
            // Act
            engine.Start();
            engine.Start();

            // Assert
            Assert.NotNull(engine);
        }
        mock.Verify(x => x.OnUnhandledException(It.IsAny<Exception>()), Times.Never);
        mock.Verify(x => x.OnContextLeaked(), Times.Never);
    }

    /// <summary>
    /// Verifies that calling facade methods after Dispose throws ObjectDisposedException or handled gracefully.
    /// Note: CalmEngine.Dispose() disposes the pump, and CalmPump.Schedule/Execute checks for shutdown/disposed.
    /// </summary>
    [Fact]
    public void ExecuteAfterDisposeThrows()
    {
        // Arrange
        var mock = new Mock<ICalmErrorObserver>();
        var engine = CreateCalmEngine(mock);
        engine.Dispose();

        // Act & Assert
        Assert.ThrowsAny<Exception>(() => engine.Start());
    }

    /// <summary>
    /// Verifies that ICalm facade correctly delegates to the pump and bus.
    /// This is a high-level test ensuring the facade is wired up.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task FacadeDelegationTest()
    {
        var TestCtxCT = TestContext.Current.CancellationToken;

        // Arrange
        var mock = new Mock<ICalmErrorObserver>();
        await using (var engine = CreateCalmEngine(mock))
        {
            engine.Start();

            // Act
            var result = await engine.ExecuteAsync(_ => Task.FromResult("FacadeWork"), TestCtxCT);

            // Assert
            Assert.Equal("FacadeWork", result);
            Assert.False(engine.IsOnEngineThread); // Caller thread
            await engine.ExecuteAsync(_ =>
            {
                Assert.True(engine.IsOnEngineThread); // Engine thread
                return Task.CompletedTask;
            }, TestCtxCT);
        }
        mock.Verify(x => x.OnUnhandledException(It.IsAny<Exception>()), Times.Never);
        mock.Verify(x => x.OnContextLeaked(), Times.Never);
    }
}
