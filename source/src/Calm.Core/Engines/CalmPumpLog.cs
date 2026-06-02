using Calm.Core.Diagnostics;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Calm.Core.Engines;

/// <summary>
/// logging methods for CalmPump, using source generators for high performance and structured logging.
/// </summary>
[SuppressMessage("Design", "MA0048:File name must match type name",
    Justification = "To access members within CalmPump.")]
internal sealed partial class CalmPump
{
    /// <summary>
    /// Logging methods for CalmPump, using source generators for high performance and structured logging.
    /// </summary>
    /// <param name="logger">The logger instance to use for logging messages.</param>
    /// <param name="pump">The CalmPump instance.</param>
    private sealed partial class CalmPumpLog(ILogger logger, CalmPump pump) : CalmLog(logger)
    {
        #region Get CalmPump State
        /// <summary>
        /// The CalmPump instance.
        /// </summary>
        private readonly CalmPump _pump = pump;

        /// <summary>
        /// The number of active asynchronous operations (spanning multiple segments).
        /// </summary>
        private int ActiveOperationsCount => Volatile.Read(ref _pump._activeOperationsCount);

        /// <summary>
        /// The count of channel readers.
        /// </summary>
        private int ChannelReaderCount => _pump._channel.Reader.Count;

        /// <summary>
        /// The count of channel readers.
        /// </summary>
        private bool IsLoopActive => _pump._isLoopActive;
        #endregion

        #region Logging Templates
        /// <summary>
        /// logging template for active operations count and channel reader count.
        /// </summary>
        private const string _activeCount =
            "ActiveOpeCount={ActiveOperationsCount}"
            + ", ChannelCount={ChannelReaderCount}"
            + ", IsLoopActive={IsLoopActive}";

        /// <summary>
        /// logging template for CalmTaskInfo.
        /// </summary>
        private const string _taskInfo =
            "Task={{ Id={TaskId}, RequestedFrom={TaskFileName}({TaskLineNumber}) {TaskName} }}";
        #endregion

        #region LogMethods
        /// <summary>
        /// Message with active operations count and channel reader count.
        /// </summary>
        /// <param name="logger">The logger instance</param>
        /// <param name="level">The log level.</param>
        /// <param name="memberName">caller member name.</param>
        /// <param name="fileName">caller file path.</param>
        /// <param name="lineNumber">caller line number.</param>
        /// <param name="message">The message.</param>
        /// <param name="activeOperationsCount">The count of active operations.</param>
        /// <param name="channelReaderCount">The count of channel readers.</param>
        /// <param name="isLoopActive">Whether the engine loop is active.</param>
        [LoggerMessage(
            EventId = BaseEventId | ((int)CalmLogModule.CalmPump << 16) | 0,
            Message = LogTemplate + " {{ " + _activeCount + " }}")]
        private static partial void ActiveCountAndReaderCount(
            ILogger logger, LogLevel level, string memberName, string fileName, int lineNumber,
            string message, int activeOperationsCount, int channelReaderCount, bool isLoopActive);

        /// <summary>
        /// Message with active operations count and channel reader count.
        /// </summary>
        /// <param name="logger">The logger instance</param>
        /// <param name="level">The log level.</param>
        /// <param name="memberName">caller member name.</param>
        /// <param name="fileName">caller file path.</param>
        /// <param name="lineNumber">caller line number.</param>
        /// <param name="message">The message.</param>
        /// <param name="activeOperationsCount">The count of active operations.</param>
        /// <param name="channelReaderCount">The count of channel readers.</param>
        /// <param name="isLoopActive">Whether the engine loop is active.</param>
        /// <param name="taskId">Task ID</param>
        /// <param name="taskName">Task Name</param>
        /// <param name="taskFileName">The filename of the location where the task was requested.</param>
        /// <param name="taskLineNumber">The line number of the location where the task was requested.</param>
        [LoggerMessage(
            EventId = BaseEventId | ((int)CalmLogModule.CalmPump << 16) | 1,
            Message = LogTemplate + " {{ " + _activeCount + ", " + _taskInfo + " }}")]
        private static partial void ActiveCountAndReaderCountAndTaskInfo(
            ILogger logger, LogLevel level, string memberName, string fileName, int lineNumber,
            string message, int activeOperationsCount, int channelReaderCount, bool isLoopActive,
            Guid taskId, string taskName, string taskFileName, int taskLineNumber);

