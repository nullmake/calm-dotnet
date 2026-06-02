using Serilog.Core;
using Serilog.Events;

namespace SharedTestCode;

/// <summary>
/// Enriches log events with a log level.
/// </summary>
internal sealed class LogLevelEnricher : ILogEventEnricher
{
    /// <inheritdoc/>
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var value = logEvent.Level switch
        {
            LogEventLevel.Verbose => "TRC",
            LogEventLevel.Debug => "DBG",
            LogEventLevel.Information => "INF",
            LogEventLevel.Warning => "WRN",
            LogEventLevel.Error => "ERR",
            LogEventLevel.Fatal => "CRT",
            _ => "(Unknown)"
        };
        logEvent.AddPropertyIfAbsent(new LogEventProperty("LogLevel", new ScalarValue(value)));
    }
}
