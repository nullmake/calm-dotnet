using Calm.Core;
using Calm.Extensions.DependencyInjection;
using Calm.Sample.Winforms.Infrastructure.Application;
using Calm.Sample.Winforms.Infrastructure.Logging;
using Calm.Sample.Winforms.Models;
using Calm.Sample.Winforms.ViewModels;
using Calm.Sample.Winforms.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ReactiveUI.Builder;
using Splat;
using Splat.Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace Calm.Sample.Winforms;

/// <summary>
/// Interaction logic for <see cref="Program"/>
/// </summary>
internal sealed class App : IDisposable, ICalmErrorObserver
{
    /// <summary>
    /// The logger instance.
    /// </summary>
    private readonly Microsoft.Extensions.Logging.ILogger _logger;

    /// <summary>
    /// The application options.
    /// </summary>
    public Options Options { get; }

    /// <summary>
    /// The service provider of the dependency injection.
    /// </summary>
    [SuppressMessage("Usage", "CA2213:Disposable fields should be disposed",
        Justification = "False Positive")]
    private ServiceProvider? _serviceProvider;

    /// <summary>
    /// The scope of the <see cref="MainForm"/>.
    /// </summary>
    private IServiceScope? _mainFormScope;

    /// <summary>
    /// Initializes a new instance of the <see cref="App"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="options">The application options.</param>
    public App(Microsoft.Extensions.Logging.ILogger logger, Options options)
    {
        _logger = logger;
        Options = options;

        // Logs application information.
        logger.LogInformation("Application information={Information}", CurrentApplication.Default.ToString());
    }

    #region IDisposable
    /// <summary>
    /// Indicates whether the object has been disposed.
    /// </summary>
    private bool _disposed;

    /// <inheritdoc/>
    [SuppressMessage("Usage", "VSTHRD002:Avoid problematic synchronous waits",
        Justification = "Because asynchronous methods cannot be used.")]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types",
        Justification = "Do not throw exceptions from within `Dispose`.")]
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        if (Application.MessageLoop)
        {
            try
            {
                _mainFormScope?.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to dispose MainForm scope.");
            }
        }
        if (_serviceProvider is not null)
        {
            Task.Run(async () =>
            {
                try
                {
                    var timeout = Task.Delay(TimeSpan.FromSeconds(30));
                    await Task.WhenAny(_serviceProvider.DisposeAsync().AsTask(), timeout).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to dispose MainForm scope.");
                }
            }).GetAwaiter().GetResult();
        }
        _disposed = true;
    }
    #endregion

    /// <summary>
    /// Build a service.
    /// </summary>
    public void ConfigureServices()
    {
        IServiceCollection services = new ServiceCollection();

        // Application options.
        services.AddSingleton(Options);

        // Logging
        services.AddLogging(configure =>
        {
            configure
                .ClearProviders()
                .SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace)
                .AddProvider(new LoggerProvider(Options.LogLevel)
                {
                    FilePath = Options.LogFile
                });
        });
#if false
        // OpenTelemetry
        var endPoint = new Uri("http://localhost:4317");
        services.AddOpenTelemetry()
            .WithTracing(configure =>
            {
                configure
                    .AddSource("Calm.Core")
                    .AddOtlpExporter(options =>
                    {
                        options.Endpoint = endPoint;
                    });
            })
            .WithMetrics(meterProviderBuilder =>
            {
                meterProviderBuilder
                    .AddMeter("Calm.Core")
                    .AddOtlpExporter(options =>
                    {
                        options.Endpoint = endPoint;
                    });
            });
#endif
        // Calm engine
        services.AddCalm(calmOptions =>
        {
            calmOptions.ErrorObserver = this;
        });

        // Application
        services.AddView();
        services.AddViewModel();
        services.AddModel();

        // Using Microsoft.Extensions.DependencyInjection with ReactiveUI.
        services.UseMicrosoftDependencyResolver();
        Locator.CurrentMutable.InitializeSplat();

        // Initialize Splat and ReactiveUI
        RxAppBuilder.CreateReactiveUIBuilder()
            .WithWinForms()
            .BuildApp();

        // Build the provider.
        _serviceProvider = services.BuildServiceProvider();

        // Finalize Splat registration with the service provider
        _serviceProvider.UseMicrosoftDependencyResolver();
    }

    /// <summary>
    /// Begins running a standard application message loop on the current
    /// thread, and makes the specified form visible.
    /// </summary>
    /// <exception cref="InvalidOperationException">Occurs when called before initialization.</exception>
    public void Run()
    {
        if (_serviceProvider is null)
        {
            throw new InvalidOperationException("The ConfigureServices() method must be called.");
        }
#if false
        // Start OpenTelemetry
        _ = _serviceProvider.GetService<TracerProvider>();
        _ = _serviceProvider.GetService<MeterProvider>();
#endif
        // Start Calm engine.
        _serviceProvider.GetRequiredService<ICalm>().Start();

        // Create application model instance.
        _ = _serviceProvider.GetService<Model>();

        // Show main window.
        using (_mainFormScope = _serviceProvider.CreateScope())
        {
            var mainForm = _mainFormScope.ServiceProvider.GetRequiredService<MainForm>();
            Thread.CurrentThread.Name = "UI Thread";
            Application.Run(mainForm);
        }
    }

    #region ICalmErrorObserver
    /// <inheritdoc/>
    public void OnUnhandledException(Exception exception)
    {
        _logger.LogError(exception, "Detected an uncaught exception:");
        CurrentApplication.Shutdown(-103);
    }

    /// <inheritdoc/>
    public void OnStall(StallEventArgs e)
    {
        _logger.LogWarning("Long-running task detected: Duration={Duration}, Task={Task}", e.Duration, e.Task);
    }

    /// <inheritdoc/>
    public void OnContextLeaked()
    {
        _logger.LogWarning("The CalmSynchronizationContext was detected as lost.");
    }
    #endregion
}