        /// <summary>
        /// Message with active operations count and channel reader count.
        /// </summary>
        /// <param name="logger">The logger instance</param>
        /// <param name="level">The log level.</param>
        /// <param name="memberName">caller member name.</param>
        /// <param name="fileName">caller file path.</param>
        /// <param name="lineNumber">caller line number.</param>
        /// <param name="exception">The exception to extract the message from.</param>
        /// <param name="message">The message.</param>
        /// <param name="activeOperationsCount">The count of active operations.</param>
        /// <param name="channelReaderCount">The count of channel readers.</param>
        /// <param name="isLoopActive">Whether the engine loop is active.</param>
        /// <param name="taskId">Task ID</param>
        /// <param name="taskName">Task Name</param>
        /// <param name="taskFileName">The filename of the location where the task was requested.</param>
        /// <param name="taskLineNumber">The line number of the location where the task was requested.</param>
        [LoggerMessage(
            EventId = BaseEventId | ((int)CalmLogModule.CalmPump << 16) | 2,
            Message = LogTemplate + " {{ " + _activeCount + ", " + _taskInfo + " }}"
                + ", {Exception}")]
        private static partial void ActiveCountAndReaderCountAndTaskInfo(
            ILogger logger, LogLevel level, string memberName, string fileName, int lineNumber,
            string exception, string message, int activeOperationsCount, int channelReaderCount, bool isLoopActive,
            Guid taskId, string taskName, string taskFileName, int taskLineNumber);

        /// <summary>
        /// Message with dispatcher method name when dispatching to engine thread.
        /// </summary>
        /// <param name="logger">The logger instance</param>
        /// <param name="level">The log level.</param>
        /// <param name="memberName">caller member name.</param>
        /// <param name="fileName">caller file path.</param>
        /// <param name="lineNumber">caller line number.</param>
        /// <param name="methodName">The dispatcher method name.</param>
        [LoggerMessage(
            EventId = BaseEventId | ((int)CalmLogModule.CalmPump << 16) | 3,
            Message = LogPrefix + "[{MethodName}] Dispatching to engine thread.")]
        private static partial void DispatchingToEngineThread(
            ILogger logger, LogLevel level, string memberName, string fileName, int lineNumber,
            string methodName);

        /// <summary>
        /// Message with dispatcher method name when dispatching to engine thread.
        /// </summary>
        /// <param name="logger">The logger instance</param>
        /// <param name="level">The log level.</param>
        /// <param name="memberName">caller member name.</param>
        /// <param name="fileName">caller file path.</param>
        /// <param name="lineNumber">caller line number.</param>
        /// <param name="methodName">The dispatcher method name.</param>
        /// <param name="returnType">The dispatcher method return type.</param>
        [LoggerMessage(
            EventId = BaseEventId | ((int)CalmLogModule.CalmPump << 16) | 4,
            Message = LogPrefix + "[{MethodName}<{ReturnType}>] Dispatching to engine thread.")]
        private static partial void DispatchingToEngineThread(
            ILogger logger, LogLevel level, string memberName, string fileName, int lineNumber,
            string methodName, string returnType);

        /// <summary>
        /// Message with dispatcher method name when dispatching to engine thread.
        /// </summary>
        /// <param name="logger">The logger instance</param>
        /// <param name="level">The log level.</param>
        /// <param name="memberName">caller member name.</param>
        /// <param name="fileName">caller file path.</param>
        /// <param name="lineNumber">caller line number.</param>
        /// <param name="methodName">The dispatcher method name.</param>
        /// <param name="delay">The delay time.</param>
        [LoggerMessage(
            EventId = BaseEventId | ((int)CalmLogModule.CalmPump << 16) | 5,
            Message = LogPrefix + "[{MethodName}] Dispatching to engine thread once {Delay} has elapsed.")]
        private static partial void DispatchingToEngineThread(
            ILogger logger, LogLevel level, string memberName, string fileName, int lineNumber,
            string methodName, TimeSpan delay);
        #endregion

        #region ActiveCountAndReaderCount
        /// <summary>
        /// Message with active operations count and channel reader count.
        /// </summary>
        /// <param name="logLevel">The log level.</param>
        /// <param name="message">The message.</param>
        /// <param name="memberName">Automatically populated caller member name.</param>
        /// <param name="filePath">Automatically populated caller file path.</param>
        /// <param name="lineNumber">Automatically populated caller line number.</param>
        public void ActiveCountAndReaderCount(LogLevel logLevel, string message,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            if (Logger.IsEnabled(logLevel))
            {
                var fileName = Path.GetFileName(filePath);
                ActiveCountAndReaderCount(Logger, logLevel, memberName, fileName, lineNumber,
                    message, ActiveOperationsCount, ChannelReaderCount, IsLoopActive);
            }
        }
        #endregion

