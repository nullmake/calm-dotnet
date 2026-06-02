using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Calm.Core.Tests.Engines;

/// <summary>
/// Provides tests for ExecutionContext and AsyncLocal flow within the engine.
/// </summary>
public class ContextFlowTests() : TestBase(LogLevel.Trace)
{
    /// <summary>
    /// Holds a value that is local to the asynchronous control flow.
    /// </summary>
    private static readonly AsyncLocal<int> _asyncLocalValue = new();

    /// <summary>
    /// Verifies that AsyncLocal state is preserved across asynchronous boundaries managed by ExecuteAsync.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task ExecuteAsyncPreservesAsyncLocal()
    {
        var TestCtxCT = TestContext.Current.CancellationToken;

        // Arrange
        var mock = new Mock<ICalmErrorObserver>();
        await using (var engine = CreateCalmEngine(mock))
        {
            engine.Start();
            const int expectedValue = 123;
            int? capturedValue = null;

            // Act
            _asyncLocalValue.Value = expectedValue;
            await engine.ExecuteAsync(_ =>
            {
                capturedValue = _asyncLocalValue.Value;
                return Task.CompletedTask;
            }, TestCtxCT);

            // Assert
            Assert.Equal(expectedValue, capturedValue);
        }
        mock.Verify(x => x.OnUnhandledException(It.IsAny<Exception>()), Times.Never);
        mock.Verify(x => x.OnContextLeaked(), Times.Never);
    }

    /// <summary>
    /// Verifies that AsyncLocal state flows correctly across await points (segmented execution).
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task ContextShouldFlowThroughSegments()
    {
        var TestCtxCT = TestContext.Current.CancellationToken;

        // Arrange
        var mock = new Mock<ICalmErrorObserver>();
        await using (var engine = CreateCalmEngine(mock))
        {
            engine.Start();
            const int expectedValue = 789;
            int? capturedValueBeforeAwait = null;
            int? capturedValueAfterAwait = null;

            // Act
            await engine.ExecuteAsync(async _ =>
            {
                _asyncLocalValue.Value = expectedValue;
                capturedValueBeforeAwait = _asyncLocalValue.Value;

                await Task.Yield(); // Forces a continuation (segmented execution)

                capturedValueAfterAwait = _asyncLocalValue.Value;
            }, TestCtxCT);

            // Assert
            Assert.Equal(expectedValue, capturedValueBeforeAwait);
            Assert.Equal(expectedValue, capturedValueAfterAwait);
        }
        mock.Verify(x => x.OnUnhandledException(It.IsAny<Exception>()), Times.Never);
        mock.Verify(x => x.OnContextLeaked(), Times.Never);
    }
}
