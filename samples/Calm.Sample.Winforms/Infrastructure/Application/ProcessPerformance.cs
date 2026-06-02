using System.Diagnostics;
using System.Globalization;

namespace Calm.Sample.Winforms.Infrastructure.Application;

/// <summary>
/// The process performance.
/// </summary>
internal sealed class ProcessPerformance
{
    /// <summary>
    /// The number of processors available to the current process.
    /// </summary>
    private static readonly int _processorCount = Environment.ProcessorCount;

    /// <summary>
    /// Determines whether the OS is Windows 11 or later.
    /// </summary>
    private static readonly bool _isWin11OrLater
        = OperatingSystem.IsWindows() && OperatingSystem.IsWindowsVersionAtLeast(10, 0, 22000);

    /// <summary>
    /// Initializes a new instance of the <see cref="ProcessPerformance"/> class.
    /// </summary>
    /// <param name="process">The process.</param>
    public ProcessPerformance(Process process)
    {
        string categoryName;
        string processName;
        if (_isWin11OrLater)
        {
            categoryName = "Process V2";
            processName = process.ProcessName + ":" + Environment.ProcessId.ToString(CultureInfo.InvariantCulture);
        }
        else
        {
            categoryName = "Process";
            processName = process.ProcessName;
        }
        ProcessName = processName;

        _processorTime = new(() => new(categoryName, "% Processor Time", processName));
        _privateBytes = new(() => new(categoryName, "Private Bytes", processName));
        _virtualBytes = new(() => new(categoryName, "Virtual Bytes", processName));
        _privateWorkingSet = new(() => new(categoryName, "Working Set - Private", processName));
        _ioReadBytesSec = new(() => new(categoryName, "IO Read Bytes/sec", processName));
        _ioWriteBytesSec = new(() => new(categoryName, "IO Write Bytes/sec", processName));
        _threadCount = new(() => new(categoryName, "Thread Count", processName));
        _handleCount = new(() => new(categoryName, "Handle Count", processName));
    }

    /// <summary>
    /// The name of the process.
    /// </summary>
    public string ProcessName { get; }

    #region CPU
    /// <summary>
    /// The performance counter of the "\Process\% Processor Time".
    /// </summary>
    private readonly Lazy<PerformanceCounter> _processorTime;

    /// <summary>
    /// Gets the percentage of time that all threads in the relevant process
    /// have used the processor to execute instructions.
    /// </summary>
    /// <returns>The cpu usage, in percent.</returns>
    public float GetProcessorTime() => _processorTime.Value.NextValue() / _processorCount;
    #endregion

    #region Memory
    /// <summary>
    /// The performance counter of the "\Process\Private Bytes".
    /// </summary>
    private readonly Lazy<PerformanceCounter> _privateBytes;

    /// <summary>
    /// Gets the current size in bytes of memory allocated by the process
    /// in question and not shared with other processes.
    /// </summary>
    /// <returns>The private bytes, in bytes.</returns>
    public float GetPrivateBytes() => _privateBytes.Value.NextValue();

    /// <summary>
    /// The performance counter of the "\Process\Virtual Bytes".
    /// </summary>
    private readonly Lazy<PerformanceCounter> _virtualBytes;

    /// <summary>
    /// Gets the current size of the virtual address space used by the process, in bytes.
    /// </summary>
    /// <returns>The virtual bytes, in bytes.</returns>
    public float GetVirtualBytes() => _virtualBytes.Value.NextValue();

    /// <summary>
    /// The performance counter of the "\Process\Working Set - Private".
    /// </summary>
    private readonly Lazy<PerformanceCounter> _privateWorkingSet;

    /// <summary>
    /// Gets the size in bytes of the working set, which is used exclusively by this process
    /// and is not shared with, nor can be shared with, other processes.
    /// </summary>
    /// <returns>The private working set, in bytes.</returns>
    public float GetPrivateWorkingSet() => _privateWorkingSet.Value.NextValue();
    #endregion

    #region IO
    /// <summary>
    /// The performance counter of the "\Process\IO Read Bytes/sec".
    /// </summary>
    private readonly Lazy<PerformanceCounter> _ioReadBytesSec;

