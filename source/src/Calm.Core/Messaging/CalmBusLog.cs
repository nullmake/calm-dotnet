using Calm.Core.Diagnostics;
using Calm.Core.Messaging.Handlers;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

namespace Calm.Core.Messaging;

/// <summary>
/// Logging methods for CalmPump, using source generators for high performance and structured logging.
/// </summary>
/// <param name="logger">The logger instance to use for logging messages.</param>
internal sealed partial class CalmBusLog(ILogger logger) : CalmLog(logger)
{
    #region LogMethods
    /// <summary>
    /// Message information.
    /// </summary>
    /// <param name="logger">The logger instance</param>
    /// <param name="level">The log level.</param>
    /// <param name="memberName">caller member name.</param>
    /// <param name="fileName">caller file path.</param>
    /// <param name="lineNumber">caller line number.</param>
    /// <param name="message">The log message.</param>
    /// <param name="messageTypeName">The name of message type.</param>
    [LoggerMessage(
        EventId = BaseEventId | ((int)CalmLogModule.CalmBus << 16) | 0,
        Message = LogTemplate + " {{ Message={MessageTypeName} }}")]
    private static partial void MessageInfo(
        ILogger logger, LogLevel level, string memberName, string fileName, int lineNumber,
        string message, string messageTypeName);

    /// <summary>
    /// Message and Method information.
    /// </summary>
    /// <param name="logger">The logger instance</param>
    /// <param name="level">The log level.</param>
    /// <param name="memberName">caller member name.</param>
    /// <param name="fileName">caller file path.</param>
    /// <param name="lineNumber">caller line number.</param>
    /// <param name="message">The log message.</param>
    /// <param name="messageTypeName">The name of message type.</param>
    /// <param name="methodTypeName">The name of method type.</param>
    [LoggerMessage(
        EventId = BaseEventId | ((int)CalmLogModule.CalmBus << 16) | 1,
        Message = LogTemplate + " {{ Message={MessageTypeName}, Method={methodTypeName} }}")]
    private static partial void MessageAndMethodInfo(
        ILogger logger, LogLevel level, string memberName, string fileName, int lineNumber,
        string message, string messageTypeName, string methodTypeName);

#if false
    /// <summary>
    /// Handler information.
    /// </summary>
    /// <param name="logger">The logger instance</param>
    /// <param name="level">The log level.</param>
    /// <param name="memberName">caller member name.</param>
    /// <param name="fileName">caller file path.</param>
    /// <param name="lineNumber">caller line number.</param>
    /// <param name="message">The log message.</param>
    /// <param name="messageTypeName">The name of message type.</param>
    /// <param name="responseTypeName">The name of return type.</param>
    [LoggerMessage(
        EventId = BaseEventId | ((int)CalmLogModule.CalmBus << 16) | 2,
        Message = LogTemplate + " {{ Name={MessageTypeName}<{responseTypeName}> }}")]
    private static partial void MessageInfo(
        ILogger logger, LogLevel level, string memberName, string fileName, int lineNumber,
        string message, string messageTypeName, string responseTypeName);
#endif

    /// <summary>
    /// Handler information.
    /// </summary>
    /// <param name="logger">The logger instance</param>
    /// <param name="level">The log level.</param>
    /// <param name="memberName">caller member name.</param>
    /// <param name="fileName">caller file path.</param>
    /// <param name="lineNumber">caller line number.</param>
    /// <param name="message">The log message.</param>
    /// <param name="methodName">The method name.</param>
    /// <param name="handlerTypeName">The name of message handler type.</param>
    /// <param name="messageTypeName">The name of message type.</param>
    [LoggerMessage(
        EventId = BaseEventId | ((int)CalmLogModule.CalmBus << 16) | 3,
        Message = LogPrefix + "[{MethodName}] {Message}"
            + " {{ Name={HandlerTypeName}<{MessageTypeName}> }}")]
    private static partial void RegisterUnregisterInfo(
        ILogger logger, LogLevel level, string memberName, string fileName, int lineNumber,
        string message, string methodName, string? handlerTypeName, string messageTypeName);

