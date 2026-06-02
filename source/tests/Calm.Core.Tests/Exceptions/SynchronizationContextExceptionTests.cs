using Microsoft.Extensions.Logging;
using Moq;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Calm.Core.Tests.Exceptions;

/// <summary>
/// Provides tests for exception handling behavior in SynchronizationContext.Send and Post.
/// </summary>
[SuppressMessage("Design", "MA0042:Do not use blocking calls in an async method",
    Justification = "Test Patterns Using Dispose.")]
public class SynchronizationContextExceptionTests() : TestBase(LogLevel.Trace)
{
    #region Send from engine thread

    /// <summary>
    /// Verifies that synchronous exceptions in Send from engine thread are propagated to the caller
    /// and not reported to OnUnhandledException.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task SendFromEngineThreadWithSyncExceptionShouldPropagateToCaller()
    {
        var TestCtxCT = TestContext.Current.CancellationToken;

        // Arrange
        var mock = new Mock<ICalmErrorObserver>();
        using (var engine = CreateCalmEngine(mock))
        {
            engine.Start();

            // Act & Assert
            await engine.ExecuteAsync(_ =>
            {
                Assert.NotNull(SynchronizationContext.Current);

                // Send is called from engine thread
                var ex = Assert.Throws<InvalidOperationException>(() =>
                    SynchronizationContext.Current.Send(
                        __ => throw new InvalidOperationException("Sync failure"), null));

                Assert.Equal("Sync failure", ex.Message);
                return Task.CompletedTask;
            }, TestCtxCT);
        }

        // Verify: Exception should NOT be reported to OnUnhandledException
        mock.Verify(x => x.OnUnhandledException(It.IsAny<Exception>()), Times.Never);
        mock.Verify(x => x.OnContextLeaked(), Times.Never);
    }

    /// <summary>
    /// Verifies that asynchronous exceptions (after await) in Send from engine thread are reported to OnUnhandledException.
    /// Note: async void delegates in Send cannot propagate exceptions to the caller.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task SendFromEngineThreadWithAsyncExceptionShouldNotifyObserver()
    {
        var TestCtxCT = TestContext.Current.CancellationToken;

        // Arrange
        var mock = new Mock<ICalmErrorObserver>();
        using (var engine = CreateCalmEngine(mock))
        {
            engine.Start();

            // Act
            await engine.ExecuteAsync(async _ =>
            {
                await Task.Yield();

                Assert.NotNull(SynchronizationContext.Current);

                // Send with async delegate - exception cannot propagate through async void
                SynchronizationContext.Current.Send(async __ =>
                {
                    await Task.Yield();
                    throw new InvalidOperationException("Async failure");
                }, null);
            }, TestCtxCT);
        }

        // Verify: Exception SHOULD be reported to OnUnhandledException (async void cannot propagate)
        mock.Verify(x => x.OnUnhandledException(
            It.Is<InvalidOperationException>(e => e.Message == "Async failure")), Times.Once);
        mock.Verify(x => x.OnContextLeaked(), Times.Never);
    }

    #endregion

    #region Send from external thread

    /// <summary>
    /// Verifies that synchronous exceptions in Send from external thread are propagated to the caller
    /// and not reported to OnUnhandledException.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task SendFromExternalThreadWithSyncExceptionShouldPropagateToCaller()
    {
        var TestCtxCT = TestContext.Current.CancellationToken;

        // Arrange
        var mock = new Mock<ICalmErrorObserver>();
        using (var engine = CreateCalmEngine(mock))
        {
            engine.Start();

            // Capture the engine's SynchronizationContext
            SynchronizationContext? engineContext = null;
            await engine.ExecuteAsync(_ =>
            {
                engineContext = SynchronizationContext.Current;
                return Task.CompletedTask;
            }, TestCtxCT);

            Assert.NotNull(engineContext);

            // Act & Assert: Call Send from external thread using the engine's context
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                Task.Run(() =>
                {
                    engineContext.Send(__ => throw new InvalidOperationException("Sync failure"), null);
                }, TestCtxCT));

            Assert.Equal("Sync failure", ex.Message);
        }