    /// <summary>
    /// Gets the rate at which a process is reading bytes from I/O operations.
    /// </summary>
    /// <returns>The reading bytes amount, in bytes per seconds.</returns>
    public float GetIoReadBytesSec() => _ioReadBytesSec.Value.NextValue();

    /// <summary>
    /// The performance counter of the "\Process\IO Write Bytes/sec".
    /// </summary>
    private readonly Lazy<PerformanceCounter> _ioWriteBytesSec;

    /// <summary>
    /// Gets the rate at which a process is writing bytes for I/O operations.
    /// </summary>
    /// <returns>The writing bytes amount, in bytes per seconds.</returns>
    public float GetsIoWriteBytesSec() => _ioWriteBytesSec.Value.NextValue();
    #endregion

    #region Thread
    /// <summary>
    /// The performance counter of the "Thread Count".
    /// </summary>
    private readonly Lazy<PerformanceCounter> _threadCount;

    /// <summary>
    /// Gets the number of threads currently active in the relevant process.
    /// </summary>
    /// <returns>The number of threads.</returns>
    public int GetThreadCount() => (int)_threadCount.Value.NextValue();

    /// <summary>
    /// Gets the number of thread pool threads.
    /// </summary>
    /// <returns>The number of the thread pool threads.</returns>
    public static int GetThreadPoolThreadCount() => ThreadPool.ThreadCount;

    /// <summary>
    /// Gets the number of work items that are currently queued to be processed.
    /// </summary>
    /// <returns>The number of the thread pool queue length.</returns>
    public static long GetThreadPoolQueueLength() => ThreadPool.PendingWorkItemCount;
    #endregion

    #region Handle
    /// <summary>
    /// The performance counter of the "Handle Count".
    /// </summary>
    private readonly Lazy<PerformanceCounter> _handleCount;

    /// <summary>
    /// Gets the total number of handles currently open by the process in question.
    /// </summary>
    /// <returns>The number of the handle count.</returns>
    public int GetHandleCount() => (int)_handleCount.Value.NextValue();
    #endregion

    #region GC
    /// <summary>
    /// Gets the GC heap size, in bytes, excluding fragmentation.
    /// </summary>
    /// <returns>The GC heap size, in bytes.</returns>
    public static long GetGcHeapSize() => GC.GetTotalMemory(forceFullCollection: false);

    /// <summary>
    /// Gets the GC total committed bytes of the managed heap.
    /// </summary>
    /// <returns>The GC total committed bytes.</returns>
    public static long GetGcTotalCommittedBytes() => GC.GetGCMemoryInfo().TotalCommittedBytes;

    /// <summary>
    /// Gets the GC total number of bytes allocated over the lifetime of the process.
    /// </summary>
    /// <returns>The GC total number of bytes.</returns>
    public static long GetGcTotalAllocatedBytes() => GC.GetTotalAllocatedBytes();
    #endregion

    /// <summary>
    /// Returns a <see cref="ProcessPerformanceSample"/>.
    /// </summary>
    /// <returns>a <see cref="ProcessPerformanceSample"/></returns>
    public ProcessPerformanceSample Sample()
        => new(
            TimeProvider.System.GetUtcNow(),
            GetProcessorTime(),
            GetPrivateBytes(),
            GetVirtualBytes(),
            GetPrivateWorkingSet(),
            GetIoReadBytesSec(),
            GetsIoWriteBytesSec(),
            GetThreadCount(),
            GetThreadPoolThreadCount(),
            GetThreadPoolQueueLength(),
            GetHandleCount(),
            GetGcHeapSize(),
            GetGcTotalCommittedBytes(),
            GetGcTotalAllocatedBytes());

    /// <inheritdoc/>
    public override string ToString() => Sample().ToString();

    /// <summary>
    /// Converts a byte count into a human-readable string with the appropriate unit.
    /// </summary>
    /// <param name="bytes">The number of bytes to format.</param>
    /// <param name="decimalPlaces">The number of decimal places to display.</param>
    /// <returns>A formatted string representing the byte size (e.g., "1.50 MB").</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when bytes or decimalPlaces is negative.</exception>
    public static string ToHumanReadableByteSize(double bytes, int decimalPlaces = 2)
        => ProcessPerformanceSample.ToHumanReadableByteSize(bytes, decimalPlaces);
}
