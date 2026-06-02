using Calm.Sample.Winforms.Infrastructure.Application;
using Calm.Sample.Winforms.ViewModels;
using Calm.Sample.Winforms.Views.Services;
using ReactiveMarbles.ObservableEvents;
using ReactiveUI;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;

namespace Calm.Sample.Winforms.Views;

/// <summary>
/// The About form.
/// </summary>
[SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes",
    Justification = "Create via DI container.")]
internal sealed partial class AboutForm : Form, IViewFor<AboutViewModel>
{

    /// <summary>
    /// Gets or sets the view model.
    /// </summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public AboutViewModel? ViewModel { get; set; }

    /// <inheritdoc/>
    object? IViewFor.ViewModel
    {
        get => ViewModel;
        set => ViewModel = (AboutViewModel?)value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AboutForm"/> class.
    /// </summary>
    /// <param name="viewModel">The view model instance.</param>
    public AboutForm(AboutViewModel viewModel)
    {
        ViewModel = viewModel;

        InitializeComponent();

        this.WhenActivated(disposable =>
        {
            this.OneWayBind(ViewModel,
                vm => vm.AppName,
                v => v.lblAppName.Text)
                .DisposeWith(disposable);

            this.OneWayBind(ViewModel,
                vm => vm.AppVersion,
                v => v.lblVersion.Text)
                .DisposeWith(disposable);

            this.OneWayBind(ViewModel,
                vm => vm.AppCopyright,
                v => v.lblCopyright.Text)
                .DisposeWith(disposable);

            this.OneWayBind(ViewModel,
                vm => vm.AppBuild,
                v => v.lblBuild.Text)
                .DisposeWith(disposable);

            this.OneWayBind(ViewModel,
                vm => vm.ThirdPartyNotices,
                v => v.linkThirdPartyNotices.Text)
                .DisposeWith(disposable);

            this.OneWayBind(ViewModel,
                vm => vm.Home,
                v => v.linkHome.Text)
                .DisposeWith(disposable);

            this.OneWayBind(ViewModel,
                vm => vm.AppLicense,
                v => v.txtLicense.Text)
                .DisposeWith(disposable);

            linkThirdPartyNotices.Events().Click
                .Select(_ => Path.Combine(CurrentApplication.DirectoryName, linkThirdPartyNotices.Text))
                .Where(p => File.Exists(p))
                .InvokeCommand(WindowsShellService.OpenShell)
                .DisposeWith(disposable);

            linkHome.Events().Click
                .Select(_ => linkHome.Text)
                .InvokeCommand(WindowsShellService.OpenShell)
                .DisposeWith(disposable);

            btnOK.Events().Click
                .Subscribe(_ => Close())
                .DisposeWith(disposable);

            this.Events().Paint
                .Subscribe(e =>
                {
                    using var blackPen = new Pen(Color.Black, 1);
                    e.Graphics.DrawRectangle(blackPen, 0, 0, ClientSize.Width - 1, ClientSize.Height - 1);
                })
                .DisposeWith(disposable);
        });
    }
}
