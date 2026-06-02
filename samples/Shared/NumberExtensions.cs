using System.Globalization;

namespace SharedLibrary;

public static class NumberExtensions
{
    private static readonly string[] _humanReadableByteUnits = ["B", "KB", "MB", "GB", "TB", "PB", "EB"];

    public static string ToHumanReadableByteSize(this uint bytes, int decimalPlaces = 2)
        => ((double)bytes).ToHumanReadableByteSize(decimalPlaces);

    public static string ToHumanReadableByteSize(this ulong bytes, int decimalPlaces = 2)
        => ((double)bytes).ToHumanReadableByteSize(decimalPlaces);

    public static string ToHumanReadableByteSize(this int bytes, int decimalPlaces = 2)
        => ((double)bytes).ToHumanReadableByteSize(decimalPlaces);

    public static string ToHumanReadableByteSize(this long bytes, int decimalPlaces = 2)
        => ((double)bytes).ToHumanReadableByteSize(decimalPlaces);

    public static string ToHumanReadableByteSize(this float bytes, int decimalPlaces = 2)
        => ((double)bytes).ToHumanReadableByteSize(decimalPlaces);

    public static string ToHumanReadableByteSize(this decimal bytes, int decimalPlaces = 2)
        => ((double)bytes).ToHumanReadableByteSize(decimalPlaces);

    public static string ToHumanReadableByteSize(this double bytes, int decimalPlaces = 2)
    {
        if (bytes < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(bytes), "Byte count cannot be negative.");
        }
        if (decimalPlaces < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(decimalPlaces), "Decimal places cannot be negative.");
        }

        if (bytes < 1.0)
        {
            string zeroFormat = "F" + decimalPlaces.ToString(CultureInfo.InvariantCulture);
            return bytes.ToString(zeroFormat, CultureInfo.InvariantCulture) + " B";
        }

        int unitIndex = 0;
        double value = bytes;
        while (value >= 1024.0 && unitIndex < _humanReadableByteUnits.Length - 1)
        {
            value /= 1024.0;
            unitIndex++;
        }
        value = Math.Round(value, decimalPlaces);
        if (value >= 1024.0 && unitIndex < _humanReadableByteUnits.Length - 1)
        {
            value /= 1024.0;
            unitIndex++;
        }

        var format = "F" + decimalPlaces.ToString(CultureInfo.InvariantCulture);
        return $"{value.ToString(format, CultureInfo.InvariantCulture)} {_humanReadableByteUnits[unitIndex]}";
    }
}
