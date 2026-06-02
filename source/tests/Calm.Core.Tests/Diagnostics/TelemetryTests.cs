using Microsoft.Extensions.Logging;
using Moq;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Metrics;
using Xunit;

namespace Calm.Core.Tests.Diagnostics;

/// <summary>
/// Tests for OpenTelemetry instrumentation in CalmEngine.
/// </summary>
public class TelemetryTests() : TestBase(LogLevel.Trace)
{
    /// <summary>
    /// Verifies that the engine records Activities and Metrics during message processing.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    [SuppressMessage("Performance", "CA1849:Call async methods when in an async method",
        Justification = "Since processing is being intentionally blocked, this is excluded.")]
    [SuppressMessage("Major Code Smell", "S6966:Awaitable method should be used",
        Justification = "Used intentionally for testing purposes.")]
    public async Task EngineRecordsActivityAndMetrics()
    {
        var TestCtxCT = TestContext.Current.CancellationToken;

        // ActivityListener and MeterListener are available via System.Diagnostics.DiagnosticSource package
        // even on .NET Framework.

        var activityRecorded = false;
        var metricRecorded = false;

        // Setup ActivityListener
        using var activityListener = new ActivityListener
        {
            ShouldListenTo = source => string.Equals(source.Name, "Calm.Core", StringComparison.Ordinal),
            Sample = (ref _) => ActivitySamplingResult.AllData,
            ActivityStarted = activity =>
            {
                if (string.Equals(activity.OperationName, "CalmPump.Process", StringComparison.Ordinal))
                {
                    activityRecorded = true;
                }
            }
        };
        ActivitySource.AddActivityListener(activityListener);

        // Setup MeterListener
        using var meterListener = new MeterListener();
        meterListener.InstrumentPublished = (instrument, listener) =>
        {
            if (string.Equals(instrument.Meter.Name, "Calm.Core", StringComparison.Ordinal)
                && string.Equals(instrument.Name, "calm.engine.processing_duration", StringComparison.Ordinal))
            {
                listener.EnableMeasurementEvents(instrument);
            }
        };
        meterListener.SetMeasurementEventCallback<double>((instrument, _, _, _) =>
        {
            if (string.Equals(instrument.Name, "calm.engine.processing_duration", StringComparison.Ordinal))
            {
                metricRecorded = true;
            }
        });
        meterListener.Start();

        // Run Engine Pump directly for low-level telemetry testing
        var mock = new Mock<ICalmErrorObserver>();
        await using (var engine = CreateCalmEngine(mock))
        {
            var operation = engine.Schedule(_ =>
            {
                Sleep(10);
                return Task.CompletedTask;
            }, TestCtxCT);

            // Post a stop signal after the work
            engine.Schedule(token =>
            {
#pragma warning disable VSTHRD103, MA0042 // Call async methods when in an async method
                engine.StopAsync(token).GetAwaiter().GetResult();
#pragma warning restore VSTHRD103, MA0042 // Call async methods when in an async method
                return Task.CompletedTask;
            }, TestCtxCT);

            engine.Start();

            // Wait for the work to complete
            await operation.CompletedAwaitable.ConfigureAwait(true);

            // Wait for engine to stop
            await engine.WaitForShutdownAsync(CancellationToken.None);

            // Assert
            Assert.True(activityRecorded, "Activity should have been recorded.");
            Assert.True(metricRecorded, "Metric should have been recorded.");
        }
        mock.Verify(x => x.OnUnhandledException(It.IsAny<Exception>()), Times.Never);
        mock.Verify(x => x.OnContextLeaked(), Times.Never);
    }
}
