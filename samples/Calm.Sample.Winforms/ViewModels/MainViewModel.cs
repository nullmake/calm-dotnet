using Calm.Core;
using Calm.Sample.Winforms.Infrastructure.Application;
using Calm.Sample.Winforms.Infrastructure.Collections;
using Calm.Sample.Winforms.Models.Bus.Commands;
using Calm.Sample.Winforms.Models.Bus.Events;
using Calm.Sample.Winforms.Models.Bus.Queries;
using Calm.Sample.Winforms.ViewModels.DataSources;
using DynamicData;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using ReactiveUI.Validation.Abstractions;
using ReactiveUI.Validation.Contexts;
using ReactiveUI.Validation.Extensions;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Calm.Sample.Winforms.ViewModels;

/// <summary>
/// The view model for the main form.
/// </summary>
[SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes",
    Justification = "Create via DI container.")]
internal sealed partial class MainViewModel : ReactiveObject, IValidatableViewModel, IDisposable
{
    /// <summary>
    /// The logger instance.
    /// </summary>
    private readonly ILogger _logger;

    /// <summary>
    /// The calm engine instance.
    /// </summary>
    private readonly ICalm _calm;

    /// <summary>
    /// A group of subscriptions to be unsubscribing.
    /// </summary>
    private readonly CompositeDisposable _subscriptions = [];

    #region Recompress archives.
    /// <summary>
    /// The folder to be search archive files which will be compressed.
    /// </summary>
    [Reactive]
    private string _folderPath = string.Empty;

    /// <summary>
    /// Determines whether the folder is searched recursively.
    /// </summary>
    [Reactive]
    private bool _isRecursive = true;

    /// <summary>
    /// Gets the command to start recompression.
    /// </summary>
    public ReactiveCommand<Unit, Unit> RecompressCommand { get; }

    /// <summary>
    /// The subject of the <see cref="RecompressProgressEvent"/>.
    /// </summary>
    private readonly Subject<RecompressProgressEvent> _recompressProgressEvent = new();
    #endregion

    #region Create test archives.
    /// <summary>
    /// The list of the <see cref="TestDataSize"/> property.
    /// </summary>
    public ReadOnlyCollection<ListItemData<int>> TestDataSizeList { get; } = new(
        [
            new() { Display =   "1KB", Value =   1 * 1024        },
            new() { Display =  "10KB", Value =  10 * 1024        },
            new() { Display = "100KB", Value = 100 * 1024        },
            new() { Display =   "1MB", Value =   1 * 1024 * 1024 },
            new() { Display =  "10MB", Value =  10 * 1024 * 1024 },
            new() { Display = "100MB", Value = 100 * 1024 * 1024 },
        ]);

    /// <summary>
    /// The list of the <see cref="TestDataCount"/> property.
    /// </summary>
    public ReadOnlyCollection<ListItemData<int>> TestDataCountList { get; } = new(
        [
            new() { Display =   "10", Value =   10 },
            new() { Display =  "100", Value =  100 },
            new() { Display = "1000", Value = 1000 },
        ]);

    /// <summary>
    /// The size of the test archive data.
    /// </summary>
    [Reactive]
    private int _testDataSize = 1 * 1024 * 1024;

    /// <summary>
    /// The number of test archive data.
    /// </summary>
    [Reactive]
    private int _testDataCount = 10;

    /// <summary>
    /// The folder to be create archive files.
    /// </summary>
    [Reactive(SetModifier = AccessModifier.Private)]
    private string _generatedFolderPath = string.Empty;

    /// <summary>
    /// Gets the command to generate test data.
    /// </summary>
    public ReactiveCommand<Unit, Unit> GenerateCommand { get; }

    /// <summary>
    /// The subject of the <see cref="ArchiveProgressEvent"/>.
    /// </summary>
    private readonly Subject<ArchiveProgressEvent> _createArchiveProgressEvent = new();
    #endregion

    #region Progress status
    /// <summary>
    /// The progress status.
    /// </summary>
    private readonly SourceCache<ProgressStatus, string> _progressStatusCache = new(x => x.FilePath);

