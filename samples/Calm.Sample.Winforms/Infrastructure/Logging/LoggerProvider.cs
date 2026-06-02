using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Extensions.Logging;
using System.Collections.Concurrent;
using System.Globalization;
#if DEBUG
using Serilog.Formatting.Display;
using Serilog.Sinks.PeriodicBatching;
using System.Text;
#endif

namespace Calm.Sample.Winforms.Infrastructure.Logging;

/// <summary>
/// A logger provider.
/// </summary>
/// <param name="logLevel">The log level.</param>
internal sealed class LoggerProvider(LogLevel logLevel) : ILoggerProvider
{
    /// <summary>
    /// An entry for a shared provider.
    /// </summary>
    /// <param name="provider">The provider.</param>
    /// <param name="levelSwitch">The level switch.</param>
    private sealed class SharedProviderEntry(SerilogLoggerProvider provider, LoggingLevelSwitch levelSwitch)
    {
        /// <summary>
        /// Gets the provider.
        /// </summary>
        public SerilogLoggerProvider Provider { get; } = provider;

        /// <summary>
        /// Gets the level switch.
        /// </summary>
        public LoggingLevelSwitch LevelSwitch { get; } = levelSwitch;
    }

    /// <summary>
    /// The shared serilog logger providers.
    /// </summary>
    private static readonly ConcurrentDictionary<string, SharedProviderEntry> _sharedProviders
        = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// The log level.
    /// </summary>
    public LogLevel LogLevel { get; set; } = logLevel;

    /// <summary>
    /// A message template.
    /// </summary>
    private readonly string _outputTemplate =
        "[{Timestamp:HH:mm:ss.fff} {LogLevel:u3}][{ThreadName,14}:{ThreadId,3}] {Message:lj}{NewLine}{Exception}";

    /// <summary>
    /// The log file path.
    /// </summary>
    public string? FilePath { get; set; }

    /// <inheritdoc/>
    public void Dispose()
    {
        // Shared providers are not disposed here to allow sharing across multiple LoggerProvider instances.
        // They will be disposed when the process exits.
    }

    /// <summary>
    /// Writes all logs in the buffer and closes the connection.
    /// </summary>
    public static void FlushAndShutdown()
    {
        foreach (var provider in _sharedProviders.Values.Select(e => e.Provider))
        {
            provider?.Dispose();
        }
        Log.CloseAndFlush();
    }

    /// <inheritdoc/>
    public Microsoft.Extensions.Logging.ILogger CreateLogger(string categoryName)
    {
        var entry = _sharedProviders.GetOrAdd(FilePath ?? "", static (filePath, self) =>
        {
            var levelSwitch = new LoggingLevelSwitch(GetLogEventLevel(self.LogLevel));
            var config = new LoggerConfiguration()
                .MinimumLevel.ControlledBy(levelSwitch)
                .Enrich.With(new LogLevelEnricher())
                .Enrich.WithThreadId()
                .Enrich.WithThreadName();
#if DEBUG
            config = config.WriteTo.Sink(new PeriodicBatchingSink(
                new DebugSink(self._outputTemplate, CultureInfo.InvariantCulture),
                new PeriodicBatchingSinkOptions
                {
                    BatchSizeLimit = 1000,
                    Period = TimeSpan.FromSeconds(5),
                    QueueLimit = null
                }));
#endif
            if (!string.IsNullOrEmpty(filePath))
            {
                var dir = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrWhiteSpace(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                config = config.WriteTo.File(
                    filePath,
                    outputTemplate: self._outputTemplate,
                    formatProvider: CultureInfo.InvariantCulture,
                    fileSizeLimitBytes: 1024 * 1024,
                    buffered: true,
                    //shared: true,
                    flushToDiskInterval: TimeSpan.FromSeconds(5),
                    rollOnFileSizeLimit: true,
                    retainedFileCountLimit: 2);
            }

            var provider = new SerilogLoggerProvider(config.CreateLogger(), true);
            return new SharedProviderEntry(provider, levelSwitch);
        }, this);

        // If a more detailed log level is requested, update the existing switch.
        var requestedLevel = GetLogEventLevel(LogLevel);
        if (requestedLevel < entry.LevelSwitch.MinimumLevel)
        {
            entry.LevelSwitch.MinimumLevel = requestedLevel;
        }
        return entry.Provider.CreateLogger(categoryName);
    }

    /// <summary>
    /// Gets the <see cref="LogEventLevel"/> corresponding to the current value.
    /// </summary>
    /// <param name="level">The log level.</param>
    /// <returns>The <see cref="LogEventLevel"/> corresponding to the current value.</returns>
    /// <exception cref="InvalidOperationException">There is no corresponding log level.</exception>
    private static LogEventLevel GetLogEventLevel(LogLevel level)
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

    /// <summary>
    /// Enriches log events with a log level.
    /// </summary>
    private sealed class LogLevelEnricher : ILogEventEnricher
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
#if DEBUG
    /// <summary>
    /// A custom Serilog log sink that receives log events
    /// and writes them to <see cref="System.Diagnostics.Debug"/>
    /// </summary>
    /// <param name="outputTemplate">The text template for the log output.</param>
    /// <param name="formatProvider">The provider to use for formatting the message.</param>
    private sealed class DebugSink(string outputTemplate, IFormatProvider formatProvider)
        : Serilog.Sinks.PeriodicBatching.IBatchedLogEventSink
    {
        /// <inheritdoc/>
        public async Task EmitBatchAsync(IEnumerable<LogEvent> batch)
        {
            await Task.Run(async () =>
            {
                var formatter = new MessageTemplateTextFormatter(outputTemplate, formatProvider);
                var sb = new StringBuilder();
                var writer = new StringWriter(sb);
                await using (writer.ConfigureAwait(false))
                {
                    foreach (var logEvent in batch)
                    {
                        formatter.Format(logEvent, writer);
                    }
                }
                System.Diagnostics.Debug.Write(sb.ToString());
            }).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public Task OnEmptyBatchAsync()
        {
            return Task.CompletedTask;
        }
    }
#endif
}
