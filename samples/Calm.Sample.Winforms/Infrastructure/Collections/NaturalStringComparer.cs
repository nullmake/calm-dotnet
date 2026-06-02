using Calm.Sample.Winforms.Infrastructure.Interop;

namespace Calm.Sample.Winforms.Infrastructure.Collections;

#pragma warning disable RCS1060 // Declare each type in separate file
#pragma warning disable CA1812 // Avoid uninstantiated internal classes
#pragma warning disable MA0182 // Avoid unused internal types

/// <summary>
/// Provides a string comparer that uses natural sort order.
/// This mimics the behavior of Windows File Explorer.
/// </summary>
internal sealed class NaturalStringComparer : IComparer<string>
{
    /// <summary>
    /// Compares two strings and returns a value indicating whether one is less than,
    /// equal to, or greater than the other.
    /// </summary>
    /// <param name="x">The first string to compare.</param>
    /// <param name="y">The second string to compare.</param>
    /// <returns>
    /// A signed integer that indicates the relative values of x and y,
    /// handling numeric sequences as logical numbers.
    /// </returns>
    public int Compare(string? x, string? y)
    {
        if (ReferenceEquals(x, y))
        {
            return 0;
        }
        if (x is null)
        {
            return -1;
        }
        if (y is null)
        {
            return 1;
        }
        return NativeMethods.StrCmpLogicalW(x, y);
    }
}

/// <summary>
/// Provides a string comparer that uses natural sort order.
/// This mimics the behavior of Windows File Explorer.
/// </summary>
/// <typeparam name="T">The type of objects to compare.</typeparam>
/// <param name="selector">A delegate that returns a comparison target.</param>
internal sealed class NaturalStringComparer<T>(Func<T, string> selector) : IComparer<T>
{
    /// <summary>
    /// A delegate that returns a comparison target.
    /// </summary>
    private readonly Func<T, string> _selector = selector ?? throw new ArgumentNullException(nameof(selector));

    /// <summary>
    /// Compares two strings and returns a value indicating whether one is less than,
    /// equal to, or greater than the other.
    /// </summary>
    /// <param name="x">The first string to compare.</param>
    /// <param name="y">The second string to compare.</param>
    /// <returns>
    /// A signed integer that indicates the relative values of x and y,
    /// handling numeric sequences as logical numbers.
    /// </returns>
    public int Compare(T? x, T? y)
    {
        if (ReferenceEquals(x, y))
        {
            return 0;
        }
        if (x is null)
        {
            return -1;
        }
        if (y is null)
        {
            return 1;
        }
        return NativeMethods.StrCmpLogicalW(_selector(x), _selector(y));
    }
}
