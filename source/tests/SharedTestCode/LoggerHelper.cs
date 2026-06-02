using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.XUnit3;
using System.Globalization;

namespace SharedTestCode;

/// <summary>
/// The Helper utility for the logging.
/// </summary>
internal static class LoggerHelper
{
    /// <summary>
    /// Creates a new <see cref="Microsoft.Extensions.Logging.ILogger"/> instance.
    /// </summary>
    /// <param name="level">Sets a minimum <see cref="Microsoft.Extensions.Logging.LogLevel"/>
    /// requirement for log messages to be logged.</param>
    /// <param name="categoryName">The category name for messages produced by the logger.</param>
    /// <returns>The <see cref="Microsoft.Extensions.Logging.ILogger"/>.</returns>
    public static Microsoft.Extensions.Logging.ILogger CreateLogger(LogLevel level, string categoryName)
    {
        if (level is LogLevel.None)
        {
            return NullLogger.Instance;
        }
        using var loggerFactory = LoggerFactory.Create(builder =>
        {

            builder
                .SetMinimumLevel(level)
                .AddFilter("Default", level)
                .AddDebug()
                .AddSerilog(CreateSerilog(level.ToLogEventLevel()));
        });
        return loggerFactory.CreateLogger(categoryName);
    }

    /// <summary>
    /// Creates a new <see cref="Serilog.Core.Logger"/> instance.
    /// </summary>
    /// <param name="level">Sets a minimum <see cref="LogEventLevel"/>
    /// requirement for log messages to be logged.</param>
    /// <returns>The <see cref="Serilog.Core.Logger"/>.</returns>
    public static Serilog.Core.Logger CreateSerilog(LogEventLevel level)
    {
        const string template =
            "[{Timestamp:HH:mm:ss.fff} {LogLevel}]"
            + "[{ThreadName,14}:{ThreadId,3}]"
            + " {Message:lj}{NewLine}{Exception}";
        return new LoggerConfiguration()
            .MinimumLevel.Is(level)
            .Enrich.With(new LogLevelEnricher())
            .Enrich.WithThreadId()
            .Enrich.WithThreadName()
            .WriteTo.XUnit3TestOutput(template, CultureInfo.InvariantCulture)
            .CreateLogger();
    }
}
