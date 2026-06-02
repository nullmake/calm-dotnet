using Calm.Sample.Winforms.Infrastructure.Application;
using Microsoft.Extensions.Logging;

namespace Calm.Sample.Winforms;

/// <summary>
/// The application options.
/// </summary>
internal sealed record Options
{
    /// <summary>
    /// The logging level.
    /// </summary>
    public LogLevel LogLevel { get; } = LogLevel.Information;

    /// <summary>
    /// The log file.
    /// </summary>
    public string LogFile { get; } = Path.Combine(
        CurrentApplication.DirectoryName, "Logs", CurrentApplication.Name + ".log");

    /// <summary>
    /// Initializes a new instance of the <see cref="Options"/> class.
    /// </summary>
    /// <param name="arguments">The commandline arguments.</param>
    public Options(string[] arguments)
    {
        if (arguments.Length is 0)
        {
            return;
        }

        string[] args = [.. arguments, null!];
        var i = 0;
        while (i < args.Length)
        {
            switch (args[i])
            {
                case "--loglevel":
                    LogLevel = Enum.Parse<LogLevel>(args[++i], ignoreCase: true);
                    break;
                case "--logfile":
                    LogFile = args[++i];
                    break;
                default:
                    if (!string.IsNullOrEmpty(args[i]))
                    {
                        throw new InvalidOperationException($"Unknown commandline parameter: {args[i]}");
                    }
                    break;
            }
            ++i;
        }
    }
}
