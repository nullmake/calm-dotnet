using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Globalization;

namespace SharedLibrary;

public static class DebugLogger
{
    public static ILoggingBuilder AddSampleDebug(this ILoggingBuilder builder,
        Action<SampleDebugFormatterOptions>? configure = null)
    {
        var options = new SampleDebugFormatterOptions();
        configure?.Invoke(options);
#pragma warning disable CA2000 // Dispose objects before losing scope
        builder.AddProvider(new SampleDebugLoggerProvider(options));
#pragma warning restore CA2000 // Dispose objects before losing scope
        return builder;
    }

    private sealed class SampleDebugLoggerProvider(SampleDebugFormatterOptions Options) : ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName) => new SampleDebugLogger(categoryName, Options);

        public void Dispose()
        {
        }
    }

    private sealed class SampleDebugLogger(string CategoryName, SampleDebugFormatterOptions Options) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
            => null;

        public bool IsEnabled(LogLevel logLevel)
            => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
            Exception? exception, Func<TState, Exception?, string> formatter)
        {
            var timestampFormat = Options.TimestampFormat ?? "yy-MM-dd HH:mm:ss.fff";
            var timestamp = Options.UseUtcTimestamp ? DateTimeOffset.UtcNow : DateTimeOffset.Now;

            var formattedLog = string.Format(CultureInfo.InvariantCulture,
                "[{0} {1}][{2,14}:{3,2}]{4} {5}",
                timestamp.ToString(timestampFormat, CultureInfo.InvariantCulture),
                SampleConsoleFormatter.GetLogLevelShortName(logLevel),
                Thread.CurrentThread.Name,
                Environment.CurrentManagedThreadId,
                Options.UseCategory ? ' ' + CategoryName + ':' : "",
                formatter?.Invoke(state, exception));

            if (exception is not null)
            {
                formattedLog += Environment.NewLine + exception.ToString();
            }

            Debug.WriteLine(formattedLog);
        }
    }
}

#region Custom formatter
public sealed class SampleDebugFormatterOptions
{
    public string? TimestampFormat { get; set; }
    public bool UseUtcTimestamp { get; set; }
    public bool UseCategory { get; set; } = true;
}
#endregion
