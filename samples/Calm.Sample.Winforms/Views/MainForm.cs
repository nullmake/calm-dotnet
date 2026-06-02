using Calm.Sample.Winforms.Infrastructure.Application;
using Calm.Sample.Winforms.ViewModels;
using Calm.Sample.Winforms.Views.Extensions;
using Calm.Sample.Winforms.Views.Services;
using Microsoft.Extensions.Logging;
using ReactiveMarbles.ObservableEvents;
using ReactiveUI;
using ReactiveUI.Validation.Extensions;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using System.Text;

namespace Calm.Sample.Winforms.Views;

/// <summary>
/// The main form of the sample application.
/// </summary>
[SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes",
    Justification = "Create via DI container.")]
internal sealed partial class MainForm : Form, IViewFor<MainViewModel>
{
    /// <summary>
    /// The logger instance.
    /// </summary>
    private readonly ILogger _logger;

    /// <summary>
    /// Gets or sets the view model.
    /// </summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public MainViewModel? ViewModel { get; set; }

    /// <inheritdoc/>
    object? IViewFor.ViewModel
    {
        get => ViewModel;
        set => ViewModel = (MainViewModel?)value;
    }

    /// <summary>
    /// The view factory instance.
    /// </summary>
    private readonly ViewFactory _viewFactory;

    /// <summary>
    /// The log output directory.
    /// </summary>
    private readonly string _logDirectory;

    /// <summary>
    /// The <see cref="BindingSource"/> instance for <see langword="ViewModel.ProgressStatus"/>.
    /// </summary>
    private readonly BindingSource _progressStatusSource = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="MainForm"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="viewModel">The view model instance.</param>
    /// <param name="viewFactory">The view factory instance.</param>
    /// <param name="options">The application options.</param>
    public MainForm(ILogger<MainForm> logger, MainViewModel viewModel,
        ViewFactory viewFactory, Options options)
    {
        _logger = logger;
        ViewModel = viewModel;
        _viewFactory = viewFactory;
        _logDirectory = Path.GetDirectoryName(options.LogFile) ?? "";

        InitializeComponent();
        InitializeComboBoxControl();
        InitializeDataGridViewControl();
        Size = new Size(800, 600);

        this.WhenActivated(disposable =>
        {
            BindProperties(disposable);
            BindCommands(disposable);
            BindEvents(disposable);
            RegisterInteractionHandlers(disposable);
        });
    }

