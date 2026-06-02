using Calm.Sample.Winforms.Infrastructure.Interop;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;

namespace Calm.Sample.Winforms.Infrastructure.Application;

/// <summary>
/// Information about the currently executing application.
/// </summary>
internal sealed class CurrentApplication
{
    /// <summary>
    /// Gets the unique instance of <see cref="CurrentApplication"/>.
    /// </summary>
    public static CurrentApplication Default { get; } = new CurrentApplication();

    /// <summary>
    /// Prevents a default instance of the <see cref="CurrentApplication"/> class
    /// from being created from the outside.
    /// </summary>
    private CurrentApplication()
    {
        // In the case of a self-contained application,
        // since Process.GetCurrentProcess().MainModule cannot be retrieved on the first run,
        // It calls here as well.
        _ = Location;
    }

    #region Assembly
    /// <summary>
    /// The currently executing assembly.
    /// </summary>
    public static Assembly? Assembly
    {
        get;
        set
        {
            field = value;
            Location = null!;
            DirectoryName = null!;
            FileName = null!;
            Name = null;
            Company = null;
            Copyright = null;
            Description = null;
            FileVersion = null;
            Product = null;
            Title = null;
            Trademark = null;
            Version = null;
            Versions = null;
            InformationalVersion = null;
            Metadata = null!;
            SemVer = null;
        }
    }
    = Assembly.GetEntryAssembly();
    #endregion

    #region Information
#pragma warning disable IL3000 // Avoid accessing Assembly file path when publishing as a single file
    /// <summary>
    /// The path of the application.
    /// </summary>
    public static string Location
    {
        get
        {
            if (field is not null)
            {
                return field;
            }
            if (Assembly?.Location is not null)
            {
                return field = Assembly.Location;
            }
            var module = Process.GetCurrentProcess().MainModule;
            if (module is null)
            {
                return field = "";
            }
            if (AppContext.BaseDirectory is not null)
            {
                return field = Path.Combine(AppContext.BaseDirectory, module.ModuleName);
            }
            return field = module.FileName;
        }
        private set;
    }
#pragma warning restore IL3000

    /// <summary>
    /// The application path without the file extension.
    /// </summary>
    public static string LocationWithoutExtension
        => !string.IsNullOrEmpty(DirectoryName) && !string.IsNullOrEmpty(FileNameWithoutExtension)
            ? Path.Combine(DirectoryName, FileNameWithoutExtension)
            : "";

    /// <summary>
    /// The directory of the application.
    /// </summary>
    public static string DirectoryName
    {
        get => field ??= !string.IsNullOrEmpty(Location)
            ? Path.GetDirectoryName(Location) ?? AppContext.BaseDirectory ?? ""
            : AppContext.BaseDirectory;
        private set;
    }

    /// <summary>
    /// The file name and extension of the application.
    /// </summary>
    public static string FileName
    {
        get => field ??= !string.IsNullOrEmpty(Location)
            ? Path.GetFileName(Location)
            : Path.GetFileName(Process.GetCurrentProcess().MainModule?.FileName) ?? "";
        private set;
    }

    /// <summary>
    /// The file name of the application without the extension.
    /// </summary>
    public static string FileNameWithoutExtension => Path.GetFileNameWithoutExtension(FileName);

    /// <summary>
    /// The file extension of the application.
    /// </summary>
    public static string Extension => Path.GetExtension(FileName);

    /// <summary>
    /// The assembly name.
    /// </summary>
    public static string? Name
    {
        get => field ??= Assembly?.GetName().Name;
        private set;
    }

    /// <summary>
    /// The company name.
    /// </summary>
    public static string? Company
    {
        get => field ??= GetAttribute<AssemblyCompanyAttribute>()?.Company;
        private set;
    }

    /// <summary>
    /// The copyright notices.
    /// </summary>
    public static string? Copyright
    {
        get => field ??= GetAttribute<AssemblyCopyrightAttribute>()?.Copyright;
        private set;
    }
    /// <summary>
    /// The description of the file.
    /// </summary>
    public static string? Description
    {
        get => field ??= GetAttribute<AssemblyDescriptionAttribute>()?.Description;
        private set;
    }

