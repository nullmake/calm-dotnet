using Calm.Sample.Winforms.Infrastructure.Application;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Calm.Sample.Winforms.ViewModels;

/// <summary>
/// The view model for the about form.
/// </summary>
[SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes",
    Justification = "Create via DI container.")]
internal sealed partial class AboutViewModel : ReactiveObject
{
    /// <summary>
    /// The source code repository.
    /// </summary>
    private const string _repository = "https://github.com/nullmake/calm-dotnet";

    /// <summary>
    /// The application name.
    /// </summary>
    [ObservableAsProperty]
    private string _appName = CurrentApplication.Name ?? "";

    /// <summary>
    /// The application version.
    /// </summary>
    [ObservableAsProperty]
    private string _appVersion = CurrentApplication.SemVer?.VersionCoreAndPrerelease ?? "";

    /// <summary>
    /// The application build information.
    /// </summary>
    [ObservableAsProperty]
    private string _appBuild = CurrentApplication.SemVer?.Build ?? "";

    /// <summary>
    /// The application copyright.
    /// </summary>
    [ObservableAsProperty]
    private string _appCopyright = CurrentApplication.Copyright ?? "";

    /// <summary>
    /// The application Third-Party Notices.
    /// </summary>
    [ObservableAsProperty]
    private string _thirdPartyNotices = "ThirdPartyNotices.html";

    /// <summary>
    /// The application project site.
    /// </summary>
    [ObservableAsProperty]
    private string _home = _repository;

    /// <summary>
    /// The application build information.
    /// </summary>
    [ObservableAsProperty]
    private string _appLicense = ReadLicense();

    /// <summary>
    /// Reads the application license.
    /// </summary>
    /// <returns>The license.</returns>
    [SuppressMessage("Design", "CA1031:Do not catch general exception types",
        Justification = "Do not throw exceptions from this method.")]
    private static string ReadLicense()
    {
        try
        {
            var path = Path.Combine(CurrentApplication.DirectoryName, "LICENSE");
            if (File.Exists(path))
            {
                return File.ReadAllText(path, new UTF8Encoding(false));
            }
        }
        catch
        {
            // do nothing
        }
        return "Please refer to " + _repository + "?tab=Apache-2.0-1-ov-file#readme";
    }
}
