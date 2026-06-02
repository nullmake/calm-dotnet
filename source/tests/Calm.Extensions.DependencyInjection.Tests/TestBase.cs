using Calm.Core;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using SharedTestCode;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Calm.Extensions.DependencyInjection.Tests;

/// <summary>
/// The base class for all tests in this project, providing common setup and utilities.
/// </summary>
[SuppressMessage("Maintainability", "CA1515:Consider making public types internal",
    Justification = "Test classes and related classes must be public.")]
public abstract class TestBase
{
    /// <summary>
    /// The minimum <see cref="Microsoft.Extensions.Logging.LogLevel"/>
    /// requirement for log messages to be logged.
    /// </summary>
    private readonly LogLevel _logLevel;

    /// <summary>
    /// The test output helper used to write test output during execution.
    /// </summary>
    protected Microsoft.Extensions.Logging.ILogger Logger { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TestBase"/> class.
    /// </summary>
    /// <param name="level">The test output log level</param>
    protected TestBase(LogLevel level)
    {
        _logLevel = level;
        Logger = LoggerHelper.CreateLogger(level, GetType().Name);
    }

    /// <summary>
    /// Logs output at the start and end of a block.
    /// </summary>
    /// <param name="blockName">The block name.</param>
    /// <returns>The log output object.</returns>
    protected IDisposable BlockLog(string blockName)
    {
        Logger.LogInformation("({Name}) -->", blockName);
        return new ActionDisposable(() => Logger.LogInformation("<-- ({Name})", blockName));
    }

    /// <summary>
    /// Creates a new <see cref="IHostBuilder"/> instance.
    /// </summary>
    /// <returns>The <see cref="IHostBuilder"/>.</returns>
    protected IHostBuilder CreateTestBuilder()
        => Host.CreateDefaultBuilder()
            .ConfigureLogging(logging =>
            {
                logging
                    .SetMinimumLevel(_logLevel)
                    .AddFilter("Default", _logLevel)
                    .AddDebug()
                    .AddSerilog(LoggerHelper.CreateSerilog(_logLevel.ToLogEventLevel()));
            });

    /// <summary>
    /// Wait for the engine is idle that is no tasks are currently executing.
    /// </summary>
    /// <param name="engine">The calm engine.</param>
    /// <param name="token">The cancel token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="InvalidOperationException">Failed to get Calm non-public instance.</exception>
    protected async Task WaitForIdleAsync(ICalm engine, CancellationToken token)
    {
        Logger.LogInformation("[WaitForIdleAsync] Starting.");
        var pump = typeof(CalmEngine).GetField("_pump", BindingFlags.NonPublic | BindingFlags.Instance)?
            .GetValue(engine)
            ?? throw new InvalidOperationException();

        var result = Type.GetType("Calm.Core.Engines.CalmPump, Calm.Core")?
            .GetMethod("WaitForShutdownCalmTaskAsync", BindingFlags.NonPublic | BindingFlags.Instance)?
            .Invoke(pump, [token])
            ?? throw new InvalidOperationException();

        await (Task)result;
        Logger.LogInformation("[WaitForIdleAsync] Finished.");
    }
}
