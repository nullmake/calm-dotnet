using Microsoft.Extensions.Logging;
using Serilog.Events;

namespace SharedTestCode;

/// <summary>
/// The extensions for <see cref="Microsoft.Extensions.Logging.LogLevel"/>.
/// </summary>
internal static class LogLevelExtensions
{
    /// <summary>
    /// Gets the <see cref="LogEventLevel"/> corresponding to the current value.
    /// </summary>
    /// <param name="level">The log level.</param>
    /// <returns>The <see cref="LogEventLevel"/> corresponding to the current value.</returns>
    /// <exception cref="InvalidOperationException">There is no corresponding log level.</exception>
    public static LogEventLevel ToLogEventLevel(this LogLevel level)
        => level switch
        {
            LogLevel.Trace => LogEventLevel.Verbose,
            LogLevel.Debug => LogEventLevel.Debug,
            LogLevel.Information => LogEventLevel.Information,
            LogLevel.Warning => LogEventLevel.Warning,
            LogLevel.Error => LogEventLevel.Error,
            LogLevel.Critical => LogEventLevel.Fatal,
            LogLevel.None => throw new InvalidOperationException(),
            _ => throw new InvalidOperationException()
        };
}