    /// <summary>
    /// Gets the progress status for display.
    /// </summary>
    public BindingList<ProgressStatus> ProgressStatusList { get; } = [];
    #endregion

    #region Performance
    /// <summary>
    /// The performance data for the current application.
    /// </summary>
    [Reactive]
    private ProcessPerformanceSample _processPerformanceSample = ProcessPerformanceSample.Zero;
    #endregion

    #region ViewModel
    /// <summary>
    /// Gets the command to activate the ViewModel.
    /// </summary>
    public ReactiveCommand<Unit, Unit> ActivateCommand { get; }

    /// <summary>
    /// Gets the command to deactivate the ViewModel.
    /// </summary>
    public ReactiveCommand<Unit, Unit> DeactivateCommand { get; }

    /// <summary>
    /// Determines whether any command in the ViewModel is currently running.
    /// </summary>
    [ObservableAsProperty]
    private bool _isExecuting;

    /// <summary>
    /// Describes the context in which a validation check is performed.
    /// </summary>
    private readonly ValidationContext _validationContext = new();

    /// <inheritdoc/>
    public IValidationContext ValidationContext => _validationContext;

    /// <summary>
    /// Gets the interaction to display a message.
    /// </summary>
    public Interaction<string, Unit> ShowMessageInteraction { get; } = new();

