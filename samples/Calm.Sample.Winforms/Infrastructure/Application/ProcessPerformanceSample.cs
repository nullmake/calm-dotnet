using System.Globalization;
using System.Text;

namespace Calm.Sample.Winforms.Infrastructure.Application;

/// <summary>
/// The performance data sample for process.
/// </summary>
/// <param name="TimeStamp">Date and time the data was collected, in UTC.</param>
/// <param name="ProcessorTime">The cpu usage, in percent.</param>
/// <param name="PrivateBytes">The private bytes, in bytes.</param>
/// <param name="VirtualBytes">The virtual bytes, in bytes.</param>
/// <param name="PrivateWorkingSet">The private working set, in bytes.</param>
/// <param name="IoReadBytesSec">The reading bytes amount, in bytes per seconds.</param>
/// <param name="IoWriteBytesSec">The writing bytes amount, in bytes per seconds.</param>
/// <param name="ThreadCount">The number of threads.</param>
/// <param name="ThreadPoolThreadCount">The number of the thread pool threads.</param>
/// <param name="ThreadPoolQueueLength">The number of the thread pool queue length.</param>
/// <param name="HandleCount">The number of the handle count.</param>
/// <param name="GcHeapSize">The GC heap size, in bytes.</param>
/// <param name="GcTotalCommittedBytes">The GC total committed bytes.</param>
/// <param name="GcTotalAllocatedBytes">The GC total number of bytes.</param>
internal sealed record ProcessPerformanceSample(
    DateTimeOffset TimeStamp,
    float ProcessorTime,
    float PrivateBytes,
    float VirtualBytes,
    float PrivateWorkingSet,
    float IoReadBytesSec,
    float IoWriteBytesSec,
    int ThreadCount,
    int ThreadPoolThreadCount,
    long ThreadPoolQueueLength,
    int HandleCount,
    long GcHeapSize,
    long GcTotalCommittedBytes,
    long GcTotalAllocatedBytes)
{
    /// <summary>
    /// Gets the zero initialized instance.
    /// </summary>
    public static ProcessPerformanceSample Zero => new(TimeProvider.System.GetUtcNow(),
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);

    /// <inheritdoc/>
    public override string ToString()
        => new StringBuilder()
            .Append("{ ProcessorTime=").Append(ProcessorTime.ToString("F2", CultureInfo.InvariantCulture)).Append('%')
            .Append(", PrivateBytes=").Append(ToHumanReadableByteSize(PrivateBytes))
            .Append(", VirtualBytes=").Append(ToHumanReadableByteSize(VirtualBytes))
            .Append(", PrivateWorkingSet=").Append(ToHumanReadableByteSize(PrivateWorkingSet))
            .Append(", IoReadBytesSec=").Append(ToHumanReadableByteSize(IoReadBytesSec)).Append("/sec")
            .Append(", IoWriteBytesSec=").Append(ToHumanReadableByteSize(IoWriteBytesSec)).Append("/sec")
            .Append(", ThreadCount=").Append(ThreadCount)
            .Append(", ThreadPoolThreadCount=").Append(ThreadPoolThreadCount)
            .Append(", ThreadPoolQueueLength=").Append(ThreadPoolQueueLength)
            .Append(", HandleCount=").Append(HandleCount)
            .Append(", GcHeapSize=").Append(ToHumanReadableByteSize(GcHeapSize))
            .Append(", GcTotalCommittedBytes=").Append(ToHumanReadableByteSize(GcTotalCommittedBytes))
            .Append(", GcTotalAllocatedBytes=").Append(ToHumanReadableByteSize(GcTotalAllocatedBytes))
            .Append(" }")
            .ToString();

    /// <summary>
    /// The human-readable byte size units.
    /// </summary>
    private static readonly string[] _humanReadableByteUnits = ["B", "KB", "MB", "GB", "TB", "PB", "EB"];

    /// <summary>
    /// Converts a byte count into a human-readable string with the appropriate unit.
    /// </summary>
    /// <param name="bytes">The number of bytes to format.</param>
    /// <param name="decimalPlaces">The number of decimal places to display.</param>
    /// <returns>A formatted string representing the byte size (e.g., "1.50 MB").</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when bytes or decimalPlaces is negative.</exception>
    public static string ToHumanReadableByteSize(double bytes, int decimalPlaces = 2)
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
