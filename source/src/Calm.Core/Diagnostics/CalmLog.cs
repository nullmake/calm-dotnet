using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Text;

namespace Calm.Core.Diagnostics;

/// <summary>
/// logging helper class for Calm.
/// </summary>
/// <param name="logger">The logger instance to use for logging messages.</param>
internal partial class CalmLog(ILogger logger)
{
    /// <summary>
    /// The logger instance for recording diagnostic information and errors.
    /// </summary>
    protected ILogger Logger { get; } = logger ?? throw new ArgumentNullException(nameof(logger));

    #region Event ID
    /// <summary>
    /// Base event ID for CalmPump logging.
    /// </summary>
    /// <remarks>
    /// EventId = BaseEventId | (moduleOffset &lt;&lt; 16) | methodOffset
    /// </remarks>
    protected const int BaseEventId = 1 << 28;
    #endregion

    #region Logging Templates
    /// <summary>
    /// Logging template prefix for all log messages.
    /// </summary>
    protected const string LogPrefix = "[CALM][{FileName}({LineNumber}){MemberName}] ";

    /// <summary>
    /// Logging template for log messages.
    /// </summary>
    protected const string LogTemplate = LogPrefix + "{Message}";
    #endregion

    #region Get message string
    /// <summary>
    /// Gets detailed information about the assembly.
    /// </summary>
    /// <returns>The assembly information.</returns>
    protected static string GetAssemblyInfo()
    {
        var asm = typeof(CalmLog).Assembly;

        var isDebug = asm.GetCustomAttribute<DebuggableAttribute>()?.IsJITOptimizerDisabled ?? false;
        var tfm = asm.GetCustomAttribute<TargetFrameworkAttribute>()?.FrameworkName;
        var infoVersion = asm.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

        var sb = new StringBuilder();

        sb.Append("  Assembly             : ").Append(asm.FullName);
        if (isDebug)
        {
            sb.AppendLine(" (Debug Build)");
        }
        else
        {
            sb.AppendLine();
        }
        sb.Append("  Target Framework     : ").AppendLine(tfm);
        sb.Append("  Informational Version: ").AppendLine(infoVersion);

        return sb.ToString();
    }

