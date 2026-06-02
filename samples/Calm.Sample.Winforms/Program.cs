using Calm.Sample.Winforms.Infrastructure.Application;
using Calm.Sample.Winforms.Infrastructure.Logging;
using Microsoft.Extensions.Logging;

namespace Calm.Sample.Winforms;

/// <summary>
/// The main entry point for the application.
/// </summary>
internal static class Program
{
    /// <summary>
    /// The logger instance.
    /// </summary>
    private static ILogger _logger = null!;

    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    /// <param name="args">The commandline arguments.</param>
    [STAThread]
    public static void Main(string[] args)
    {
        // Do not allow multiple instances.
        if (!CurrentApplication.TryActivateSingleInstance())
        {
            CurrentApplication.ActivateExistingInstance();
            return;
        }

        // Parses commandline arguments.
        Options options;
        try
        {
            options = new Options(args);

            // Creates the startup logger.
            _logger = CreateLogger(options.LogFile, options.LogLevel);

            Thread.CurrentThread.Name = nameof(Main);
            _logger.LogInformation("Application start.");
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, CurrentApplication.Name, MessageBoxButtons.OK, MessageBoxIcon.Information);
            throw;
        }

        try
        {
            using var app = new App(_logger, options);

            // Catching exceptions that are not handled in the application domain and asynchronous processing.
            CurrentApplication.Exit += (_, _) => app.Dispose();
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

            // Configure services.
            app.ConfigureServices();

            // Run the application.
            app.Run();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "The application failed to start up.");
            throw;
        }
        finally
        {
            _logger.LogInformation("Application exit.");
            LoggerProvider.FlushAndShutdown();
        }
    }

    /// <summary>
    /// Creates the logger.
    /// </summary>
    /// <param name="logFile">The log file.</param>
    /// <param name="logLevel">The logging level.</param>
    /// <returns>The logger instance.</returns>
    private static ILogger CreateLogger(string logFile, LogLevel logLevel)
    {
        using var provider = new LoggerProvider(logLevel)
        {
            FilePath = logFile
        };
        return provider.CreateLogger(nameof(Program));
    }

    /// <summary>
    /// An event handler that is called whenever an exception is thrown
    /// that is not handled by the application domain.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        => Shutdown(-100, e?.ExceptionObject as Exception);

    /// <summary>
    /// An event handler that is called whenever an exception is thrown
    /// that is not handled by the asynchronous processing.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    private static void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
        => Shutdown(-101, e?.Exception);

    /// <summary>
    /// Shutdown application
    /// </summary>
    /// <param name="exitCode">The exit code to return to the operating system.</param>
    /// <param name="ex">Exception object</param>
    private static void Shutdown(int exitCode, Exception? ex = null)
    {
        _logger.LogError(ex, "Detected an uncaught exception:");
        CurrentApplication.Shutdown(exitCode);
    }
}
