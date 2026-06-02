using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Calm.Core.Tests.Engines;

/// <summary>
/// Provides tests for engine startup behavior.
/// </summary>
public class StartupTests() : TestBase(LogLevel.Trace)
{
    /// <summary>
    /// Verifies that Start starts a background thread.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task StartStartsBackgroundThread()
    {
        var TestCtxCT = TestContext.Current.CancellationToken;

        // Arrange
        var mock = new Mock<ICalmErrorObserver>();
        await using (var engine = CreateCalmEngine(mock))
        {
            int? executionThreadId = null;

            // Act
            engine.Start();
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
    /// Verifies that calling Start multiple times is ignored.
    /// </summary>
    [Fact]
    public void StartCalledMultipleTimesIgnored()
    {
        // Arrange
        var mock = new Mock<ICalmErrorObserver>();
        using (var engine = CreateCalmEngine(mock))
        {
            // Act & Assert (Should not throw)
            engine.Start();
            engine.Start();
        }
        mock.Verify(x => x.OnUnhandledException(It.IsAny<Exception>()), Times.Never);
        mock.Verify(x => x.OnContextLeaked(), Times.Never);
    }
}