    /// <summary>
    /// Gets a detailed message from an exception.
    /// </summary>
    /// <param name="exception">The exception to extract the message from.</param>
    /// <returns>The detailed message from an exception.</returns>
    protected static string GetExpectionMessage(Exception exception)
    {
        var sb = new StringBuilder();
        GetExpectionMessage(sb, "Exception Details:", exception);
        if (exception is AggregateException aex)
        {
            foreach (var inner in aex.Flatten().InnerExceptions)
            {
                GetExpectionMessage(sb, "Inner Exception:", inner);
            }
        }
        else
        {
            if (exception.InnerException is not null)
            {
                GetExpectionMessage(sb, "Inner Exception:", exception.InnerException);
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Gets a detailed message from an exception.
    /// </summary>
    /// <param name="sb">The StringBuilder to append the message to.</param>
    /// <param name="message">A custom message to include with the exception details.</param>
    /// <param name="exception">The exception to extract the message from.</param>
    private static void GetExpectionMessage(StringBuilder sb, string message, Exception exception)
    {
        sb.AppendLine(message);
        sb.Append(exception.GetType().Name).Append(": ").AppendLine(exception.Message);
        sb.Append("TargetSite: ").AppendLine(exception.TargetSite?.DeclaringType?.FullName ?? "null");
        sb.Append("Stack Trace: ").AppendLine(exception.StackTrace ?? "null");
    }

    /// <summary>
    /// Gets a detailed message about the logger.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <returns>The detailed message from an logger.</returns>
    private static string GetLoggerInfoMessage(ILogger? logger)
        => logger is null
            ? "ILogger=null"
            : $"ILogger={logger.GetType().FullName}, MinimumLevel={GetLogLevel(logger)}";

    /// <summary>
    /// Gets the effective log level based on the provided logger's configuration.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <returns>The log level.</returns>
    private static LogLevel GetLogLevel(ILogger? logger)
    {
        if (logger is null)
        {
            return LogLevel.None;
        }
        if (logger.IsEnabled(LogLevel.Trace))
        {
            return LogLevel.Trace;
        }
        if (logger.IsEnabled(LogLevel.Debug))
        {
            return LogLevel.Debug;
        }
        if (logger.IsEnabled(LogLevel.Information))
        {
            return LogLevel.Information;
        }
        if (logger.IsEnabled(LogLevel.Warning))
        {
            return LogLevel.Warning;
        }
        if (logger.IsEnabled(LogLevel.Error))
        {
            return LogLevel.Error;
        }
        if (logger.IsEnabled(LogLevel.Critical))
        {
            return LogLevel.Critical;
        }
        return LogLevel.None;
    }
    #endregion

    #region LogMethods
    /// <summary>
    /// log method for all log levels.
    /// </summary>
    /// <param name="logger">The logger instance</param>
    /// <param name="level">The log level.</param>
    /// <param name="memberName">caller member name.</param>
    /// <param name="fileName">caller file path.</param>
    /// <param name="lineNumber">caller line number.</param>
    /// <param name="message">The message.</param>
    [LoggerMessage(
        EventId = BaseEventId | ((int)CalmLogModule.CalmLog << 16) | 0,
        Message = LogTemplate)]
    protected static partial void WriteLine(ILogger logger, LogLevel level,
        string memberName, string fileName, int lineNumber,
        string message);

    /// <summary>
    /// log method for all log levels.
    /// </summary>
    /// <param name="logger">The logger instance</param>
    /// <param name="level">The log level.</param>
    /// <param name="memberName">caller member name.</param>
    /// <param name="fileName">caller file path.</param>
    /// <param name="lineNumber">caller line number.</param>
    /// <param name="message">The message.</param>
    /// <param name="duration">The duration of the operation.</param>
    [LoggerMessage(
        EventId = BaseEventId | ((int)CalmLogModule.CalmLog << 16) | 1,
        Message = LogTemplate + " {{ Duration={Duration} }}")]
    private static partial void Duration(ILogger logger, LogLevel level,
        string memberName, string fileName, int lineNumber,
         string message, TimeSpan duration);
    #endregion

    #region WriteLine
    /// <summary>
    /// Output the message.
    /// </summary>
    /// <param name="logLevel">The log level.</param>
    /// <param name="message">The message.</param>
    /// <param name="memberName">Automatically populated caller member name.</param>
    /// <param name="filePath">Automatically populated caller file path.</param>
    /// <param name="lineNumber">Automatically populated caller line number.</param>
    public void WriteLine(LogLevel logLevel, string message,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        if (logger.IsEnabled(logLevel))
        {
            var fileName = Path.GetFileName(filePath);
            WriteLine(Logger, logLevel, memberName, fileName, lineNumber, message);
        }
    }

    /// <summary>
    /// Output the message with exception information.
    /// </summary>
    /// <param name="logLevel">The log level.</param>
    /// <param name="exception">The exception to extract the message from.</param>
    /// <param name="message">The message.</param>
    /// <param name="memberName">Automatically populated caller member name.</param>
    /// <param name="filePath">Automatically populated caller file path.</param>
    /// <param name="lineNumber">Automatically populated caller line number.</param>
    public void WriteLine(LogLevel logLevel, Exception exception, string message,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        if (logger.IsEnabled(logLevel))
        {
            var fileName = Path.GetFileName(filePath);
            var combinedMessage = message + Environment.NewLine + GetExpectionMessage(exception);
            WriteLine(Logger, logLevel, memberName, fileName, lineNumber, combinedMessage);
        }
    }
    #endregion

    #region Trace
    /// <summary>
    /// Trace Message
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="memberName">Automatically populated caller member name.</param>
    /// <param name="filePath">Automatically populated caller file path.</param>
    /// <param name="lineNumber">Automatically populated caller line number.</param>
    public void Trace(string message,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
        => WriteLine(LogLevel.Trace, message, memberName, filePath, lineNumber);

    /// <summary>
    /// Trace Message
    /// </summary>
    /// <param name="exception">The exception to extract the message from.</param>
    /// <param name="message">The message.</param>
    /// <param name="memberName">Automatically populated caller member name.</param>
    /// <param name="filePath">Automatically populated caller file path.</param>
    /// <param name="lineNumber">Automatically populated caller line number.</param>
    public void Trace(Exception exception, string message,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
        => WriteLine(LogLevel.Trace, exception, message, memberName, filePath, lineNumber);
    #endregion

    #region Debug
    /// <summary>
    /// Debug Message
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="memberName">Automatically populated caller member name.</param>
    /// <param name="filePath">Automatically populated caller file path.</param>
    /// <param name="lineNumber">Automatically populated caller line number.</param>
    public void Debug(string message,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
        => WriteLine(LogLevel.Debug, message, memberName, filePath, lineNumber);

    /// <summary>
    /// Debug Message
    /// </summary>
    /// <param name="exception">The exception to extract the message from.</param>
    /// <param name="message">The message.</param>
    /// <param name="memberName">Automatically populated caller member name.</param>
    /// <param name="filePath">Automatically populated caller file path.</param>
    /// <param name="lineNumber">Automatically populated caller line number.</param>
    public void Debug(Exception exception, string message,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
        => WriteLine(LogLevel.Debug, exception, message, memberName, filePath, lineNumber);
    #endregion

    #region Information
    /// <summary>
    /// Information Message
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="memberName">Automatically populated caller member name.</param>
    /// <param name="filePath">Automatically populated caller file path.</param>
    /// <param name="lineNumber">Automatically populated caller line number.</param>
    public void Information(string message,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
        => WriteLine(LogLevel.Information, message, memberName, filePath, lineNumber);

    /// <summary>
    /// Information Message
    /// </summary>
    /// <param name="exception">The exception to extract the message from.</param>
    /// <param name="message">The message.</param>
    /// <param name="memberName">Automatically populated caller member name.</param>
    /// <param name="filePath">Automatically populated caller file path.</param>
    /// <param name="lineNumber">Automatically populated caller line number.</param>
    public void Information(Exception exception, string message,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
        => WriteLine(LogLevel.Information, exception, message, memberName, filePath, lineNumber);
    #endregion

    #region Warning
    /// <summary>
    /// Warning Message
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="memberName">Automatically populated caller member name.</param>
    /// <param name="filePath">Automatically populated caller file path.</param>
    /// <param name="lineNumber">Automatically populated caller line number.</param>
    public void Warning(string message,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
        => WriteLine(LogLevel.Warning, message, memberName, filePath, lineNumber);

    /// <summary>
    /// Warning Message
    /// </summary>
    /// <param name="exception">The exception to extract the message from.</param>
    /// <param name="message">The message.</param>
    /// <param name="memberName">Automatically populated caller member name.</param>
    /// <param name="filePath">Automatically populated caller file path.</param>
    /// <param name="lineNumber">Automatically populated caller line number.</param>
    public void Warning(Exception exception, string message,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
        => WriteLine(LogLevel.Warning, exception, message, memberName, filePath, lineNumber);
    #endregion

    #region Error
    /// <summary>
    /// Error Message
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="memberName">Automatically populated caller member name.</param>
    /// <param name="filePath">Automatically populated caller file path.</param>
    /// <param name="lineNumber">Automatically populated caller line number.</param>
    public void Error(string message,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
        => WriteLine(LogLevel.Error, message, memberName, filePath, lineNumber);

    /// <summary>
    /// Error Message
    /// </summary>
    /// <param name="exception">The exception to extract the message from.</param>
    /// <param name="message">The message.</param>
    /// <param name="memberName">Automatically populated caller member name.</param>
    /// <param name="filePath">Automatically populated caller file path.</param>
    /// <param name="lineNumber">Automatically populated caller line number.</param>
    public void Error(Exception exception, string message,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
        => WriteLine(LogLevel.Error, exception, message, memberName, filePath, lineNumber);
    #endregion

    #region Critical
    /// <summary>
    /// Critical Message
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="memberName">Automatically populated caller member name.</param>
    /// <param name="filePath">Automatically populated caller file path.</param>
    /// <param name="lineNumber">Automatically populated caller line number.</param>
    public void Critical(string message,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
        => WriteLine(LogLevel.Critical, message, memberName, filePath, lineNumber);

    /// <summary>
    /// Critical Message
    /// </summary>
    /// <param name="exception">The exception to extract the message from.</param>
    /// <param name="message">The message.</param>
    /// <param name="memberName">Automatically populated caller member name.</param>
    /// <param name="filePath">Automatically populated caller file path.</param>
    /// <param name="lineNumber">Automatically populated caller line number.</param>
    public void Critical(Exception exception, string message,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
        => WriteLine(LogLevel.Critical, exception, message, memberName, filePath, lineNumber);
    #endregion

    #region AssemblyInfo
    /// <summary>
    /// Information log for Assembly information.
    /// </summary>
    /// <param name="logLevel">The log level to use for logging the assembly information.</param>
    /// <param name="memberName">Automatically populated caller member name.</param>
    /// <param name="filePath">Automatically populated caller file path.</param>
    /// <param name="lineNumber">Automatically populated caller line number.</param>
    public void AssemblyInfo(LogLevel logLevel,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        if (logger.IsEnabled(logLevel))
        {
            var fileName = Path.GetFileName(filePath);
            var combinedMessage = "Assembly Information:" + Environment.NewLine + GetAssemblyInfo();
            WriteLine(Logger, logLevel, memberName, fileName, lineNumber, combinedMessage);
        }
    }
    #endregion

    #region EngineOptions
    /// <summary>
    /// Information log for CalmOptions.
    /// </summary>
    /// <param name="logLevel">The log level to use for logging the assembly information.</param>
    /// <param name="options">The CalmOptions instance</param>
    /// <param name="memberName">Automatically populated caller member name.</param>
    /// <param name="filePath">Automatically populated caller file path.</param>
    /// <param name="lineNumber">Automatically populated caller line number.</param>
    [SuppressMessage("Globalization", "CA1307:Specify StringComparison for clarity",
        Justification = "Because net472 and netstandard2.0 do not support StringComparison")]
    [SuppressMessage("Usage", "MA0001:StringComparison is missing",
        Justification = "Because net472 and netstandard2.0 do not support StringComparison")]
    public void EngineOptions(LogLevel logLevel, CalmOptions? options,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        if (logger.IsEnabled(logLevel))
        {
            var fileName = Path.GetFileName(filePath);
            var optionsString = options?.ToString()
            .Replace("{ ", Environment.NewLine + "  ")
            .Replace(", ", Environment.NewLine + "  ")
            .Replace(" }", "") ?? "{}";
            var combinedMessage = "CalmOptions Information: " + optionsString;
            WriteLine(Logger, logLevel, memberName, fileName, lineNumber, combinedMessage);
        }
    }
    #endregion

    #region LoggerInfo
    /// <summary>
    /// Information log for CalmOptions.
    /// </summary>
    /// <param name="logLevel">The log level to use for logging the assembly information.</param>
    /// <param name="memberName">Automatically populated caller member name.</param>
    /// <param name="filePath">Automatically populated caller file path.</param>
    /// <param name="lineNumber">Automatically populated caller line number.</param>
    public void LoggerInfo(LogLevel logLevel,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        if (logger.IsEnabled(logLevel))
        {
            var fileName = Path.GetFileName(filePath);
            var message = "Logger Information: " + GetLoggerInfoMessage(Logger);
            WriteLine(Logger, logLevel, memberName, fileName, lineNumber, message);
        }
    }
    #endregion

    #region Duration
    /// <summary>
    /// Information Message with the duration of an operation.
    /// </summary>
    /// <param name="logLevel">The log level to use for logging the assembly information.</param>
    /// <param name="message">The message.</param>
    /// <param name="duration">The duration of the operation.</param>
    /// <param name="memberName">Automatically populated caller member name.</param>
    /// <param name="filePath">Automatically populated caller file path.</param>
    /// <param name="lineNumber">Automatically populated caller line number.</param>
    public void Duration(LogLevel logLevel, string message, TimeSpan duration,
       [CallerMemberName] string memberName = "",
       [CallerFilePath] string filePath = "",
       [CallerLineNumber] int lineNumber = 0)
        => Duration(Logger, logLevel, memberName, filePath, lineNumber, message, duration);
    #endregion

    #region Dispose
    /// <summary>
    /// Warning log for multiple dispose calls.
    /// </summary>
    /// <param name="logLevel">The log level to use for logging the assembly information.</param>
    /// <param name="memberName">Automatically populated caller member name.</param>
    /// <param name="filePath">Automatically populated caller file path.</param>
    /// <param name="lineNumber">Automatically populated caller line number.</param>
    public void AlreadyDisposed(LogLevel logLevel,
       [CallerMemberName] string memberName = "",
       [CallerFilePath] string filePath = "",
       [CallerLineNumber] int lineNumber = 0)
    {
        WriteLine(logLevel, "Attempted to dispose multiple times. Ignoring subsequent dispose call.",
            memberName, filePath, lineNumber);
    }
    #endregion
}
