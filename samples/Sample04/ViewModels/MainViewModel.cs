using Calm.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using Sample04.Models.Bus.Commands;
using Sample04.Models.Bus.Events;
using Sample04.Models.Bus.Queries;

namespace Sample04.ViewModels;

internal sealed partial class MainViewModel : ObservableObject
{
    private readonly ICalm _calm;
    private readonly ILogger _logger;

    [ObservableProperty]
    public partial string ApplicationName { get; private set; }

    [ObservableProperty]
    public partial string ApplicationVersion { get; private set; }

    [ObservableProperty]
    public partial double SamplingPeriod { get; set; }

    [ObservableProperty]
    public partial double CpuUsage { get; private set; }

    [ObservableProperty]
    public partial long PrivateBytes { get; private set; }

    [ObservableProperty]
    public partial long WorkingSet { get; private set; }

    [ObservableProperty]
    public partial long VirtualMermory { get; private set; }

    [ObservableProperty]
    public partial int HandleCount { get; private set; }

    [ObservableProperty]
    public partial long GcHeapSize { get; private set; }

    public MainViewModel(ICalm calm, ILogger<MainViewModel> logger)
    {
        _calm = calm;
        _logger = logger;

        var result = calm.Query.SendAsync(new ApplicationInfoQuery()).GetAwaiter().GetResult();
        ApplicationName = result.Name;
        ApplicationVersion = result.Version;
        SamplingPeriod = 1;
    }

    partial void OnSamplingPeriodChanged(double oldValue, double newValue)
    {
        try
        {
            var command = new ChangeSamplingPeriodCommand(TimeSpan.FromSeconds(newValue));
            _calm.Command.SendAsync(command).GetAwaiter().GetResult();
            _logger.LogInformation("Changed SamplingPeriod {Old:0.00} -> {New:0.00}", oldValue, newValue);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to change sampling speed.");
            throw;
        }
    }

    [CalmHandler]
    private Task HandleProcessResourceUpdatedEventAsync(ProcessResourceUpdatedEvent @event, CancellationToken token)
    {
        _logger.LogInformation("Handle event: {Event}", @event);

        CpuUsage = @event.CpuUsage;
        PrivateBytes = @event.PrivateBytes;
        WorkingSet = @event.WorkingSet;
        VirtualMermory = @event.VirtualMermory;
        HandleCount = @event.HandleCount;
        GcHeapSize = @event.GcHeapSize;
        return Task.CompletedTask;
    }
}
