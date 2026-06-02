using Calm.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sample04.Models;
using Sample04.ViewModels;
using Sample04.Views;
using SharedLibrary;
using System.Windows;

namespace Sample04;
/// <summary>
/// Interaction logic for App.xaml
/// </summary>
internal sealed partial class App : Application
{
    private IHost _host = default!;

    private async void Application_Startup(object sender, StartupEventArgs e)
    {
        Thread.CurrentThread.Name = "Main Thread";

        // Create and configure a builder object.
        var builder = Host.CreateApplicationBuilder();

        // Add the logger.
        builder.Logging
            .ClearProviders()
            .AddSampleDebug();

        // Add CALM engine.
        builder.Services.AddCalm(configure =>
        {
            // Since the logger is passed from NET Generic Host, CALM engine logs are output.
            configure.EnableLogger = true;
        });

        // Add services.
        builder.Services
            .AddView()
            .AddViewModel()
            .AddModel();

        // Create a Generic Host.
        _host = builder.Build();

        // Create services with an application lifetime.
        _ = _host.Services.GetRequiredService<Model>();

        // Run the application.
        await _host.StartAsync().ConfigureAwait(false);

        // Show main window.
        using var scope = _host.Services.CreateScope();
        var mainWindow = scope.ServiceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    private async void Application_Exit(object sender, ExitEventArgs e)
    {
        // Stop the application.
        await _host.StopAsync().ConfigureAwait(false);
        _host.Dispose();
    }
}