        // Verify: Exception should NOT be reported to OnUnhandledException
        mock.Verify(x => x.OnUnhandledException(It.IsAny<Exception>()), Times.Never);
        mock.Verify(x => x.OnContextLeaked(), Times.Never);
    }

    /// <summary>
    /// Verifies that asynchronous exceptions (after await) in Send from external thread are reported to OnUnhandledException.
    /// Note: async void delegates in Send cannot propagate exceptions to the caller.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task SendFromExternalThreadWithAsyncExceptionShouldNotifyObserver()
    {
        var TestCtxCT = TestContext.Current.CancellationToken;

        // Arrange
        var mock = new Mock<ICalmErrorObserver>();
        using (var engine = CreateCalmEngine(mock))
        {
            engine.Start();

            // Capture the engine's SynchronizationContext
            SynchronizationContext? engineContext = null;
            await engine.ExecuteAsync(_ =>
            {
                engineContext = SynchronizationContext.Current;
                return Task.CompletedTask;
            }, TestCtxCT);

            Assert.NotNull(engineContext);

            // Act: Call Send from external thread using the engine's context
            await Task.Run(() =>
            {
                engineContext.Send(async __ =>
                {
                    await Task.Yield();
                    throw new InvalidOperationException("Async failure");
                }, null);
            }, TestCtxCT);

            // Wait for the posted action to be processed
            await WaitForIdleAsync(engine, TestCtxCT);
        }

        // Verify: Exception SHOULD be reported to OnUnhandledException (async void cannot propagate)
        mock.Verify(x => x.OnUnhandledException(
            It.Is<InvalidOperationException>(e => e.Message == "Async failure")), Times.Once);
        mock.Verify(x => x.OnContextLeaked(), Times.Never);
    }

    #endregion

    #region Post from engine thread

    /// <summary>
    /// Verifies that synchronous exceptions in Post from engine thread are reported to OnUnhandledException.
    /// Post is fire-and-forget, so exceptions cannot be propagated to the caller.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task PostFromEngineThreadWithSyncExceptionShouldNotifyObserver()
    {
        var TestCtxCT = TestContext.Current.CancellationToken;

        // Arrange
        var mock = new Mock<ICalmErrorObserver>();
        using (var engine = CreateCalmEngine(mock))
        {
            engine.Start();

            // Act
            await engine.ExecuteAsync(async _ =>
            {
                Assert.NotNull(SynchronizationContext.Current);

                // Post is called from engine thread (fire-and-forget)
                SynchronizationContext.Current.Post(
                    __ => throw new InvalidOperationException("Sync failure"), null);
            }, TestCtxCT);

            // Wait for the posted action to be processed
            await WaitForIdleAsync(engine, TestCtxCT);
        }

        // Verify: Exception SHOULD be reported to OnUnhandledException (fire-and-forget)
        mock.Verify(x => x.OnUnhandledException(
            It.Is<InvalidOperationException>(e => e.Message == "Sync failure")), Times.Once);
        mock.Verify(x => x.OnContextLeaked(), Times.Never);
    }

    /// <summary>
    /// Verifies that asynchronous exceptions (after await) in Post from engine thread are reported to OnUnhandledException.
    /// Post is fire-and-forget, so exceptions cannot be propagated to the caller.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task PostFromEngineThreadWithAsyncExceptionShouldNotifyObserver()
    {
        var TestCtxCT = TestContext.Current.CancellationToken;

        // Arrange
        var mock = new Mock<ICalmErrorObserver>();
        using (var engine = CreateCalmEngine(mock))
        {
            engine.Start();

            // Act
            await engine.ExecuteAsync(async _ =>
            {
                await Task.Yield();

                Assert.NotNull(SynchronizationContext.Current);

                // Post is called from engine thread (fire-and-forget)
                SynchronizationContext.Current.Post(async __ =>
                {
                    await Task.Yield();
                    throw new InvalidOperationException("Async failure");
                }, null);
            }, TestCtxCT);

            // Wait for the posted action to be processed
            await WaitForIdleAsync(engine, TestCtxCT);
        }

        // Verify: Exception SHOULD be reported to OnUnhandledException (fire-and-forget)
        mock.Verify(x => x.OnUnhandledException(
            It.Is<InvalidOperationException>(e => e.Message == "Async failure")), Times.Once);
        mock.Verify(x => x.OnContextLeaked(), Times.Never);
    }

    #endregion

    #region Post from external thread

    /// <summary>
    /// Verifies that synchronous exceptions in Post from external thread are reported to OnUnhandledException.
    /// Post is fire-and-forget, so exceptions cannot be propagated to the caller.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task PostFromExternalThreadWithSyncExceptionShouldNotifyObserver()
    {
        var TestCtxCT = TestContext.Current.CancellationToken;

        // Arrange
        var mock = new Mock<ICalmErrorObserver>();
        using (var engine = CreateCalmEngine(mock))
        {
            engine.Start();

            // Capture the engine's SynchronizationContext
            SynchronizationContext? engineContext = null;
            await engine.ExecuteAsync(_ =>
            {
                engineContext = SynchronizationContext.Current;
                return Task.CompletedTask;
            }, TestCtxCT);

            Assert.NotNull(engineContext);

            // Act: Post from external thread using the engine's context
            await Task.Run(() =>
            {
                engineContext.Post(__ => throw new InvalidOperationException("Sync failure"), null);
            }, TestCtxCT);

            // Wait for the posted action to be processed
            await WaitForIdleAsync(engine, TestCtxCT);
        }

        // Verify: Exception SHOULD be reported to OnUnhandledException (fire-and-forget)
        mock.Verify(x => x.OnUnhandledException(
            It.Is<InvalidOperationException>(e => e.Message == "Sync failure")), Times.Once);
        mock.Verify(x => x.OnContextLeaked(), Times.Never);
    }

    /// <summary>
    /// Verifies that asynchronous exceptions (after await) in Post from external thread are reported to OnUnhandledException.
    /// Post is fire-and-forget, so exceptions cannot be propagated to the caller.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task PostFromExternalThreadWithAsyncExceptionShouldNotifyObserver()
    {
        var TestCtxCT = TestContext.Current.CancellationToken;

        // Arrange
        var mock = new Mock<ICalmErrorObserver>();
        using (var engine = CreateCalmEngine(mock))
        {
            engine.Start();

            // Capture the engine's SynchronizationContext
            SynchronizationContext? engineContext = null;
            await engine.ExecuteAsync(_ =>
            {
                engineContext = SynchronizationContext.Current;
                return Task.CompletedTask;
            }, TestCtxCT);

            Assert.NotNull(engineContext);

            // Act: Post from external thread using the engine's context
            await Task.Run(() =>
            {
                engineContext.Post(async __ =>
                {
                    await Task.Yield();
                    throw new InvalidOperationException("Async failure");
                }, null);
            }, TestCtxCT);

            // Wait for the posted action to be processed
            await WaitForIdleAsync(engine, TestCtxCT);
        }

        // Verify: Exception SHOULD be reported to OnUnhandledException (fire-and-forget)
        mock.Verify(x => x.OnUnhandledException(
            It.Is<InvalidOperationException>(e => e.Message == "Async failure")), Times.Once);
        mock.Verify(x => x.OnContextLeaked(), Times.Never);
    }

    #endregion
}
