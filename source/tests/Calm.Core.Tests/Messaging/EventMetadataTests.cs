using Calm.Core.Messaging.Bus;
using Calm.Core.Tests.TestClasses;
using Calm.Core.Tests.TestClasses.Messages;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Calm.Core.Tests.Messaging;

/// <summary>
/// Provides tests for message metadata behavior, including immediate publication and log suppression.
/// </summary>
public class EventMetadataTests : TestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EventMetadataTests"/> class.
    /// </summary>
    public EventMetadataTests()
        : base(LogLevel.Trace)
    {
    }

    /// <summary>
    /// Verifies that an event marked with [CalmImmediate] bypasses the outbox even within a Unit of Work.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task ImmediateEventBypassesOutbox()
    {
        var TestCtxCT = TestContext.Current.CancellationToken;

        var mock = new Mock<ICalmErrorObserver>();
        await using (var engine = CreateCalmEngine(mock))
        {
            engine.Start();

            var handler = new TestMetadataClass();
            engine.Register(handler);

            await engine.ExecuteAsync(async _ =>
            {
                engine.Event.Publish(new TestImmediateEvent(), TestCtxCT);
                engine.Event.Publish(new TestEvent("Normal"), TestCtxCT);

                // Since we are ON the engine thread, the immediate event is scheduled via Scheduler.Schedule.
                // It won't execute until the current segment (this lambda) finishes.
                // However, the Normal event is in the Outbox and will be executed
                // by ExecuteRootUoWAsync AFTER all segments.
                Assert.False(handler.NormalCalled, "Normal event should be in Outbox");
            }, TestCtxCT);

            // After UoW completes, normal event should be called.
            await WaitForIdleAsync(engine, TestCtxCT);

            Assert.True(handler.ImmediateCalled);
            Assert.True(handler.NormalCalled);
        }
        mock.Verify(x => x.OnUnhandledException(It.IsAny<Exception>()), Times.Never);
        mock.Verify(x => x.OnContextLeaked(), Times.Never);
    }

    /// <summary>
    /// Verifies that the message metadata is correctly cached.
    /// </summary>
    [Fact]
    public void MetadataIsCached()
    {
        var meta1 = CalmMessageMetadata.Get(typeof(TestImmediateEvent));
        var meta2 = CalmMessageMetadata.Get(typeof(TestImmediateEvent));

        Assert.Same(meta1, meta2);
        Assert.True(meta1.Immediate);
        Assert.False(meta1.SuppressLog);
    }

    /// <summary>
    /// Verifies that an event marked with [CalmSuppressLog] has the correct metadata.
    /// </summary>
    [Fact]
    public void QuietEventHasSuppressLogTrue()
    {
        var meta = CalmMessageMetadata.Get(typeof(TestQuietEvent));
        Assert.True(meta.SuppressLog);
        Assert.False(meta.Immediate);
    }

    /// <summary>
    /// Verifies that a command marked with [CalmSuppressLog] has the correct metadata.
    /// </summary>
    [Fact]
    public void QuietCommandHasSuppressLogTrue()
    {
        var meta = CalmMessageMetadata.Get(typeof(TestQuietCommand));
        Assert.True(meta.SuppressLog);
        Assert.False(meta.Immediate);
    }
}