    /// <summary>
    /// The file version.
    /// </summary>
    public static string? FileVersion
    {
        get => field ??= GetAttribute<AssemblyFileVersionAttribute>()?.Version;
        private set;
    }

    /// <summary>
    /// The product name.
    /// </summary>
    public static string? Product
    {
        get => field ??= GetAttribute<AssemblyProductAttribute>()?.Product;
        private set;
    }

    /// <summary>
    /// The title of the file.
    /// </summary>
    public static string? Title
    {
        get => field ??= GetAttribute<AssemblyTitleAttribute>()?.Title;
        private set;
    }

    /// <summary>
    /// The trademark.
    /// </summary>
    public static string? Trademark
    {
        get => field ??= GetAttribute<AssemblyTrademarkAttribute>()?.Trademark;
        private set;
    }

    /// <summary>
    /// The assembly version.
    /// </summary>
    public static string? Version
    {
        get => field ??= Versions?.ToString();
        private set;
    }

    /// <summary>
    /// The assembly version as a <see cref="System.Version"/> object.
    /// </summary>
    public static Version? Versions
    {
        get => field ??= Assembly?.GetName().Version;
        private set;
    }

    /// <summary>
    /// Additional version information for the assembly manifest.
    /// </summary>
    public static string? InformationalVersion
    {
        get => field ??= GetAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        private set;
    }

    /// <summary>
    /// The product version.
    /// </summary>
    public static string? ProductVersion => InformationalVersion ?? Version;

    /// <summary>
    /// The Semantic Versioning 2.0.0
    /// </summary>
    public static SemVer? SemVer
    {
        get => field ??= new SemVer(InformationalVersion);
        private set;
    }

    /// <summary>
    /// A collection of assembly metadata.
    /// </summary>
    public static IReadOnlyDictionary<string, string?> Metadata
    {
        get => field ??= Assembly is null
            ? []
            : GetAttributes<AssemblyMetadataAttribute>().ToDictionary(m => m.Key, m => m.Value, StringComparer.Ordinal);
        private set;
    }

    /// <summary>
    /// Gets the specified attribute information of the assembly.
    /// </summary>
    /// <typeparam name="T">The type of the attribute.</typeparam>
    /// <returns>The attribute information, or <see langword="null"/> if not found.</returns>
    private static T? GetAttribute<T>()
        where T : Attribute
        => Assembly is null ? null : (T?)Attribute.GetCustomAttribute(Assembly, typeof(T));

    /// <summary>
    /// Gets the specified attribute information of the assembly.
    /// </summary>
    /// <typeparam name="T">The type of the attribute.</typeparam>
    /// <returns>A collection of the attribute information.</returns>
    private static IEnumerable<T> GetAttributes<T>()
        where T : Attribute
        => Assembly is null ? [] : Attribute.GetCustomAttributes(Assembly, typeof(T)).Cast<T>();

    /// <inheritdoc/>
    public override string ToString()
    {
        var sb = new StringBuilder()
            .Append('{')
            .Append("\n  Location=").Append(Location)
            .Append("\n  LocationWithoutExtension=").Append(LocationWithoutExtension)
            .Append("\n  DirectoryName=").Append(DirectoryName)
            .Append("\n  FileName=").Append(FileName)
            .Append("\n  FileNameWithoutExtension=").Append(FileNameWithoutExtension)
            .Append("\n  Extension=").Append(Extension)
            .Append("\n  Version=").Append(Version)
            .Append("\n  FileVersion=").Append(FileVersion)
            .Append("\n  InformationalVersion=").Append(InformationalVersion)
            .Append("\n  Name=").Append(Name)
            .Append("\n  Product=").Append(Product)
            .Append("\n  Title=").Append(Title)
            .Append("\n  Description=").Append(Description)
            .Append("\n  Copyright=").Append(Copyright)
            .Append("\n  Company=").Append(Company)
            .Append("\n  Trademark=").Append(Trademark);
        foreach (var metadata in Metadata)
        {
            sb.Append("\n  ").Append(metadata.Key).Append('=').Append(metadata.Value);
        }
        sb.Append("\n}");
        return sb.ToString();
    }
    #endregion

