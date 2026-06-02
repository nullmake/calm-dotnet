using ReactiveUI;
using System.Diagnostics;

namespace Calm.Sample.Winforms.Views.Services;

/// <summary>
/// Windows-specific shell services.
/// </summary>
internal static class WindowsShellService
{
    /// <summary>
    /// The command to open the specific path with windows explorer.
    /// </summary>
    /// <remarks>
    /// <see langword="string"/> The path to be open.<br/>
    /// <see langword="bool"/> true if a process resource is started; false if no new process resource is started.<br/>
    /// </remarks>
    public static ReactiveCommand<string, bool> OpenExplorer { get; }
        = ReactiveCommand.Create<string, bool>(path =>
        {
            if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
            {
                return false;
            }
            using var process = new Process
            {
                StartInfo = new("explorer.exe", path)
                {
                    UseShellExecute = false
                }
            };
            return process.Start();
        });

    /// <summary>
    /// The command to open the specific path with windows shell.
    /// </summary>
    /// <remarks>
    /// <see langword="string"/> The path to be open.<br/>
    /// <see langword="bool"/> true if a process resource is started; false if no new process resource is started.<br/>
    /// </remarks>
    public static ReactiveCommand<string, bool> OpenShell { get; }
        = ReactiveCommand.Create<string, bool>(path =>
        {
            using var process = new Process
            {
                StartInfo = new(path)
                {
                    UseShellExecute = true
                }
            };
            return process.Start();
        });

    /// <summary>
    /// The command to open the folder selection dialog.
    /// </summary>
    /// <remarks>
    /// <see langword="ParameterConfigure&lt;FolderBrowserDialog&gt;?"/> The configure the folder selection dialog.<br/>
    /// <see langword="FolderBrowserDialog?"/> The <see cref="FolderBrowserDialog"/> instance if the user clicks OK;
    /// otherwise null.<br/>
    /// </remarks>
    public static ReactiveCommand<ParameterConfigure<FolderBrowserDialog>?, FolderBrowserDialog?> OpenFolderDialog { get; }
        = ReactiveCommand.Create<ParameterConfigure<FolderBrowserDialog>?, FolderBrowserDialog?>(configure =>
        {
            var dialog = new FolderBrowserDialog();
            configure?.Invoke(dialog);
            if (dialog.ShowDialog() is DialogResult.OK)
            {
                return dialog;
            }
            return null;
        });

    /// <summary>
    /// The command to show message box.
    /// </summary>
    /// <remarks>
    /// <see langword="RefParameterConfigure&lt;MessageBoxParams&gt;"/> The configure the message box.<br/>
    /// <see langword="DialogResult"/> One of the DialogResult values.<br/>
    /// </remarks>
    public static ReactiveCommand<RefParameterConfigure<MessageBoxParams>, DialogResult> ShowMessageBox { get; }
        = ReactiveCommand.Create<RefParameterConfigure<MessageBoxParams>, DialogResult>(configure =>
        {
            var param = new MessageBoxParams();
            configure?.Invoke(ref param);

            return MessageBox.Show(param.Owner, param.Text, param.Caption,
                param.Buttons, param.Icon, param.DefaultButton, param.Options);
        });
}