    /// <summary>
    /// Gets the interaction to confirmation.
    /// </summary>
    public Interaction<string, bool> ShowConfirmInteraction { get; } = new();
    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="MainViewModel"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="calm">The calm engine instance.</param>
    [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "<Pending>")]
    public MainViewModel(ILogger<MainViewModel> logger, ICalm calm)
    {
        _logger = logger;
        _calm = calm;

        // Sets up collections.
        SetupCollections();

        // Sets up validations;
        var isFolderPathValid = SetupFolderPathValidation();

        // Sets up activation and deactivation commands.
        ActivateCommand = ReactiveCommand.Create(HandleActivateCommand);
        DeactivateCommand = ReactiveCommand.Create(HandleDeactivateCommand);

        // Sets up commands.
        var canExecute = this.WhenAnyValue(x => x.IsExecuting).Select(x => !x);

        GenerateCommand = ReactiveCommand.CreateFromTask(
            HandleGenerateCommandAsync,
            canExecute);

        RecompressCommand = ReactiveCommand.CreateFromTask(
            HandleStartRecompressCommandAsync,
            canExecute.CombineLatest(isFolderPathValid, (c1, c2) => c1 && c2));

        Observable.Merge
            (
                GenerateCommand.ThrownExceptions,
                RecompressCommand.ThrownExceptions
            )
            .SelectMany(ex => ShowMessageInteraction.Handle(ex.Message))
            .Subscribe()
            .DisposeWith(_subscriptions);

        // Sets up properties
        _isExecutingHelper = CreateIsExecutingHelper();

        // Sets up stream.
        SetupCreateArchiveProgressEventStream();
        SetupRecompressProgressEventStream();
    }

    #region IDisposable
    /// <summary>
    /// Indicates whether the object has been disposed.
    /// </summary>
    private bool _disposed;

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }
        _logger.LogInformation("Disposing '{Class}' instance.", nameof(MainViewModel));
        _calm.Unregister(this);
        _subscriptions.Dispose();
        GenerateCommand.Dispose();
        RecompressCommand.Dispose();
        _recompressProgressEvent.Dispose();
        _createArchiveProgressEvent.Dispose();
        _validationContext.Dispose();
        _progressStatusCache.Dispose();
        _disposed = true;
    }
    #endregion

    #region Activate/Deactivate
    /// <summary>
    /// Executes the <see cref="ActivateCommand"/> command.
    /// </summary>
    [SuppressMessage("Usage", "VSTHRD002:Avoid problematic synchronous waits",
        Justification = "Because asynchronous methods cannot be used.")]
    private void HandleActivateCommand()
    {
        var task = Task.Run(async () =>
        {
            _processPerformanceSample = await _calm.Query.SendAsync(new GetSystemResourceQuery());
            await _calm.Command.SendAsync(new StartMonitoringSystemResourceCommand(TimeSpan.FromSeconds(1)));
        });
        if (!_calm.IsOnEngineThread)
        {
            task.GetAwaiter().GetResult();
        }
    }

    /// <summary>
    /// Executes the <see cref="DeactivateCommand"/> command.
    /// </summary>
    [SuppressMessage("Usage", "VSTHRD002:Avoid problematic synchronous waits",
        Justification = "Because asynchronous methods cannot be used.")]
    public void HandleDeactivateCommand()
    {
        var task = Task.Run(async () =>
        {
            await _calm.Command.SendAsync(new StopMonitoringSystemResourceCommand());
        });
        if (!_calm.IsOnEngineThread)
        {
            task.GetAwaiter().GetResult();
        }
    }
    #endregion

    /// <summary>
    /// Creates and configures the <see cref="ObservableAsPropertyHelper{T}"/>
    /// for the <see cref="IsExecuting"/> property.
    /// </summary>
    /// <returns>The configured property helper that monitors command executions.</returns>
    private ObservableAsPropertyHelper<bool> CreateIsExecutingHelper()
        => Observable.CombineLatest
            (
                this.WhenAnyObservable(vm => vm.GenerateCommand.IsExecuting),
                this.WhenAnyObservable(vm => vm.RecompressCommand.IsExecuting)
            )
            .Select(s => s.Contains(true))
            .ToProperty(this, vm => vm.IsExecuting)
            .DisposeWith(_subscriptions);

    /// <summary>
    /// Sets up validation rules for the <see cref="FolderPath"/>.
    /// </summary>
    /// <returns>An observable sequence indicating whether the folder path is valid.</returns>
    private IObservable<bool> SetupFolderPathValidation()
    {
        var rule1 = this.ValidationRule(
           vm => vm.FolderPath,
           path => !string.IsNullOrWhiteSpace(path),
           "Please enter a folder path to search for archive files.")
            .DisposeWith(_subscriptions);

        var rule2 = this.ValidationRule(
           vm => vm.FolderPath,
           path => Directory.Exists(path),
           "The specified folder path does not exist.")
            .DisposeWith(_subscriptions);

        return Observable.CombineLatest
            (
                rule1.ValidationChanged.Select(state => state.IsValid).StartWith(false),
                rule2.ValidationChanged.Select(state => state.IsValid).StartWith(false),
                (s1, s2) => s1 && s2
            );
    }

    /// <summary>
    /// Sets up collections.
    /// </summary>
    [SuppressMessage("Compiler", "CS0618:Type or member is obsolete", Justification = "<Pending>")]
    private void SetupCollections()
    {
        _progressStatusCache.Connect()
            .ObserveOn(RxSchedulers.MainThreadScheduler)
            .SortAndBind(ProgressStatusList, new NaturalStringComparer<ProgressStatus>(x => x.FilePath))
            .Subscribe()
            .DisposeWith(_subscriptions);
    }

    /// <summary>
    /// Sets up <see cref="ArchiveProgressEvent"/> event streams.
    /// </summary>
    [SuppressMessage("Design", "CA1031:Do not catch general exception types",
        Justification = "To prevent the stream from being unsubscribed.")]
    private void SetupCreateArchiveProgressEventStream()
    {
        _createArchiveProgressEvent
            .Buffer(TimeSpan.FromMilliseconds(100), 10)
            .ObserveOn(RxSchedulers.MainThreadScheduler)
            .Subscribe(events =>
            {
                _progressStatusCache.Edit(innerList =>
                {
                    foreach (var e in events)
                    {
                        try
                        {
                            var fileName = Path.GetFileName(e.FilePath);

                            var item = innerList.Lookup(fileName);
                            if (item.HasValue)
                            {
                                item.Value.NewSize = e.Size;
                                item.Value.Status = e.Status;
                            }
                            else
                            {
                                innerList.AddOrUpdate(new ProgressStatus
                                {
                                    FilePath = fileName,
                                    OriginalSize = 0,
                                    NewSize = e.Size,
                                    Status = e.Status
                                });
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to update recompress status.");
                        }
                    }
                });
            })
            .DisposeWith(_subscriptions);
    }

    /// <summary>
    /// Sets up <see cref="RecompressProgressEvent"/> event streams.
    /// </summary>
    [SuppressMessage("Design", "CA1031:Do not catch general exception types",
        Justification = "To prevent the stream from being unsubscribed.")]
    private void SetupRecompressProgressEventStream()
    {
        _recompressProgressEvent
            .Buffer(TimeSpan.FromMilliseconds(100), 10)
            .ObserveOn(RxSchedulers.MainThreadScheduler)
            .Subscribe(events =>
            {
                _progressStatusCache.Edit(innerList =>
                {
                    foreach (var e in events)
                    {
                        try
                        {
                            var relativePath = !string.IsNullOrWhiteSpace(FolderPath)
                                    ? Path.GetRelativePath(FolderPath, e.FilePath)
                                    : e.FilePath;
                            var item = innerList.Lookup(relativePath);
                            if (item.HasValue)
                            {
                                item.Value.NewSize = e.NewSize;
                                item.Value.Status = e.Status;
                            }
                            else
                            {
                                innerList.AddOrUpdate(new ProgressStatus
                                {
                                    FilePath = relativePath,
                                    OriginalSize = e.OriginalSize,
                                    NewSize = e.NewSize,
                                    Status = e.Status
                                });
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to update recompress status.");
                        }
                    }
                });
            })
            .DisposeWith(_subscriptions);
    }

    /// <summary>
    /// Executes the generate test data command.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task HandleGenerateCommandAsync()
    {
        try
        {
            _progressStatusCache.Clear();

            GeneratedFolderPath = string.Empty;
            string dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestData");
            Directory.CreateDirectory(dir);

            await Task.WhenAll(Enumerable.Range(1, TestDataCount)
                .Select(i =>
                {
                    var cmd = new CreateSampleArchiveCommand(
                        Path.Combine(dir, string.Create(CultureInfo.InvariantCulture, $"test_{i}.zip")),
                        TestDataSize);
                    return _calm.Command.SendAsync(cmd);
                })).ConfigureAwait(true);
            GeneratedFolderPath = dir;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate test data.");
            throw;
        }
    }

    /// <summary>
    /// Handles the <see cref="ArchiveProgressEvent"/> event.
    /// </summary>
    /// <param name="event">The event args.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    [CalmHandler]
    private Task HandleCreateArchiveProgressAsync(ArchiveProgressEvent @event, CancellationToken token)
    {
        _logger.LogInformation("Handle event: {Event}", @event);
        _createArchiveProgressEvent.OnNext(@event);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Executes the start recompression command.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task HandleStartRecompressCommandAsync()
    {
        try
        {
            _progressStatusCache.Clear();

            var command = new RecompressCommand(FolderPath, IsRecursive)
            {
                Delay = TimeSpan.FromMilliseconds(32)
            };
            await _calm.Command.SendAsync(command).ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to recompress.");
            throw;
        }
    }

    /// <summary>
    /// Handles the <see cref="RecompressProgressEvent"/> event.
    /// </summary>
    /// <param name="event">The event args.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    [CalmHandler]
    private Task HandleRecompressProgressAsync(RecompressProgressEvent @event, CancellationToken token)
    {
        _logger.LogInformation("Handle event: {Event}", @event);
        _recompressProgressEvent.OnNext(@event);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Handles the <see cref="UpdatedSystemResourceEvent"/> event.
    /// </summary>
    /// <param name="event">The event args.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    [CalmHandler]
    private Task HandleUpdatedSystemResourceEventAsync(UpdatedSystemResourceEvent @event, CancellationToken token)
    {
        _logger.LogInformation("Handle event: {Event}", @event);
        RxSchedulers.MainThreadScheduler.Schedule(() => ProcessPerformanceSample = @event.Sample);
        return Task.CompletedTask;
    }
}