    /// <summary>
    /// Handler information.
    /// </summary>
    /// <param name="logger">The logger instance</param>
    /// <param name="level">The log level.</param>
    /// <param name="memberName">caller member name.</param>
    /// <param name="fileName">caller file path.</param>
    /// <param name="lineNumber">caller line number.</param>
    /// <param name="message">The log message.</param>
    /// <param name="methodName">The method name.</param>
    /// <param name="handlerTypeName">The name of message handler type.</param>
    /// <param name="messageTypeName">The name of message type.</param>
    /// <param name="responseTypeName">The name of return type.</param>
    [LoggerMessage(
        EventId = BaseEventId | ((int)CalmLogModule.CalmBus << 16) | 4,
        Message = LogPrefix + "[{MethodName}] {Message}"
            + " {{ Name={HandlerTypeName}<{MessageTypeName}, {responseTypeName}> }}")]
    private static partial void RegisterUnregisterInfo(
        ILogger logger, LogLevel level, string memberName, string fileName, int lineNumber,
        string message, string methodName, string handlerTypeName, string messageTypeName, string responseTypeName);

    /// <summary>
    /// Dispatch information.
    /// </summary>
    /// <param name="logger">The logger instance</param>
    /// <param name="level">The log level.</param>
    /// <param name="memberName">caller member name.</param>
    /// <param name="fileName">caller file path.</param>
    /// <param name="lineNumber">caller line number.</param>
    /// <param name="logMessage">The message.</param>
    /// <param name="methodName">The method name.</param>
    /// <param name="messageObject">The message object.</param>
    [LoggerMessage(
        EventId = BaseEventId | ((int)CalmLogModule.CalmBus << 16) | 5,
        Message = LogPrefix + "[{MethodName}] {LogMessage} Message={{ {MessageObject} }}")]
    private static partial void DispatchInfo(
        ILogger logger, LogLevel level, string memberName, string fileName, int lineNumber,
        string logMessage, string methodName, object messageObject);

    /// <summary>
    /// The message of the flushing all deferred events in the outbox.
    /// </summary>
    /// <param name="logger">The logger instance</param>
    /// <param name="level">The log level.</param>
    /// <param name="memberName">caller member name.</param>
    /// <param name="fileName">caller file path.</param>
    /// <param name="lineNumber">caller line number.</param>
    /// <param name="count">The number of the deferred events.</param>
    [LoggerMessage(
        EventId = BaseEventId | ((int)CalmLogModule.CalmBus << 16) | 6,
        Message = LogPrefix + " Flush all deferred events in the outbox. Count={Count}")]
    private static partial void FlushAllDeferredEvents(
        ILogger logger, LogLevel level, string memberName, string fileName, int lineNumber,
        int count);
    #endregion

    #region RegisteringAllHandler
    /// <summary>
    /// Log for handler registration information.
    /// </summary>
    /// <param name="logLevel">The log level.</param>
    /// <param name="target">The handler instance containing methods marked
    /// with <see cref="CalmHandlerAttribute"/>.</param>
    /// <param name="memberName">Automatically populated caller member name.</param>
    /// <param name="filePath">Automatically populated caller file path.</param>
    /// <param name="lineNumber">Automatically populated caller line number.</param>
    public void RegisteringAllHandler(LogLevel logLevel, string target,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        if (Logger.IsEnabled(logLevel))
        {
            var fileName = Path.GetFileName(filePath);
            WriteLine(Logger, logLevel, memberName, fileName, lineNumber,
                "Registering all handler of the " + target);
        }
    }
    #endregion

    #region RegisteredAllHandler
    /// <summary>
    /// Log for handler registration information.
    /// </summary>
    /// <param name="logLevel">The log level.</param>
    /// <param name="target">The handler instance containing methods marked
    /// with <see cref="CalmHandlerAttribute"/>.</param>
    /// <param name="memberName">Automatically populated caller member name.</param>
    /// <param name="filePath">Automatically populated caller file path.</param>
    /// <param name="lineNumber">Automatically populated caller line number.</param>
    public void RegisteredAllHandler(LogLevel logLevel, string target,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        if (Logger.IsEnabled(logLevel))
        {
            var fileName = Path.GetFileName(filePath);
            WriteLine(Logger, logLevel, memberName, fileName, lineNumber,
                "Registered all handler of the " + target);
        }
    }
    #endregion