    #region Prevention of multiple startups.
    /// <summary>
    /// The application mutex.
    /// </summary>
    [SuppressMessage("Minor Code Smell", "S1450:Private fields only used as local variables in methods should become local variables",
        Justification = "False Positive")]
    private static Mutex? _mutex;

    /// <summary>
    /// Determines whether the application is already running and attempts to activate a single instance.
    /// </summary>
    /// <param name="mutexName">A unique mutex name. If <see langword="null"/>,
    /// the assembly name is automatically used.</param>
    /// <returns>true if activation was successful (this is the only running instance); otherwise, false.</returns>
    /// <exception cref="InvalidOperationException">
    /// The assembly name could not be retrieved to set the mutex name.
    /// </exception>
    public static bool TryActivateSingleInstance(string? mutexName = null)
    {
        var name = mutexName;
        if (string.IsNullOrEmpty(name))
        {
            name = !string.IsNullOrEmpty(Name)
                ? $@"Global\{Name}"
                : throw new InvalidOperationException(
                    "The assembly name could not be retrieved to set the mutex name.");
        }

        _mutex = new Mutex(true, name, out bool createdNew);
        if (!createdNew)
        {
            _mutex.Dispose();
            _mutex = null;
        }
        return createdNew;
    }

    /// <summary>
    /// Finds the already running process of the same application and brings its main window to the foreground.
    /// </summary>
    public static void ActivateExistingInstance()
    {
        var currentProcess = Process.GetCurrentProcess();
        foreach (var process in Process.GetProcessesByName(currentProcess.ProcessName))
        {
            // Skip the current process itself
            if (process.Id == currentProcess.Id)
            {
                continue;
            }

            var hWnd = process.MainWindowHandle;
            if (hWnd != IntPtr.Zero)
            {
                // If the window is minimized, restore it
                if (NativeMethods.IsIconic(hWnd))
                {
                    NativeMethods.ShowWindow(hWnd, NativeMethods.CmdShow.SW_RESTORE);
                }

                // Bring the window to the foreground
                NativeMethods.SetForegroundWindow(hWnd);
                break;
            }
        }
    }
    #endregion

    #region Shutdown and Restart
    /// <summary>
    /// This occurs just before a Restart or Shutdown is called.
    /// </summary>
    public static event EventHandler Exit = delegate { };

    /// <summary>
    /// Shuts down the application.
    /// </summary>
    /// <param name="exitCode">The exit code to return to the operating system.</param>
    public static void Shutdown(int exitCode)
    {
        Exit(null, EventArgs.Empty);
        Environment.Exit(exitCode);
    }

    /// <summary>
    /// Shuts down the application and immediately restarts a new instance.
    /// </summary>
    /// <param name="exitCode">The exit code to return to the operating system.</param>
    /// <param name="arguments">The command-line arguments to pass to the new instance.</param>
    public static void Restart(int exitCode, string arguments = "")
    {
        var startInfo = new ProcessStartInfo
        {
#if NET6_0_OR_GREATER
            FileName = Environment.ProcessPath,
#else
            FileName = Process.GetCurrentProcess().MainModule?.FileName
                ?? Path.ChangeExtension(Location, ".exe"),
#endif
            Arguments = arguments,
            UseShellExecute = true
        };

        Exit(null, EventArgs.Empty);
        ReleaseSingleInstance();
        Process.Start(startInfo);
        Environment.Exit(exitCode);
    }

    /// <summary>
    /// Releases the resources allocated for single instance prevention.
    /// </summary>
    private static void ReleaseSingleInstance()
    {
        if (_mutex is not null)
        {
            _mutex.ReleaseMutex();
            _mutex.Dispose();
            _mutex = null;
        }
    }
    #endregion

    #region System resource
    /// <summary>
    /// Gets the performance data.
    /// </summary>
    /// <returns>The current application resources.</returns>
    public static ProcessPerformance Performance
    {
        get
        {
            using var currentProcess = Process.GetCurrentProcess();
            return new(currentProcess);
        }
    }
    #endregion
}
