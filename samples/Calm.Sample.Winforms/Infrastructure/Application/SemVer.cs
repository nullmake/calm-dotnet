using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Calm.Sample.Winforms.Infrastructure.Application;

/// <summary>
/// The Semantic Versioning 2.0.0.
/// </summary>
internal readonly partial struct SemVer : IEquatable<SemVer>
{
    [GeneratedRegex(@"^(0|[1-9]\d*)\.(0|[1-9]\d*)\.(0|[1-9]\d*)"
        + @"(?:-((?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*)(?:\.(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*))*))?"
        + @"(?:\+([0-9a-zA-Z-]+(?:\.[0-9a-zA-Z-]+)*))?$")]
    private static partial Regex SemVerRegex { get; }

    /// <summary>
    /// Indicates whether the SemVer is valid.
    /// </summary>
    public bool IsValid => Major >= 0;

    /// <summary>
    /// The major version.
    /// </summary>
    public int Major { get; } = -1;

    /// <summary>
    /// The minor version.
    /// </summary>
    public int Minor { get; } = -1;

    /// <summary>
    /// The patch version.
    /// </summary>
    public int Patch { get; } = -1;

    /// <summary>
    /// The prerelease version.
    /// </summary>
    public string Prerelease { get; } = "";

    /// <summary>
    /// The build metadata.
    /// </summary>
    public string Build { get; } = "";

    /// <summary>
    /// The valid semver.
    /// </summary>
    public string ValidSemVer { get; } = "";

    /// <summary>
    /// The version number.
    /// </summary>
    public string VersionCore => IsValid
        ? string.Create(CultureInfo.InvariantCulture, $"{Major}.{Minor}.{Patch}")
        : "";

    /// <summary>
    /// The version number and prerelease version.
    /// </summary>
    public string VersionCoreAndPrerelease
    {
        get
        {
            if (!IsValid)
            {
                return "";
            }
            if (string.IsNullOrEmpty(Prerelease))
            {
                return VersionCore;
            }
            return VersionCore + '-' + Prerelease;
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SemVer"/> class.
    /// </summary>
    /// <param name="semver">The Semantic Versioning 2.0.0 string.</param>
    public SemVer(string? semver)
    {
        if (string.IsNullOrWhiteSpace(semver))
        {
            return;
        }
        var matches = SemVerRegex.Matches(semver);
        if (matches.Count is 0)
        {
            return;
        }
        var groups = matches[0].Groups;
        if (groups.Count < 4)
        {
            return;
        }
        if (!int.TryParse(groups[1].Value, CultureInfo.InvariantCulture, out var major)
            || !int.TryParse(groups[2].Value, CultureInfo.InvariantCulture, out var minor)
            || !int.TryParse(groups[3].Value, CultureInfo.InvariantCulture, out var patch))
        {
            return;
        }
        ValidSemVer = groups[0].Value;
        Major = major;
        Minor = minor;
        Patch = patch;

        if (groups.Count < 5)
        {
            return;
        }
        Prerelease = groups[4].Value;

        if (groups.Count < 6)
        {
            return;
        }
        Build = groups[5].Value;
    }

    /// <inheritdoc/>
    public override string ToString()
#if true
        => ValidSemVer;
#else // for debug
    {
        if (!IsValid)
        {
            return "";
        }
        var sb = new System.Text.StringBuilder()
            .Append(Major).Append('.').Append(Minor).Append('.').Append(Patch);
        if (!string.IsNullOrEmpty(Prerelease))
        {
            sb.Append('-').Append(Prerelease);
        }
        if (!string.IsNullOrEmpty(Build))
        {
            sb.Append('+').Append(Build);
        }
        return sb.ToString();
    }
#endif

    /// <inheritdoc/>
    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj is SemVer ver && Equals(ver);
    }

    /// <inheritdoc/>
    public bool Equals(SemVer other)
        => Major == other.Major
            && Minor == other.Minor
            && Patch == other.Patch
            && string.Equals(Prerelease, other.Prerelease, StringComparison.Ordinal)
            && string.Equals(Build, other.Build, StringComparison.Ordinal);

    /// <inheritdoc/>
    public override int GetHashCode()
        => HashCode.Combine(Major, Minor, Patch, Prerelease, Build);

    /// <summary>
    /// Compares two <see cref="SemVer"/> instances for equality.
    /// </summary>
    /// <param name="left">The first SemVer instance to compare.</param>
    /// <param name="right">The second SemVer instance to compare.</param>
    /// <returns>true if the specified SemVer instances are equal; otherwise, false.</returns>
    public static bool operator ==(SemVer left, SemVer right)
        => left.Equals(right);

    /// <summary>
    /// Compares two <see cref="SemVer"/> instances for inequality.
    /// </summary>
    /// <param name="left">The first SemVer instance to compare.</param>
    /// <param name="right">The second SemVer instance to compare.</param>
    /// <returns>true if the specified SemVer instances are not equal; otherwise, false.</returns>
    public static bool operator !=(SemVer left, SemVer right)
        => !(left == right);
}