    #region RegisteringHandler
    /// <summary>
    /// Log for handler registration information.
    /// </summary>
    /// <param name="logLevel">The log level.</param>
    /// <param name="messageHandler">The message handler.</param>
    /// <param name="memberName">Automatically populated caller member name.</param>
    /// <param name="filePath">Automatically populated caller file path.</param>
    /// <param name="lineNumber">Automatically populated caller line number.</param>
    public void RegisteringHandler(LogLevel logLevel, IMessageHandler messageHandler,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        if (Logger.IsEnabled(logLevel))
        {
            var fileName = Path.GetFileName(filePath);
            RegisterUnregisterInfo(Logger, logLevel, memberName, fileName, lineNumber,
                "Registering handler", "RegisterHandler",
                messageHandler.Name, messageHandler.MessageType.Name);
        }
    }

    /// <summary>
    /// Log for handler registration information.
    /// </summary>
    /// <param name="logLevel">The log level.</param>
    /// <param name="requestHandler">The delegate handler type.</param>
    /// <param name="memberName">Automatically populated caller member name.</param>
    /// <param name="filePath">Automatically populated caller file path.</param>
    /// <param name="lineNumber">Automatically populated caller line number.</param>
    public void RegisteringHandler(LogLevel logLevel, IRequestHandler requestHandler,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        if (Logger.IsEnabled(logLevel))
        {
            var fileName = Path.GetFileName(filePath);
            RegisterUnregisterInfo(Logger, logLevel, memberName, fileName, lineNumber,
                "Registering handler", "RegisterHandler",
                requestHandler.Name, requestHandler.RequestType.Name, requestHandler.ResponseType.Name);
        }
    }
    #endregion

    #region RegisteredHandler
    /// <summary>
    /// Log for handler registration information.
    /// </summary>
    /// <param name="logLevel">The log level.</param>
    /// <param name="messageHandler">The message handler.</param>
    /// <param name="memberName">Automatically populated caller member name.</param>
    /// <param name="filePath">Automatically populated caller file path.</param>
    /// <param name="lineNumber">Automatically populated caller line number.</param>
    public void RegisteredHandler(LogLevel logLevel, IMessageHandler messageHandler,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        if (Logger.IsEnabled(logLevel))
        {
            var fileName = Path.GetFileName(filePath);
            RegisterUnregisterInfo(Logger, logLevel, memberName, fileName, lineNumber,
                "Registered handler", "RegisterHandler",
                messageHandler.Name, messageHandler.MessageType.Name);
        }
    }

    /// <summary>
    /// Log for handler registration information.
    /// </summary>
    /// <param name="logLevel">The log level.</param>
    /// <param name="requestHandler">The delegate handler type.</param>
    /// <param name="memberName">Automatically populated caller member name.</param>
    /// <param name="filePath">Automatically populated caller file path.</param>
    /// <param name="lineNumber">Automatically populated caller line number.</param>
    public void RegisteredHandler(LogLevel logLevel, IRequestHandler requestHandler,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        if (Logger.IsEnabled(logLevel))
        {
            var fileName = Path.GetFileName(filePath);
            RegisterUnregisterInfo(Logger, logLevel, memberName, fileName, lineNumber,
                "Registered handler", "RegisterHandler",
                requestHandler.Name, requestHandler.RequestType.Name, requestHandler.ResponseType.Name);
        }
    }
    #endregion

