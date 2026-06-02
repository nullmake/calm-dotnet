using Calm.Core.Tests.TestClasses;
using Calm.Core.Tests.TestClasses.Messages;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Calm.Core.Tests.Exceptions;

/// <summary>
/// Provides tests for exception handling behavior in Command, CommandWithResponse, Query, and Event messaging.
/// </summary>
public class MessagingExceptionTests() : TestBase(LogLevel.Trace)
{
    #region Command
    #region Command - Root call
    /// <summary>
    /// Verifies that synchronous exceptions in Command from engine thread (root) are propagated to the caller
    /// and not reported to OnUnhandledException.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task CommandFromEngineThreadRootSyncExceptionShouldPropagateToCaller()
    {
        var TestCtxCT = TestContext.Current.CancellationToken;

        // Arrange
        var mock = new Mock<ICalmErrorObserver>();
        await using (var engine = CreateCalmEngine(mock))
        {
            engine.Start();
            using var test = new TestClass(engine, Logger);
            engine.Command.Register<TestCommand>(test.HandleTestCommandAsync);

            // Act & Assert
            var command = new TestCommand("Execute", (_, _) => throw new InvalidOperationException("Sync failure"));
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                engine.Command.SendAsync(command, TestCtxCT));
        }

        // Verify: Exception should NOT be reported to OnUnhandledException (root UoW)
        mock.Verify(x => x.OnUnhandledException(It.IsAny<Exception>()), Times.Never);
        mock.Verify(x => x.OnContextLeaked(), Times.Never);
    }

    /// <summary>
    /// Verifies that asynchronous exceptions in Command from engine thread (root) are propagated to the caller
    /// and not reported to OnUnhandledException.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task CommandFromEngineThreadRootAsyncExceptionShouldPropagateToCaller()
    {
        var TestCtxCT = TestContext.Current.CancellationToken;

        // Arrange
        var mock = new Mock<ICalmErrorObserver>();
        await using (var engine = CreateCalmEngine(mock))
        {
            engine.Start();
            using var test = new TestClass(engine, Logger);
            engine.Command.Register<TestCommand>(test.HandleTestCommandAsync);

            // Act & Assert
            var command = new TestCommand("Execute", async (_, _) =>
            {
                await Task.Yield();
                throw new InvalidOperationException("Async failure");
            });
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                engine.Command.SendAsync(command, TestCtxCT));
        }

        // Verify: Exception should NOT be reported to OnUnhandledException (root UoW)
        mock.Verify(x => x.OnUnhandledException(It.IsAny<Exception>()), Times.Never);
        mock.Verify(x => x.OnContextLeaked(), Times.Never);
    }

    /// <summary>
    /// Verifies that synchronous exceptions in Command from external thread (root) are propagated to the caller
    /// and not reported to OnUnhandledException.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task CommandFromExternalThreadRootSyncExceptionShouldPropagateToCaller()
    {
        var TestCtxCT = TestContext.Current.CancellationToken;

        // Arrange
        var mock = new Mock<ICalmErrorObserver>();
        await using (var engine = CreateCalmEngine(mock))
        {
            engine.Start();
            using var test = new TestClass(engine, Logger);
            engine.Command.Register<TestCommand>(test.HandleTestCommandAsync);

            // Act & Assert
            var command = new TestCommand("Execute", (_, _) => throw new InvalidOperationException("Sync failure"));
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                Task.Run(() => engine.Command.SendAsync(command, TestCtxCT), TestCtxCT));
        }

        // Verify: Exception should NOT be reported to OnUnhandledException (root UoW)
        mock.Verify(x => x.OnUnhandledException(It.IsAny<Exception>()), Times.Never);
        mock.Verify(x => x.OnContextLeaked(), Times.Never);
    }

    /// <summary>
    /// Verifies that asynchronous exceptions in Command from external thread (root) are propagated to the caller
    /// and not reported to OnUnhandledException.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task CommandFromExternalThreadRootAsyncExceptionShouldPropagateToCaller()
    {
        var TestCtxCT = TestContext.Current.CancellationToken;

        // Arrange
        var mock = new Mock<ICalmErrorObserver>();
        await using (var engine = CreateCalmEngine(mock))
        {
            engine.Start();
            using var test = new TestClass(engine, Logger);
            engine.Command.Register<TestCommand>(test.HandleTestCommandAsync);

            // Act & Assert
            var command = new TestCommand("Execute", async (_, _) =>
            {
                await Task.Yield();
                throw new InvalidOperationException("Async failure");
            });
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                Task.Run(() => engine.Command.SendAsync(command, TestCtxCT), TestCtxCT));
        }

        // Verify: Exception should NOT be reported to OnUnhandledException (root UoW)
        mock.Verify(x => x.OnUnhandledException(It.IsAny<Exception>()), Times.Never);
        mock.Verify(x => x.OnContextLeaked(), Times.Never);
    }
    #endregion

    #region Command - Nested call
    /// <summary>
    /// Verifies that synchronous exceptions in nested Command from engine thread are propagated to the caller
    /// and reported to OnUnhandledException.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task CommandFromEngineThreadNestedSyncExceptionShouldPropagateAndNotify()
    {
        var TestCtxCT = TestContext.Current.CancellationToken;

        // Arrange
        var mock = new Mock<ICalmErrorObserver>();
        await using (var engine = CreateCalmEngine(mock))
        {
            engine.Start();
            using var test = new TestClass(engine, Logger);
            engine.Command.Register<TestCommand>(test.HandleTestCommandAsync);

            // Act & Assert
            var parentCommand = new TestCommand("Parent", async (_, token) =>
            {
                var nestedCommand = new TestCommand("Nested",
                    (_, _) => throw new InvalidOperationException("Nested sync failure"));
                await engine.Command.SendAsync(nestedCommand, token);
            });
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                engine.Command.SendAsync(parentCommand, TestCtxCT));
        }

        // Verify: Exception SHOULD be reported to OnUnhandledException (nested)
        mock.Verify(x => x.OnUnhandledException(
            It.Is<InvalidOperationException>(e => e.Message == "Nested sync failure")), Times.Once);
        mock.Verify(x => x.OnContextLeaked(), Times.Never);
    }

    /// <summary>
    /// Verifies that asynchronous exceptions in nested Command from engine thread are propagated to the caller
    /// and reported to OnUnhandledException.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task CommandFromEngineThreadNestedAsyncExceptionShouldPropagateAndNotify()
    {
        var TestCtxCT = TestContext.Current.CancellationToken;

        // Arrange
        var mock = new Mock<ICalmErrorObserver>();
        await using (var engine = CreateCalmEngine(mock))
        {
            engine.Start();
            using var test = new TestClass(engine, Logger);
            engine.Command.Register<TestCommand>(test.HandleTestCommandAsync);

            // Act & Assert
            var parentCommand = new TestCommand("Parent", async (_, token) =>
            {
                var nestedCommand = new TestCommand("Nested", async (_, _) =>
                {
                    await Task.Yield();
                    throw new InvalidOperationException("Nested async failure");
                });
                await engine.Command.SendAsync(nestedCommand, token);
            });
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                engine.Command.SendAsync(parentCommand, TestCtxCT));
        }

        // Verify: Exception SHOULD be reported to OnUnhandledException (nested)
        mock.Verify(x => x.OnUnhandledException(
            It.Is<InvalidOperationException>(e => e.Message == "Nested async failure")), Times.Once);
        mock.Verify(x => x.OnContextLeaked(), Times.Never);
    }

    /// <summary>
    /// Verifies that synchronous exceptions in nested Command from external thread are propagated to the caller
    /// and reported to OnUnhandledException.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task CommandFromExternalThreadNestedSyncExceptionShouldPropagateAndNotify()
    {
        var TestCtxCT = TestContext.Current.CancellationToken;

        // Arrange
        var mock = new Mock<ICalmErrorObserver>();
        await using (var engine = CreateCalmEngine(mock))
        {
            engine.Start();
            using var test = new TestClass(engine, Logger);
            engine.Command.Register<TestCommand>(test.HandleTestCommandAsync);

            // Act & Assert
            var parentCommand = new TestCommand("Parent", async (_, token) =>
            {
                var nestedCommand = new TestCommand("Nested",
                    (_, _) => throw new InvalidOperationException("Nested sync failure"));
                await engine.Command.SendAsync(nestedCommand, token);
            });
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                Task.Run(() => engine.Command.SendAsync(parentCommand, TestCtxCT), TestCtxCT));
        }

        // Verify: Exception SHOULD be reported to OnUnhandledException (nested)
        mock.Verify(x => x.OnUnhandledException(
            It.Is<InvalidOperationException>(e => e.Message == "Nested sync failure")), Times.Once);
        mock.Verify(x => x.OnContextLeaked(), Times.Never);
    }

    /// <summary>
    /// Verifies that asynchronous exceptions in nested Command from external thread are propagated to the caller
    /// and reported to OnUnhandledException.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task CommandFromExternalThreadNestedAsyncExceptionShouldPropagateAndNotify()
    {
        var TestCtxCT = TestContext.Current.CancellationToken;

        // Arrange
        var mock = new Mock<ICalmErrorObserver>();
        await using (var engine = CreateCalmEngine(mock))
        {
            engine.Start();
            using var test = new TestClass(engine, Logger);
            engine.Command.Register<TestCommand>(test.HandleTestCommandAsync);

            // Act & Assert
            var parentCommand = new TestCommand("Parent", async (_, token) =>
            {
                var nestedCommand = new TestCommand("Nested", async (_, _) =>
                {
                    await Task.Yield();
                    throw new InvalidOperationException("Nested async failure");
                });
                await engine.Command.SendAsync(nestedCommand, token);
            });
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                Task.Run(() => engine.Command.SendAsync(parentCommand, TestCtxCT), TestCtxCT));
        }

        // Verify: Exception SHOULD be reported to OnUnhandledException (nested)
        mock.Verify(x => x.OnUnhandledException(
            It.Is<InvalidOperationException>(e => e.Message == "Nested async failure")), Times.Once);
        mock.Verify(x => x.OnContextLeaked(), Times.Never);
    }
    #endregion
    #endregion

    #region CommandWithResponse
    #region CommandWithResponse - Root call
    /// <summary>
    /// Verifies that synchronous exceptions in CommandWithResponse from engine thread (root) are propagated to the caller
    /// and not reported to OnUnhandledException.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task CommandWithResponseFromEngineThreadRootSyncExceptionShouldPropagateToCaller()
    {
        var TestCtxCT = TestContext.Current.CancellationToken;

        // Arrange
        var mock = new Mock<ICalmErrorObserver>();
        await using (var engine = CreateCalmEngine(mock))
        {
            engine.Start();
            using var test = new TestClass(engine, Logger);
            engine.Command.Register<TestCommandWithResponse, TestResponse>(test.HandleTestCommandWithResponseAsync);

            // Act & Assert
            var command = new TestCommandWithResponse("Execute", "Result",
                (_, _) => throw new InvalidOperationException("Sync failure"));
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => engine.Command.SendAsync(command, TestCtxCT));
        }

        // Verify: Exception should NOT be reported to OnUnhandledException (root UoW)
        mock.Verify(x => x.OnUnhandledException(It.IsAny<Exception>()), Times.Never);
        mock.Verify(x => x.OnContextLeaked(), Times.Never);
    }

    /// <summary>
    /// Verifies that asynchronous exceptions in CommandWithResponse from engine thread (root) are propagated to the caller
    /// and not reported to OnUnhandledException.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task CommandWithResponseFromEngineThreadRootAsyncExceptionShouldPropagateToCaller()
    {
        var TestCtxCT = TestContext.Current.CancellationToken;

        // Arrange
        var mock = new Mock<ICalmErrorObserver>();
        await using (var engine = CreateCalmEngine(mock))
        {
            engine.Start();
            using var test = new TestClass(engine, Logger);
            engine.Command.Register<TestCommandWithResponse, TestResponse>(test.HandleTestCommandWithResponseAsync);

            // Act & Assert
            var command = new TestCommandWithResponse("Execute", "Result", async (_, _) =>
            {
                await Task.Yield();
                throw new InvalidOperationException("Async failure");
            });
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                engine.Command.SendAsync(command, TestCtxCT));
        }

        // Verify: Exception should NOT be reported to OnUnhandledException (root UoW)
        mock.Verify(x => x.OnUnhandledException(It.IsAny<Exception>()), Times.Never);
        mock.Verify(x => x.OnContextLeaked(), Times.Never);
    }

    /// <summary>
    /// Verifies that synchronous exceptions in CommandWithResponse from external thread (root) are propagated to the caller
    /// and not reported to OnUnhandledException.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task CommandWithResponseFromExternalThreadRootSyncExceptionShouldPropagateToCaller()
    {
        var TestCtxCT = TestContext.Current.CancellationToken;

        // Arrange
        var mock = new Mock<ICalmErrorObserver>();
        await using (var engine = CreateCalmEngine(mock))
        {
            engine.Start();
            using var test = new TestClass(engine, Logger);
            engine.Command.Register<TestCommandWithResponse, TestResponse>(test.HandleTestCommandWithResponseAsync);

            // Act & Assert
            var command = new TestCommandWithResponse("Execute", "Result",
                (_, _) => throw new InvalidOperationException("Sync failure"));
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                Task.Run(() => engine.Command.SendAsync(command, TestCtxCT), TestCtxCT));
        }

        // Verify: Exception should NOT be reported to OnUnhandledException (root UoW)
        mock.Verify(x => x.OnUnhandledException(It.IsAny<Exception>()), Times.Never);
        mock.Verify(x => x.OnContextLeaked(), Times.Never);
    }

    /// <summary>
    /// Verifies that asynchronous exceptions in CommandWithResponse from external thread (root) are propagated to the caller
    /// and not reported to OnUnhandledException.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task CommandWithResponseFromExternalThreadRootAsyncExceptionShouldPropagateToCaller()
    {
        var TestCtxCT = TestContext.Current.CancellationToken;

        // Arrange
        var mock = new Mock<ICalmErrorObserver>();
        await using (var engine = CreateCalmEngine(mock))
        {
            engine.Start();
            using var test = new TestClass(engine, Logger);
            engine.Command.Register<TestCommandWithResponse, TestResponse>(test.HandleTestCommandWithResponseAsync);

            // Act & Assert
            var command = new TestCommandWithResponse("Execute", "Result", async (_, _) =>
            {
                await Task.Yield();
                throw new InvalidOperationException("Async failure");
            });
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                Task.Run(() => engine.Command.SendAsync(command, TestCtxCT), TestCtxCT));
        }

        // Verify: Exception should NOT be reported to OnUnhandledException (root UoW)
        mock.Verify(x => x.OnUnhandledException(It.IsAny<Exception>()), Times.Never);
        mock.Verify(x => x.OnContextLeaked(), Times.Never);
    }
    #endregion

    #region CommandWithResponse - Nested call
    /// <summary>
    /// Verifies that synchronous exceptions in nested CommandWithResponse from engine thread are propagated to the caller
    /// and reported to OnUnhandledException.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task CommandWithResponseFromEngineThreadNestedSyncExceptionShouldPropagateAndNotify()
    {
        var TestCtxCT = TestContext.Current.CancellationToken;

        // Arrange
        var mock = new Mock<ICalmErrorObserver>();
        await using (var engine = CreateCalmEngine(mock))
        {
            engine.Start();
            using var test = new TestClass(engine, Logger);
            engine.Command.Register<TestCommandWithResponse, TestResponse>(test.HandleTestCommandWithResponseAsync);

            // Act & Assert
            var parentCommand = new TestCommandWithResponse("Parent", "Result", async (_, token) =>
            {
                var nestedCommand = new TestCommandWithResponse("Nested", "Result", (_, _) => throw new InvalidOperationException("Nested sync failure"));
                await engine.Command.SendAsync(nestedCommand, token);
            });
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                engine.Command.SendAsync(parentCommand, TestCtxCT));
        }

        // Verify: Exception SHOULD be reported to OnUnhandledException (nested)
        mock.Verify(x => x.OnUnhandledException(
            It.Is<InvalidOperationException>(e => e.Message == "Nested sync failure")), Times.Once);
        mock.Verify(x => x.OnContextLeaked(), Times.Never);
    }

    /// <summary>
    /// Verifies that asynchronous exceptions in nested CommandWithResponse from engine thread are propagated to the caller
    /// and reported to OnUnhandledException.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task CommandWithResponseFromEngineThreadNestedAsyncExceptionShouldPropagateAndNotify()
    {
        var TestCtxCT = TestContext.Current.CancellationToken;

        // Arrange
        var mock = new Mock<ICalmErrorObserver>();
        await using (var engine = CreateCalmEngine(mock))
        {
            engine.Start();
            using var test = new TestClass(engine, Logger);
            engine.Command.Register<TestCommandWithResponse, TestResponse>(test.HandleTestCommandWithResponseAsync);

            // Act & Assert
            var parentCommand = new TestCommandWithResponse("Parent", "Result", async (_, token) =>
            {
                var nestedCommand = new TestCommandWithResponse("Nested", "Result", async (_, _) =>
                {
                    await Task.Yield();
                    throw new InvalidOperationException("Nested async failure");
                });
                await engine.Command.SendAsync(nestedCommand, token);
            });
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                engine.Command.SendAsync(parentCommand, TestCtxCT));
        }

        // Verify: Exception SHOULD be reported to OnUnhandledException (nested)
        mock.Verify(x => x.OnUnhandledException(
            It.Is<InvalidOperationException>(e => e.Message == "Nested async failure")), Times.Once);
        mock.Verify(x => x.OnContextLeaked(), Times.Never);
    }

    /// <summary>
    /// Verifies that synchronous exceptions in nested CommandWithResponse from external thread are propagated to the caller
    /// and reported to OnUnhandledException.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task CommandWithResponseFromExternalThreadNestedSyncExceptionShouldPropagateAndNotify()
    {
        var TestCtxCT = TestContext.Current.CancellationToken;

        // Arrange
        var mock = new Mock<ICalmErrorObserver>();
        await using (var engine = CreateCalmEngine(mock))
        {
            engine.Start();
            using var test = new TestClass(engine, Logger);
            engine.Command.Register<TestCommandWithResponse, TestResponse>(test.HandleTestCommandWithResponseAsync);

            // Act & Assert
            var parentCommand = new TestCommandWithResponse("Parent", "Result", async (_, token) =>
            {
                var nestedCommand = new TestCommandWithResponse("Nested", "Result",
                    (_, _) => throw new InvalidOperationException("Nested sync failure"));
                await engine.Command.SendAsync(nestedCommand, token);
            });
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                Task.Run(() => engine.Command.SendAsync(parentCommand, TestCtxCT), TestCtxCT));
        }

        // Verify: Exception SHOULD be reported to OnUnhandledException (nested)
        mock.Verify(x => x.OnUnhandledException(
            It.Is<InvalidOperationException>(e => e.Message == "Nested sync failure")), Times.Once);
        mock.Verify(x => x.OnContextLeaked(), Times.Never);
    }

    /// <summary>
    /// Verifies that asynchronous exceptions in nested CommandWithResponse from external thread are propagated to the caller
    /// and reported to OnUnhandledException.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task CommandWithResponseFromExternalThreadNestedAsyncExceptionShouldPropagateAndNotify()
    {
        var TestCtxCT = TestContext.Current.CancellationToken;

        // Arrange
        var mock = new Mock<ICalmErrorObserver>();
        await using (var engine = CreateCalmEngine(mock))
        {
            engine.Start();
            using var test = new TestClass(engine, Logger);
            engine.Command.Register<TestCommandWithResponse, TestResponse>(test.HandleTestCommandWithResponseAsync);

            // Act & Assert
            var parentCommand = new TestCommandWithResponse("Parent", "Result", async (_, token) =>
            {
                var nestedCommand = new TestCommandWithResponse("Nested", "Result", async (_, _) =>
                {
                    await Task.Yield();
                    throw new InvalidOperationException("Nested async failure");
                });
                await engine.Command.SendAsync(nestedCommand, token);
            });
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                Task.Run(() => engine.Command.SendAsync(parentCommand, TestCtxCT), TestCtxCT));
        }

        // Verify: Exception SHOULD be reported to OnUnhandledException (nested)
        mock.Verify(x => x.OnUnhandledException(
            It.Is<InvalidOperationException>(e => e.Message == "Nested async failure")), Times.Once);
        mock.Verify(x => x.OnContextLeaked(), Times.Never);
    }
    #endregion
    #endregion

    #region Query
    #region Query - Root call
    /// <summary>
    /// Verifies that synchronous exceptions in Query from engine thread (root) are propagated to the caller
    /// and not reported to OnUnhandledException.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task QueryFromEngineThreadRootSyncExceptionShouldPropagateToCaller()
    {
        var TestCtxCT = TestContext.Current.CancellationToken;

        // Arrange
        var mock = new Mock<ICalmErrorObserver>();
        await using (var engine = CreateCalmEngine(mock))
        {
            engine.Start();
            using var test = new TestClass(engine, Logger);
            engine.Query.Register<TestQuery, TestResponse>(test.HandleTestQueryAsync);

            // Act & Assert
            var query = new TestQuery(42, "Result", (_, _) => throw new InvalidOperationException("Sync failure"));
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                engine.Query.SendAsync(query, TestCtxCT));
        }

        // Verify: Exception should NOT be reported to OnUnhandledException (root, no UoW)
        mock.Verify(x => x.OnUnhandledException(It.IsAny<Exception>()), Times.Never);
        mock.Verify(x => x.OnContextLeaked(), Times.Never);
    }

    /// <summary>
    /// Verifies that asynchronous exceptions in Query from engine thread (root) are propagated to the caller
    /// and not reported to OnUnhandledException.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task QueryFromEngineThreadRootAsyncExceptionShouldPropagateToCaller()
    {
        var TestCtxCT = TestContext.Current.CancellationToken;

        // Arrange
        var mock = new Mock<ICalmErrorObserver>();
        await using (var engine = CreateCalmEngine(mock))
        {
            engine.Start();
            using var test = new TestClass(engine, Logger);
            engine.Query.Register<TestQuery, TestResponse>(test.HandleTestQueryAsync);

            // Act & Assert
            var query = new TestQuery(42, "Result", async (_, _) =>
            {
                await Task.Yield();
                throw new InvalidOperationException("Async failure");
            });
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                engine.Query.SendAsync(query, TestCtxCT));
        }

        // Verify: Exception should NOT be reported to OnUnhandledException (root, no UoW)
        mock.Verify(x => x.OnUnhandledException(It.IsAny<Exception>()), Times.Never);
        mock.Verify(x => x.OnContextLeaked(), Times.Never);
    }

    /// <summary>
    /// Verifies that synchronous exceptions in Query from external thread (root) are propagated to the caller
    /// and not reported to OnUnhandledException.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task QueryFromExternalThreadRootSyncExceptionShouldPropagateToCaller()
    {
        var TestCtxCT = TestContext.Current.CancellationToken;

        // Arrange
        var mock = new Mock<ICalmErrorObserver>();
        await using (var engine = CreateCalmEngine(mock))
        {
            engine.Start();
            using var test = new TestClass(engine, Logger);
            engine.Query.Register<TestQuery, TestResponse>(test.HandleTestQueryAsync);

            // Act & Assert
            var query = new TestQuery(42, "Result", (_, _) => throw new InvalidOperationException("Sync failure"));
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                Task.Run(() => engine.Query.SendAsync(query, TestCtxCT), TestCtxCT));
        }

        // Verify: Exception should NOT be reported to OnUnhandledException (root, no UoW)
        mock.Verify(x => x.OnUnhandledException(It.IsAny<Exception>()), Times.Never);
        mock.Verify(x => x.OnContextLeaked(), Times.Never);
    }

    /// <summary>
    /// Verifies that asynchronous exceptions in Query from external thread (root) are propagated to the caller
    /// and not reported to OnUnhandledException.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task QueryFromExternalThreadRootAsyncExceptionShouldPropagateToCaller()
    {
        var TestCtxCT = TestContext.Current.CancellationToken;

        // Arrange
        var mock = new Mock<ICalmErrorObserver>();
        await using (var engine = CreateCalmEngine(mock))
        {
            engine.Start();
            using var test = new TestClass(engine, Logger);
            engine.Query.Register<TestQuery, TestResponse>(test.HandleTestQueryAsync);

            // Act & Assert
            var query = new TestQuery(42, "Result", async (_, _) =>
            {
                await Task.Yield();
                throw new InvalidOperationException("Async failure");
            });
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                Task.Run(() => engine.Query.SendAsync(query, TestCtxCT), TestCtxCT));
        }

        // Verify: Exception should NOT be reported to OnUnhandledException (root, no UoW)
        mock.Verify(x => x.OnUnhandledException(It.IsAny<Exception>()), Times.Never);
        mock.Verify(x => x.OnContextLeaked(), Times.Never);
    }
    #endregion

    #region Query - Nested call
    /// <summary>
    /// Verifies that synchronous exceptions in nested Query from engine thread are propagated to the caller
    /// and reported to OnUnhandledException.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task QueryFromEngineThreadNestedSyncExceptionShouldPropagateAndNotify()
    {
        var TestCtxCT = TestContext.Current.CancellationToken;

        // Arrange
        var mock = new Mock<ICalmErrorObserver>();
        await using (var engine = CreateCalmEngine(mock))
        {
            engine.Start();
            using var test = new TestClass(engine, Logger);
            engine.Command.Register<TestCommand>(test.HandleTestCommandAsync);
            engine.Query.Register<TestQuery, TestResponse>(test.HandleTestQueryAsync);

            // Act & Assert
            var parentCommand = new TestCommand("Parent", async (_, token) =>
            {
                var query = new TestQuery(42, "Result",
                    (_, _) => throw new InvalidOperationException("Nested sync failure"));
                await engine.Query.SendAsync(query, token);
            });
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                engine.Command.SendAsync(parentCommand, TestCtxCT));
        }

        // Verify: Exception SHOULD be reported to OnUnhandledException (nested)
        mock.Verify(x => x.OnUnhandledException(
            It.Is<InvalidOperationException>(e => e.Message == "Nested sync failure")), Times.Once);
        mock.Verify(x => x.OnContextLeaked(), Times.Never);
    }

    /// <summary>
    /// Verifies that asynchronous exceptions in nested Query from engine thread are propagated to the caller
    /// and reported to OnUnhandledException.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task QueryFromEngineThreadNestedAsyncExceptionShouldPropagateAndNotify()
    {
        var TestCtxCT = TestContext.Current.CancellationToken;

        // Arrange
        var mock = new Mock<ICalmErrorObserver>();
        await using (var engine = CreateCalmEngine(mock))
        {
            engine.Start();
            using var test = new TestClass(engine, Logger);
            engine.Command.Register<TestCommand>(test.HandleTestCommandAsync);
            engine.Query.Register<TestQuery, TestResponse>(test.HandleTestQueryAsync);

            // Act & Assert
            var parentCommand = new TestCommand("Parent", async (_, token) =>
            {
                var query = new TestQuery(42, "Result", async (_, _) =>
                {
                    await Task.Yield();
                    throw new InvalidOperationException("Nested async failure");
                });
                await engine.Query.SendAsync(query, token);
            });
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                engine.Command.SendAsync(parentCommand, TestCtxCT));
        }

        // Verify: Exception SHOULD be reported to OnUnhandledException (nested)
        mock.Verify(x => x.OnUnhandledException(
            It.Is<InvalidOperationException>(e => e.Message == "Nested async failure")), Times.Once);
        mock.Verify(x => x.OnContextLeaked(), Times.Never);
    }

    /// <summary>
    /// Verifies that synchronous exceptions in nested Query from external thread are propagated to the caller
    /// and reported to OnUnhandledException.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task QueryFromExternalThreadNestedSyncExceptionShouldPropagateAndNotify()
    {
        var TestCtxCT = TestContext.Current.CancellationToken;

        // Arrange
        var mock = new Mock<ICalmErrorObserver>();
        await using (var engine = CreateCalmEngine(mock))
        {
            engine.Start();
            using var test = new TestClass(engine, Logger);
            engine.Command.Register<TestCommand>(test.HandleTestCommandAsync);
            engine.Query.Register<TestQuery, TestResponse>(test.HandleTestQueryAsync);

            // Act & Assert
            var parentCommand = new TestCommand("Parent", async (_, token) =>
            {
                var query = new TestQuery(42, "Result",
                    (_, _) => throw new InvalidOperationException("Nested sync failure"));
                await engine.Query.SendAsync(query, token);
            });
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                Task.Run(() => engine.Command.SendAsync(parentCommand, TestCtxCT), TestCtxCT));
        }

        // Verify: Exception SHOULD be reported to OnUnhandledException (nested)
        mock.Verify(x => x.OnUnhandledException(
            It.Is<InvalidOperationException>(e => e.Message == "Nested sync failure")), Times.Once);
        mock.Verify(x => x.OnContextLeaked(), Times.Never);
    }

    /// <summary>
    /// Verifies that asynchronous exceptions in nested Query from external thread are propagated to the caller
    /// and reported to OnUnhandledException.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task QueryFromExternalThreadNestedAsyncExceptionShouldPropagateAndNotify()
    {
        var TestCtxCT = TestContext.Current.CancellationToken;

        // Arrange
        var mock = new Mock<ICalmErrorObserver>();
        await using (var engine = CreateCalmEngine(mock))
        {
            engine.Start();
            using var test = new TestClass(engine, Logger);
            engine.Command.Register<TestCommand>(test.HandleTestCommandAsync);
            engine.Query.Register<TestQuery, TestResponse>(test.HandleTestQueryAsync);

            // Act & Assert
            var parentCommand = new TestCommand("Parent", async (_, token) =>
            {
                var query = new TestQuery(42, "Result", async (_, _) =>
                {
                    await Task.Yield();
                    throw new InvalidOperationException("Nested async failure");
                });
                await engine.Query.SendAsync(query, token);
            });
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                Task.Run(() => engine.Command.SendAsync(parentCommand, TestCtxCT), TestCtxCT));
        }

        // Verify: Exception SHOULD be reported to OnUnhandledException (nested)
        mock.Verify(x => x.OnUnhandledException(
            It.Is<InvalidOperationException>(e => e.Message == "Nested async failure")), Times.Once);
        mock.Verify(x => x.OnContextLeaked(), Times.Never);
    }
    #endregion
    #endregion

    #region Event
    #region Event - Root call (Fire-and-forget)
    /// <summary>
    /// Verifies that synchronous exceptions in Event from engine thread (root) are reported to OnUnhandledException.
    /// Event is fire-and-forget, so exceptions cannot be propagated to the caller.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task EventFromEngineThreadRootSyncExceptionShouldNotifyObserver()
    {
        var TestCtxCT = TestContext.Current.CancellationToken;

        // Arrange
        var mock = new Mock<ICalmErrorObserver>();
        await using (var engine = CreateCalmEngine(mock))
        {
            engine.Start();
            using var test = new TestClass(engine, Logger);
            engine.Event.Register<TestEvent>(test.HandleTestEventAsync);

            // Act
            var eventMessage = new TestEvent("Hello",
                (_, _) => throw new InvalidOperationException("Sync failure"));
            engine.Event.Publish(eventMessage, TestCtxCT);

            // Wait for event processing
            await test.WaitUntilNoActiveHandlersAsync(TestCtxCT);
        }

        // Verify: Exception SHOULD be reported to OnUnhandledException (fire-and-forget)
        mock.Verify(x => x.OnUnhandledException(
            It.Is<InvalidOperationException>(e => e.Message == "Sync failure")), Times.Once);
        mock.Verify(x => x.OnContextLeaked(), Times.Never);
    }

    /// <summary>
    /// Verifies that asynchronous exceptions in Event from engine thread (root) are reported to OnUnhandledException.
    /// Event is fire-and-forget, so exceptions cannot be propagated to the caller.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task EventFromEngineThreadRootAsyncExceptionShouldNotifyObserver()
    {
        var TestCtxCT = TestContext.Current.CancellationToken;

        // Arrange
        var mock = new Mock<ICalmErrorObserver>();
        await using (var engine = CreateCalmEngine(mock))
        {
            engine.Start();
            using var test = new TestClass(engine, Logger);
            engine.Event.Register<TestEvent>(test.HandleTestEventAsync);

            // Act
            var eventMessage = new TestEvent("Hello", async (_, _) =>
            {
                await Task.Yield();
                throw new InvalidOperationException("Async failure");
            });
            engine.Event.Publish(eventMessage, TestCtxCT);

            // Wait for event processing
            await test.WaitUntilNoActiveHandlersAsync(TestCtxCT);
        }

        // Verify: Exception SHOULD be reported to OnUnhandledException (fire-and-forget)
        mock.Verify(x => x.OnUnhandledException(
            It.Is<InvalidOperationException>(e => e.Message == "Async failure")), Times.Once);
        mock.Verify(x => x.OnContextLeaked(), Times.Never);
    }

    /// <summary>
    /// Verifies that synchronous exceptions in Event from external thread (root) are reported to OnUnhandledException.
    /// Event is fire-and-forget, so exceptions cannot be propagated to the caller.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task EventFromExternalThreadRootSyncExceptionShouldNotifyObserver()
    {
        var TestCtxCT = TestContext.Current.CancellationToken;

        // Arrange
        var mock = new Mock<ICalmErrorObserver>();
        await using (var engine = CreateCalmEngine(mock))
        {
            engine.Start();
            using var test = new TestClass(engine, Logger);
            engine.Event.Register<TestEvent>(test.HandleTestEventAsync);

            // Act
            var eventMessage = new TestEvent("Hello", (_, _) => throw new InvalidOperationException("Sync failure"));
            await Task.Run(() => engine.Event.Publish(eventMessage, TestCtxCT), TestCtxCT);

            // Wait for event processing
            await test.WaitUntilNoActiveHandlersAsync(TestCtxCT);
        }

        // Verify: Exception SHOULD be reported to OnUnhandledException (fire-and-forget)
        mock.Verify(x => x.OnUnhandledException(
            It.Is<InvalidOperationException>(e => e.Message == "Sync failure")), Times.Once);
        mock.Verify(x => x.OnContextLeaked(), Times.Never);
    }

    /// <summary>
    /// Verifies that asynchronous exceptions in Event from external thread (root) are reported to OnUnhandledException.
    /// Event is fire-and-forget, so exceptions cannot be propagated to the caller.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task EventFromExternalThreadRootAsyncExceptionShouldNotifyObserver()
    {
        var TestCtxCT = TestContext.Current.CancellationToken;

        // Arrange
        var mock = new Mock<ICalmErrorObserver>();
        await using (var engine = CreateCalmEngine(mock))
        {
            engine.Start();
            using var test = new TestClass(engine, Logger);
            engine.Event.Register<TestEvent>(test.HandleTestEventAsync);

            // Act
            var eventMessage = new TestEvent("Hello", async (_, _) =>
            {
                await Task.Yield();
                throw new InvalidOperationException("Async failure");
            });
            await Task.Run(() => engine.Event.Publish(eventMessage, TestCtxCT), TestCtxCT);

            // Wait for event processing
            await test.WaitUntilNoActiveHandlersAsync(TestCtxCT);
        }

        // Verify: Exception SHOULD be reported to OnUnhandledException (fire-and-forget)
        mock.Verify(x => x.OnUnhandledException(
            It.Is<InvalidOperationException>(e => e.Message == "Async failure")), Times.Once);
        mock.Verify(x => x.OnContextLeaked(), Times.Never);
    }
    #endregion

    #region Event - Nested call (via outbox)
    /// <summary>
    /// Verifies that synchronous exceptions in nested Event from engine thread are reported to OnUnhandledException.
    /// Events published within a command are deferred to outbox and flushed on success.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task EventFromEngineThreadNestedSyncExceptionShouldNotifyObserver()
    {
        var TestCtxCT = TestContext.Current.CancellationToken;

        // Arrange
        var mock = new Mock<ICalmErrorObserver>();
        await using (var engine = CreateCalmEngine(mock))
        {
            engine.Start();
            using var test = new TestClass(engine, Logger);
            engine.Command.Register<TestCommand>(test.HandleTestCommandAsync);
            engine.Event.Register<TestEvent>(test.HandleTestEventAsync);

            // Act
            var parentCommand = new TestCommand("Parent", async (_, token) =>
            {
                var eventMessage = new TestEvent("From Command",
                    (_, _) => throw new InvalidOperationException("Nested sync failure"));
                engine.Event.Publish(eventMessage, token);
            });
            await engine.Command.SendAsync(parentCommand, TestCtxCT);

            // Wait for event processing
            await test.WaitUntilNoActiveHandlersAsync(TestCtxCT);
        }

        // Verify: Exception SHOULD be reported to OnUnhandledException (outbox execution)
        mock.Verify(x => x.OnUnhandledException(
            It.Is<InvalidOperationException>(e => e.Message == "Nested sync failure")), Times.Once);
        mock.Verify(x => x.OnContextLeaked(), Times.Never);
    }

    /// <summary>
    /// Verifies that asynchronous exceptions in nested Event from engine thread are reported to OnUnhandledException.
    /// Events published within a command are deferred to outbox and flushed on success.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task EventFromEngineThreadNestedAsyncExceptionShouldNotifyObserver()
    {
        var TestCtxCT = TestContext.Current.CancellationToken;

        // Arrange
        var mock = new Mock<ICalmErrorObserver>();
        await using (var engine = CreateCalmEngine(mock))
        {
            engine.Start();
            using var test = new TestClass(engine, Logger);
            engine.Command.Register<TestCommand>(test.HandleTestCommandAsync);
            engine.Event.Register<TestEvent>(test.HandleTestEventAsync);

            // Act
            var parentCommand = new TestCommand("Parent", async (_, token) =>
            {
                var eventMessage = new TestEvent("From Command", async (_, _) =>
                {
                    await Task.Yield();
                    throw new InvalidOperationException("Nested async failure");
                });
                engine.Event.Publish(eventMessage, token);
            });
            await engine.Command.SendAsync(parentCommand, TestCtxCT);

            // Wait for event processing
            await test.WaitUntilNoActiveHandlersAsync(TestCtxCT);
        }

        // Verify: Exception SHOULD be reported to OnUnhandledException (outbox execution)
        mock.Verify(x => x.OnUnhandledException(
            It.Is<InvalidOperationException>(e => e.Message == "Nested async failure")), Times.Once);
        mock.Verify(x => x.OnContextLeaked(), Times.Never);
    }

    /// <summary>
    /// Verifies that synchronous exceptions in nested Event from external thread are reported to OnUnhandledException.
    /// Events published within a command are deferred to outbox and flushed on success.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task EventFromExternalThreadNestedSyncExceptionShouldNotifyObserver()
    {
        var TestCtxCT = TestContext.Current.CancellationToken;

        // Arrange
        var mock = new Mock<ICalmErrorObserver>();
        await using (var engine = CreateCalmEngine(mock))
        {
            engine.Start();
            using var test = new TestClass(engine, Logger);
            engine.Command.Register<TestCommand>(test.HandleTestCommandAsync);
            engine.Event.Register<TestEvent>(test.HandleTestEventAsync);

            // Act
            var parentCommand = new TestCommand("Parent", async (_, token) =>
            {
                var eventMessage = new TestEvent("From Command",
                    (_, _) => throw new InvalidOperationException("Nested sync failure"));
                engine.Event.Publish(eventMessage, token);
            });
            await Task.Run(() => engine.Command.SendAsync(parentCommand, TestCtxCT), TestCtxCT);

            // Wait for event processing
            await test.WaitUntilNoActiveHandlersAsync(TestCtxCT);
        }

        // Verify: Exception SHOULD be reported to OnUnhandledException (outbox execution)
        mock.Verify(x => x.OnUnhandledException(
            It.Is<InvalidOperationException>(e => e.Message == "Nested sync failure")), Times.Once);
        mock.Verify(x => x.OnContextLeaked(), Times.Never);
    }

    /// <summary>
    /// Verifies that asynchronous exceptions in nested Event from external thread are reported to OnUnhandledException.
    /// Events published within a command are deferred to outbox and flushed on success.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task EventFromExternalThreadNestedAsyncExceptionShouldNotifyObserver()
    {
        var TestCtxCT = TestContext.Current.CancellationToken;

        // Arrange
        var mock = new Mock<ICalmErrorObserver>();
        await using (var engine = CreateCalmEngine(mock))
        {
            engine.Start();
            using var test = new TestClass(engine, Logger);
            engine.Command.Register<TestCommand>(test.HandleTestCommandAsync);
            engine.Event.Register<TestEvent>(test.HandleTestEventAsync);

            // Act
            var parentCommand = new TestCommand("Parent", async (_, token) =>
            {
                var eventMessage = new TestEvent("From Command", async (_, _) =>
                {
                    await Task.Yield();
                    throw new InvalidOperationException("Nested async failure");
                });
                engine.Event.Publish(eventMessage, token);
            });
            await Task.Run(() => engine.Command.SendAsync(parentCommand, TestCtxCT), TestCtxCT);

            // Wait for event processing
            await test.WaitUntilNoActiveHandlersAsync(TestCtxCT);
        }

        // Verify: Exception SHOULD be reported to OnUnhandledException (outbox execution)
        mock.Verify(x => x.OnUnhandledException(
            It.Is<InvalidOperationException>(e => e.Message == "Nested async failure")), Times.Once);
        mock.Verify(x => x.OnContextLeaked(), Times.Never);
    }
    #endregion
    #endregion
}