        #region ActiveCountAndReaderCountAndTaskInfo
        /// <summary>
        /// Message with active operations count and channel reader count and CalmTaskInfo.
        /// </summary>
        /// <param name="logLevel">The log level.</param>
        /// <param name="message">The message.</param>
        /// <param name="taskInfo">The CalmTaskInfo containing task details.</param>
        /// <param name="memberName">Automatically populated caller member name.</param>
        /// <param name="filePath">Automatically populated caller file path.</param>
        /// <param name="lineNumber">Automatically populated caller line number.</param>
        public void ActiveCountAndReaderCountAndTaskInfo(LogLevel logLevel,
            string message, CalmTaskInfo taskInfo,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            if (Logger.IsEnabled(logLevel))
            {
                var fileName = Path.GetFileName(filePath);
                var taskFileName = Path.GetFileName(taskInfo.FilePath);
                ActiveCountAndReaderCountAndTaskInfo(
                    Logger, logLevel, memberName, fileName, lineNumber,
                    message, ActiveOperationsCount, ChannelReaderCount, IsLoopActive,
                    taskInfo.Id, taskInfo.Name, taskFileName, taskInfo.LineNumber);
            }
        }

        /// <summary>
        /// Message with active operations count and channel reader count and CalmTaskInfo.
        /// </summary>
        /// <param name="logLevel">The log level.</param>
        /// <param name="exception">The exception to extract the message from.</param>
        /// <param name="message">The message.</param>
        /// <param name="taskInfo">The CalmTaskInfo containing task details.</param>
        /// <param name="memberName">Automatically populated caller member name.</param>
        /// <param name="filePath">Automatically populated caller file path.</param>
        /// <param name="lineNumber">Automatically populated caller line number.</param>
        public void ActiveCountAndReaderCountAndTaskInfo(LogLevel logLevel,
            Exception exception, string message, CalmTaskInfo taskInfo,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            if (Logger.IsEnabled(logLevel))
            {
                var fileName = Path.GetFileName(filePath);
                var exceptionMessage = GetExpectionMessage(exception);
                var taskFileName = Path.GetFileName(taskInfo.FilePath);
                ActiveCountAndReaderCountAndTaskInfo(
                    Logger, logLevel, memberName, fileName, lineNumber,
                    exceptionMessage, message,
                    ActiveOperationsCount, ChannelReaderCount, IsLoopActive,
                    taskInfo.Id, taskInfo.Name, taskFileName, taskInfo.LineNumber);
            }
        }
        #endregion

        #region DispatchingToEngineThread
        /// <summary>
        /// Message with dispatcher method name when dispatching to engine thread.
        /// </summary>
        /// <param name="logLevel">The log level.</param>
        /// <param name="methodName">The dispatcher method name.</param>
        /// <param name="memberName">Automatically populated caller member name.</param>
        /// <param name="filePath">Automatically populated caller file path.</param>
        /// <param name="lineNumber">Automatically populated caller line number.</param>
        public void DispatchingToEngineThread(LogLevel logLevel, string methodName,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            if (Logger.IsEnabled(logLevel))
            {
                var fileName = Path.GetFileName(filePath);
                DispatchingToEngineThread(Logger, logLevel, memberName, fileName, lineNumber, methodName);
            }
        }

        /// <summary>
        /// Message with dispatcher method name when dispatching to engine thread.
        /// </summary>
        /// <param name="logLevel">The log level.</param>
        /// <param name="methodName">The dispatcher method name.</param>
        /// <param name="returnType">The dispatcher method return type.</param>
        /// <param name="memberName">Automatically populated caller member name.</param>
        /// <param name="filePath">Automatically populated caller file path.</param>
        /// <param name="lineNumber">Automatically populated caller line number.</param>
        public void DispatchingToEngineThread(LogLevel logLevel, string methodName, Type returnType,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            if (Logger.IsEnabled(logLevel))
            {
                var fileName = Path.GetFileName(filePath);
                DispatchingToEngineThread(Logger, logLevel, memberName, fileName, lineNumber,
                    methodName, returnType.Name);
            }
        }

        /// <summary>
        /// Message with dispatcher method name when dispatching to engine thread.
        /// </summary>
        /// <param name="logLevel">The log level.</param>
        /// <param name="methodName">The dispatcher method name.</param>
        /// <param name="delay">The delay time.</param>
        /// <param name="memberName">Automatically populated caller member name.</param>
        /// <param name="filePath">Automatically populated caller file path.</param>
        /// <param name="lineNumber">Automatically populated caller line number.</param>
        public void DispatchingToEngineThread(LogLevel logLevel, string methodName, TimeSpan delay,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            if (Logger.IsEnabled(logLevel))
            {
                var fileName = Path.GetFileName(filePath);
                DispatchingToEngineThread(Logger, logLevel, memberName, fileName, lineNumber,
                    methodName, delay);
            }
        }
        #endregion
    }
}