    #region UnregisteringAllHandler
    /// <summary>
    /// Log for handler registration information.
    /// </summary>
    /// <param name="logLevel">The log level.</param>
    /// <param name="target">The handler instance containing methods marked
    /// with <see cref="CalmHandlerAttribute"/>.</param>
    /// <param name="memberName">Automatically populated caller member name.</param>
    /// <param name="filePath">Automatically populated caller file path.</param>
    /// <param name="lineNumber">Automatically populated caller line number.</param>
    public void UnregisteringAllHandler(LogLevel logLevel, string target,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        if (Logger.IsEnabled(logLevel))
        {
            var fileName = Path.GetFileName(filePath);
            WriteLine(Logger, logLevel, memberName, fileName, lineNumber,
                "Unregistering all handler of the " + target);
        }
    }
    #endregion

    #region UnregisteredAllHandler
    /// <summary>
    /// Log for handler registration information.
    /// </summary>
    /// <param name="logLevel">The log level.</param>
    /// <param name="target">The handler instance containing methods marked
    /// with <see cref="CalmHandlerAttribute"/>.</param>
    /// <param name="memberName">Automatically populated caller member name.</param>
    /// <param name="filePath">Automatically populated caller file path.</param>
    /// <param name="lineNumber">Automatically populated caller line number.</param>
    public void UnregisteredAllHandler(LogLevel logLevel, string target,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        if (Logger.IsEnabled(logLevel))
        {
            var fileName = Path.GetFileName(filePath);
            WriteLine(Logger, logLevel, memberName, fileName, lineNumber,
                "Unregistered all handler of the " + target);
        }
    }
    #endregion

    #region UnregisteringHandler
    /// <summary>
    /// Log for handler unregistration information.
    /// </summary>
    /// <param name="logLevel">The log level.</param>
    /// <param name="method">The callback methos of the message type.</param>
    /// <param name="messageType">The message type.</param>
    /// <param name="memberName">Automatically populated caller member name.</param>
    /// <param name="filePath">Automatically populated caller file path.</param>
    /// <param name="lineNumber">Automatically populated caller line number.</param>
    public void UnregisteringHandler(LogLevel logLevel, Delegate method, Type messageType,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        if (Logger.IsEnabled(logLevel))
        {
            var fileName = Path.GetFileName(filePath);
            RegisterUnregisterInfo(Logger, logLevel, memberName, fileName, lineNumber,
                "Unegistering handler", "UnregisterHandler", method.Method.Name, messageType.Name);
        }
    }

    /// <summary>
    /// Log for handler unregistration information.
    /// </summary>
    /// <param name="logLevel">The log level.</param>
    /// <param name="method">The callback methos of the message type.</param>
    /// <param name="messageType">The message type.</param>
    /// <param name="responseType">The response type.</param>
    /// <param name="memberName">Automatically populated caller member name.</param>
    /// <param name="filePath">Automatically populated caller file path.</param>
    /// <param name="lineNumber">Automatically populated caller line number.</param>
    public void UnregisteringHandler(LogLevel logLevel, Delegate method, Type messageType, Type responseType,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        if (Logger.IsEnabled(logLevel))
        {
            var fileName = Path.GetFileName(filePath);
            RegisterUnregisterInfo(Logger, logLevel, memberName, fileName, lineNumber,
                "Unegistering handler", "UnregisterHandler",
                method.Method.Name, messageType.Name, responseType.Name);
        }
    }
    #endregion

    #region UnregisteredHandler
    /// <summary>
    /// Log for handler unregistration information.
    /// </summary>
    /// <param name="logLevel">The log level.</param>
    /// <param name="method">The callback methos of the message type.</param>
    /// <param name="messageType">The message type.</param>
    /// <param name="memberName">Automatically populated caller member name.</param>
    /// <param name="filePath">Automatically populated caller file path.</param>
    /// <param name="lineNumber">Automatically populated caller line number.</param>
    public void UnregisteredHandler(LogLevel logLevel, Delegate method, Type messageType,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        if (Logger.IsEnabled(logLevel))
        {
            var fileName = Path.GetFileName(filePath);
            RegisterUnregisterInfo(Logger, logLevel, memberName, fileName, lineNumber,
                "Unregistered handler", "UnregisterHandler", method.Method.Name, messageType.Name);
        }
    }

