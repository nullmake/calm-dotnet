using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace SharedLibrary;

public static class ConsoleLogger
{
    public static ILoggingBuilder AddSampleConsole(this ILoggingBuilder builder,
        Action<SampleConsoleFormatterOptions>? configure)
    {
        builder.AddConsoleFormatter<SampleConsoleFormatter, SampleConsoleFormatterOptions>(configure ?? (_ => { }));
        builder.AddConsole(configure =>
        {
            configure.FormatterName = SampleConsoleFormatter.FormatterName;
        });
        return builder;
    }

    private static ILoggerFactory CreateFactory(LogLevel logLevel)
        => LoggerFactory.Create(builder =>
        {
            builder.AddSampleConsole(configure =>
            {
                configure.TimestampFormat = "HH:mm:ss.fff";
                configure.UseUtcTimestamp = false;
                configure.UseCategory = true;
                configure.IncludeScopes = true;
            });
            builder.SetMinimumLevel(logLevel);
        });

    public static ILogger Create(string categoryName, LogLevel logLevel = LogLevel.Information)
    {
        using var factory = CreateFactory(logLevel);
        return factory.CreateLogger(categoryName);
    }

    public static ILogger Create(Type type, LogLevel logLevel = LogLevel.Information)
    {
        using var factory = CreateFactory(logLevel);
        return factory.CreateLogger(type);
    }

    public static ILogger<T> Create<T>(LogLevel logLevel = LogLevel.Information)
    {
        using var factory = CreateFactory(logLevel);
        return factory.CreateLogger<T>();
    }
}

#region Custom formatter
public sealed class SampleConsoleFormatterOptions : ConsoleFormatterOptions
{
    public bool UseCategory { get; set; }
}

[SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes",
    Justification = "This is used by the LoggerFactory.")]
internal sealed class SampleConsoleFormatter(IOptionsMonitor<SampleConsoleFormatterOptions> options)
    : ConsoleFormatter(FormatterName)
{
    public const string FormatterName = "Sample";
    private readonly SampleConsoleFormatterOptions _options = options.CurrentValue;

    public override void Write<TState>(in LogEntry<TState> logEntry,
        IExternalScopeProvider? scopeProvider, TextWriter textWriter)
    {
        textWriter.Write('[');

        var timestamp = (_options.UseUtcTimestamp ? DateTimeOffset.UtcNow : DateTimeOffset.Now)
            .ToString(_options.TimestampFormat ?? "yy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
        textWriter.Write(timestamp);

        textWriter.Write(' ');
        var loglevel = GetLogLevelShortName(logEntry.LogLevel);
        textWriter.Write(loglevel);
        textWriter.Write(']');

        var threadName = Thread.CurrentThread.Name ?? "(No Name)";
        var threadId = Environment.CurrentManagedThreadId;
        var color = threadName switch
        {
            "Calm Thread" => "\u001b[96m",
            _ => threadId == 1 ? "\u001b[92m" : ""
        };
        textWriter.Write("[{0}{1,14}:{2,2}\u001b[0m]", color, threadName, threadId);

        if (_options.UseCategory)
        {
            textWriter.Write(' ');
            textWriter.Write(logEntry.Category);
        }

        if (_options.IncludeScopes && scopeProvider is not null)
        {
            scopeProvider.ForEachScope((scope, tw) =>
            {
                tw.Write(" => ");
                tw.Write(scope);
            }, textWriter);
        }

        textWriter.Write(": ");
        textWriter.WriteLine(logEntry.Formatter(logEntry.State, logEntry.Exception));

        if (logEntry.Exception != null)
        {
            textWriter.WriteLine(logEntry.Exception.ToString());
        }
    }

    public static string GetLogLevelShortName(LogLevel logLevel)
        => logLevel switch
        {
            LogLevel.Trace => "TRC",
            LogLevel.Debug => "DBG",
            LogLevel.Information => "INF",
            LogLevel.Warning => "WRN",
            LogLevel.Error => "ERR",
            LogLevel.Critical => "CRT",
            _ => "???"
        };
}
#endregion