    /// <summary>
    /// Initialize a DataGridView control.
    /// </summary>
    private void InitializeDataGridViewControl()
    {
        dgvRecompressStatus.AllowUserToAddRows = false;
        dgvRecompressStatus.AllowUserToDeleteRows = false;
        dgvRecompressStatus.DataSource = _progressStatusSource;

        dgvRecompressStatus.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = "FilePath",
            HeaderText = "FilePath",
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
        });
        dgvRecompressStatus.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = "OriginalSize",
            HeaderText = "OriginalSize",
            AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader
        });
        dgvRecompressStatus.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = "NewSize",
            HeaderText = "NewSize",
            AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader
        });
        dgvRecompressStatus.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = "Status",
            HeaderText = "Status",
            AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader
        });
    }

    /// <summary>
    /// Initialize a ComboBox control.
    /// </summary>
    private void InitializeComboBoxControl()
    {
        comboTestDataSize.SetDataSource(ViewModel!.TestDataSizeList);
        comboTestDataCount.SetDataSource(ViewModel!.TestDataCountList);
    }

    /// <summary>
    /// Sets up the property bindings between the ViewModel and the View.
    /// </summary>
    /// <param name="disposable">The <see cref="CompositeDisposable"/> to register the property bindings to.</param>
    private void BindProperties(CompositeDisposable disposable)
    {
        this.Bind(ViewModel,
            vm => vm.FolderPath,
            v => v.txtFolderPath.Text,
            signalViewUpdate: txtFolderPath.Events().LostFocus)
            .DisposeWith(disposable);

        this.BindValidation(ViewModel,
            vm => vm.FolderPath,
            v => v.lblFolderPathError.Text)
            .DisposeWith(disposable);

        this.Bind(ViewModel,
            vm => vm.IsRecursive,
            v => v.chkRecursive.Checked)
            .DisposeWith(disposable);

        this.Bind(ViewModel,
            vm => vm.TestDataSize,
            v => v.comboTestDataSize.SelectedValue)
            .DisposeWith(disposable);

        this.Bind(ViewModel,
            vm => vm.TestDataCount,
            v => v.comboTestDataCount.SelectedValue)
            .DisposeWith(disposable);

        this.OneWayBind(ViewModel,
            vm => vm.GeneratedFolderPath,
            v => v.linkGenerateFolder.Text)
            .DisposeWith(disposable);

        this.OneWayBind(ViewModel,
            vm => vm.ProgressStatusList,
            v => v._progressStatusSource.DataSource)
            .DisposeWith(disposable);

        this.OneWayBind(ViewModel,
            vm => vm.IsExecuting,
            v => v.tpnlGenerate.Enabled,
            value => !value)
            .DisposeWith(disposable);

        this.OneWayBind(ViewModel,
            vm => vm.IsExecuting,
            v => v.tpnlFolderSelect.Enabled,
            value => !value)
            .DisposeWith(disposable);

        this.OneWayBind(ViewModel,
            vm => vm.IsExecuting,
            v => v.tpnlStart.Enabled,
            value => !value)
           .DisposeWith(disposable);

        this.OneWayBind(ViewModel,
            vm => vm.ProcessPerformanceSample,
            v => v.tsslSystemResource.Text,
            value => new StringBuilder()
                .Append("CPU: ").Append(value.ProcessorTime.ToString("F2", CultureInfo.InvariantCulture))
                    .Append('%')
                .Append(", MEM: ").Append(ProcessPerformanceSample.ToHumanReadableByteSize(value.PrivateBytes))
                .Append(", IO Read: ").Append(ProcessPerformanceSample.ToHumanReadableByteSize(value.IoReadBytesSec))
                    .Append("/sec")
                .Append(", IO Write: ").Append(ProcessPerformanceSample.ToHumanReadableByteSize(value.IoWriteBytesSec))
                    .Append("/sec")
                .ToString())
            .DisposeWith(disposable);
    }

    /// <summary>
    /// Sets up the command bindings between the ViewModel and the View.
    /// </summary>
    /// <param name="disposable">The <see cref="CompositeDisposable"/> to register the command bindings to.</param>
    private void BindCommands(CompositeDisposable disposable)
    {
        this.BindCommand(ViewModel,
            vm => vm.GenerateCommand,
            v => v.btnGenerate)
            .DisposeWith(disposable);

        this.BindCommand(ViewModel,
            vm => vm.RecompressCommand,
            v => v.btnStart)
            .DisposeWith(disposable);
    }

    /// <summary>
    /// Sets up the event bindings for the View controls.
    /// </summary>
    /// <param name="disposable">The <see cref="CompositeDisposable"/> to register the event bindings to.</param>
    private void BindEvents(CompositeDisposable disposable)
    {
        // Exits application.
        tsmiExit.Events().Click
            .Subscribe(_ => Close())
            .DisposeWith(disposable);

        //Activates the ViewMoodel.
        this.Events().Load
            .Select(_ => Unit.Default)
            .InvokeCommand(ViewModel, vm => vm.ActivateCommand)
            .DisposeWith(disposable);

        // Dectivates the ViewMoodel.
        this.Events().FormClosing
            .Select(_ => Unit.Default)
            .InvokeCommand(ViewModel, vm => vm.DeactivateCommand)
            .DisposeWith(disposable);

        // Clicking the folder path link opens it in File Explorer.
        Observable.Merge
            (
                linkGenerateFolder.Events().LinkClicked.Select(_ => linkGenerateFolder.Text),
                tsmlOpenLogDir.Events().Click.Select(_ => _logDirectory)
            )
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .InvokeCommand(WindowsShellService.OpenExplorer)
            .DisposeWith(disposable);

        // Display the folder selection dialog when the button is clicked.
        btnFolderSelect.Events().Click
            .SelectMany(_ => WindowsShellService.OpenFolderDialog.Execute(dialog =>
            {
                dialog.InitialDirectory = txtFolderPath.Text;
            }))
            .Where(dialog => dialog is not null)
            .Subscribe(dialog => ViewModel!.FolderPath = dialog!.SelectedPath)
            .DisposeWith(disposable);

        // Determines whether the dragged item is a valid directory path.
        txtFolderPath.Events().DragEnter
            .Subscribe(e =>
            {
                e.Effect = DirectoryPathContains(e.Data) ? DragDropEffects.Copy : DragDropEffects.None;
            })
            .DisposeWith(disposable);

        // Extracts and assigns the folder path from the dropped data.
        txtFolderPath.Events().DragDrop
            .Select(e => ExtractDirectoryPath(e.Data).FirstOrDefault() ?? "")
            .BindTo(this, x => x.ViewModel!.FolderPath)
            .DisposeWith(disposable);

        // Adjusts column widths when updating the data grid.
        Observable.Merge
            (
                _progressStatusSource.Events().ListChanged.Select(_ => Unit.Default),
                dgvRecompressStatus.Events().Scroll.Select(_ => Unit.Default)
            )
            .Sample(TimeSpan.FromMilliseconds(300))
            .ObserveOn(RxSchedulers.MainThreadScheduler)
            .Subscribe(_ => dgvRecompressStatus.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.DisplayedCells))
            .DisposeWith(disposable);

        // Shows row counts when updating the progress data.
        _progressStatusSource.Events().ListChanged
            .Select(_ => _progressStatusSource.Count)
            .StartWith(_progressStatusSource.Count)
            .Subscribe(count => tsslItemCount.Text = string.Create(CultureInfo.InvariantCulture, $"Items: {count}"))
            .DisposeWith(disposable);

        // Shows the about form.
        tsmlAbout.Events().Click
            .Subscribe(_ =>
            {
                using var scope = _viewFactory.CreateScope();
                _viewFactory.Create<AboutForm>().ShowDialog();
            })
            .DisposeWith(disposable);
    }

    /// <summary>
    /// Registers handlers for ViewModel interactions.
    /// </summary>
    /// <param name="disposable">The <see cref="CompositeDisposable"/> to register the handler to.</param>
    private void RegisterInteractionHandlers(CompositeDisposable disposable)
    {
        ViewModel!.ShowMessageInteraction.RegisterHandler(async interaction =>
        {
            await WindowsShellService.ShowMessageBox.Execute((ref dialog) =>
            {
                dialog.Text = interaction.Input;
                dialog.Caption = CurrentApplication.Name;
            });
            interaction.SetOutput(Unit.Default);
        }).DisposeWith(disposable);

        ViewModel!.ShowConfirmInteraction.RegisterHandler(async interaction =>
        {
            var result = await WindowsShellService.ShowMessageBox.Execute((ref dialog) =>
            {
                dialog.Text = interaction.Input;
                dialog.Caption = CurrentApplication.Name;
                dialog.Buttons = MessageBoxButtons.YesNo;
            });
            interaction.SetOutput(result is DialogResult.Yes);
        }).DisposeWith(disposable);
    }

    /// <summary>
    /// Whether the directory path is included in the transferring data.
    /// </summary>
    /// <param name="dataObject">The transferring data.</param>
    /// <returns>true if included; otherwise false.</returns>
    private static bool DirectoryPathContains(IDataObject? dataObject)
    {
        if ((dataObject?.GetDataPresent(DataFormats.FileDrop)) is not true)
        {
            return false;
        }
        var paths = dataObject.GetData(DataFormats.FileDrop) as string[] ?? [];
        return Array.Exists(paths, p => Directory.Exists(p));
    }

    /// <summary>
    /// Extract the directory path in the transferring data.
    /// </summary>
    /// <param name="dataObject">The transferring data.</param>
    /// <returns>The directory paths.</returns>
    private static IEnumerable<string> ExtractDirectoryPath(IDataObject? dataObject)
    {
        var paths = dataObject?.GetData(DataFormats.FileDrop) as string[] ?? [];
        return paths.Where(p => Directory.Exists(p));
    }
}