    /// <summary>
    /// Log for handler unregistration information.
    /// </summary>
    /// <param name="logLevel">The log level.</param>
    /// <param name="method">The callback methos of the message type.</param>
    /// <param name="messageType">The message type.</param>
    /// <param name="responseType">The response type.</param>
    /// <param name="memberName">Automatically populated caller member name.</param>
    /// <param name="filePath">Automatically populated caller file path.</param>
    /// <param name="lineNumber">Automatically populated caller line number.</param>
    public void UnregisteredHandler(LogLevel logLevel, Delegate method, Type messageType, Type responseType,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        if (Logger.IsEnabled(logLevel))
        {
            var fileName = Path.GetFileName(filePath);
            RegisterUnregisterInfo(Logger, logLevel, memberName, fileName, lineNumber,
                "Unregistered handler", "UnregisterHandler",
                method.Method.Name, messageType.Name, responseType.Name);
        }
    }
    #endregion

#if false
    #region MessageInfo
    /// <summary>
    /// Message with message type.
    /// </summary>
    /// <param name="logLevel">The log level.</param>
    /// <param name="message">The message.</param>
    /// <param name="messageType">The message type.</param>
    /// <param name="memberName">Automatically populated caller member name.</param>
    /// <param name="filePath">Automatically populated caller file path.</param>
    /// <param name="lineNumber">Automatically populated caller line number.</param>
    public void MessageInfo(LogLevel logLevel, string message, Type messageType,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        if (Logger.IsEnabled(logLevel))
        {
            var fileName = Path.GetFileName(filePath);
            MessageInfo(Logger, logLevel, memberName, fileName, lineNumber,
                message, messageType.Name);
        }
    }

    /// <summary>
    /// Message with message type.
    /// </summary>
    /// <param name="logLevel">The log level.</param>
    /// <param name="message">The message.</param>
    /// <param name="messageType">The message type.</param>
    /// <param name="responseType">The message response type.</param>
    /// <param name="memberName">Automatically populated caller member name.</param>
    /// <param name="filePath">Automatically populated caller file path.</param>
    /// <param name="lineNumber">Automatically populated caller line number.</param>
    public void MessageInfo(LogLevel logLevel, string message, Type messageType, Type responseType,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        if (Logger.IsEnabled(logLevel))
        {
            var fileName = Path.GetFileName(filePath);
            MessageInfo(Logger, logLevel, memberName, fileName, lineNumber,
                message, messageType.Name, responseType.Name);
        }
    }
    #endregion

    #region MessageAndMethodInfo
    /// <summary>
    /// Message with message type and method type.
    /// </summary>
    /// <param name="logLevel">The log level.</param>
    /// <param name="message">The message.</param>
    /// <param name="messageType">The message type.</param>
    /// <param name="methodType">The method type.</param>
    /// <param name="memberName">Automatically populated caller member name.</param>
    /// <param name="filePath">Automatically populated caller file path.</param>
    /// <param name="lineNumber">Automatically populated caller line number.</param>
    public void MessageAndMethodInfo(LogLevel logLevel, string message, Type messageType, Type methodType,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        if (Logger.IsEnabled(logLevel))
        {
            var fileName = Path.GetFileName(filePath);
            MessageAndMethodInfo(Logger, logLevel, memberName, fileName, lineNumber,
                message, messageType.Name, methodType.Name);
        }
    }
    #endregion
#endif

    #region IgnoreNullRegistation
    /// <summary>
    /// Message indicating that registration failed because the handler is null.
    /// </summary>
    /// <param name="logLevel">The log level.</param>
    /// <param name="messageType">The message type.</param>
    /// <param name="memberName">Automatically populated caller member name.</param>
    /// <param name="filePath">Automatically populated caller file path.</param>
    /// <param name="lineNumber">Automatically populated caller line number.</param>
    public void IgnoreNullRegistation(LogLevel logLevel, Type messageType,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        if (Logger.IsEnabled(logLevel))
        {
            var fileName = Path.GetFileName(filePath);
            MessageInfo(Logger, logLevel, memberName, fileName, lineNumber,
                "The given handler value was null, so it was not added.", messageType.Name);
        }
    }
    #endregion

    #region NoHandlersRegistered
    /// <summary>
    /// Message for no handlers registered for a message type.
    /// </summary>
    /// <param name="logLevel">The log level.</param>
    /// <param name="messageType">The message type.</param>
    /// <param name="memberName">Automatically populated caller member name.</param>
    /// <param name="filePath">Automatically populated caller file path.</param>
    /// <param name="lineNumber">Automatically populated caller line number.</param>
    public void NoHandlersRegistered(LogLevel logLevel, Type messageType,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        if (Logger.IsEnabled(logLevel))
        {
            var fileName = Path.GetFileName(filePath);
            MessageInfo(Logger, logLevel, memberName, fileName, lineNumber,
                "No handlers registered.", messageType.Name);
        }
    }

    /// <summary>
    /// Message for no handlers registered for a message type.
    /// </summary>
    /// <param name="logLevel">The log level.</param>
    /// <param name="messageType">The message type.</param>
    /// <param name="method">The method.</param>
    /// <param name="memberName">Automatically populated caller member name.</param>
    /// <param name="filePath">Automatically populated caller file path.</param>
    /// <param name="lineNumber">Automatically populated caller line number.</param>
    public void NoHandlersRegistered(LogLevel logLevel, Type messageType, Delegate method,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        if (Logger.IsEnabled(logLevel))
        {
            var fileName = Path.GetFileName(filePath);
            MessageAndMethodInfo(Logger, logLevel, memberName, fileName, lineNumber,
                "No handlers registered.", messageType.Name, method.GetType().Name);
        }
    }

    /// <summary>
    /// Message for no handlers registered for a message type.
    /// </summary>
    /// <param name="logLevel">The log level.</param>
    /// <param name="messageType">The message type.</param>
    /// <param name="responseType">The message response type.</param>
    /// <param name="memberName">Automatically populated caller member name.</param>
    /// <param name="filePath">Automatically populated caller file path.</param>
    /// <param name="lineNumber">Automatically populated caller line number.</param>
    public void NoHandlersRegistered(LogLevel logLevel, Type messageType, Type responseType,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        if (Logger.IsEnabled(logLevel))
        {
            var fileName = Path.GetFileName(filePath);
            MessageAndMethodInfo(Logger, logLevel, memberName, fileName, lineNumber,
                "No handlers registered.", messageType.Name, responseType.Name);
        }
    }
    #endregion

    #region DispatchInfo
    /// <summary>
    /// log for handler registration information.
    /// </summary>
    /// <param name="logLevel">The log level.</param>
    /// <param name="message">The message.</param>
    /// <param name="methodName">The method name.</param>
    /// <param name="messageObject">The message object.</param>
    /// <param name="memberName">Automatically populated caller member name.</param>
    /// <param name="filePath">Automatically populated caller file path.</param>
    /// <param name="lineNumber">Automatically populated caller line number.</param>
    public void DispatchInfo(LogLevel logLevel, string message, string methodName, object messageObject,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        if (Logger.IsEnabled(logLevel))
        {
            var fileName = Path.GetFileName(filePath);
            DispatchInfo(Logger, logLevel, memberName, fileName, lineNumber,
                message, methodName, messageObject);
        }
    }
    #endregion

    #region FlushAllDeferredEvents
    /// <summary>
    /// The message of the flushing all deferred events in the outbox.
    /// </summary>
    /// <param name="logLevel">The log level.</param>
    /// <param name="count">The number of the deferred events.</param>
    /// <param name="memberName">Automatically populated caller member name.</param>
    /// <param name="filePath">Automatically populated caller file path.</param>
    /// <param name="lineNumber">Automatically populated caller line number.</param>
    public void FlushAllDeferredEvents(LogLevel logLevel, int count,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        if (Logger.IsEnabled(logLevel))
        {
            var fileName = Path.GetFileName(filePath);
            FlushAllDeferredEvents(Logger, logLevel, memberName, fileName, lineNumber, count);
        }
    }
    #endregion
}
