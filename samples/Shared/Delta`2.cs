using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace SharedLibrary;

[SuppressMessage("Performance", "CA1815:Override equals and operator equals on value types")]
public struct Delta<TValue, TDiff>(TValue initialValue)
    where TValue : struct
{
    private static readonly Lazy<Func<TValue, TValue, TDiff>> _subtract = new(() =>
    {
        var left = Expression.Parameter(typeof(TValue), "left");
        var right = Expression.Parameter(typeof(TValue), "right");

        try
        {
            var body = Expression.Subtract(left, right);
            return Expression.Lambda<Func<TValue, TValue, TDiff>>(body, left, right).Compile();
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException($"{typeof(TValue).Name} The type does not support subtraction.");
        }
    }, true);

    public TValue Previous { get; private set; } = initialValue;
    public TValue Current { get; private set; } = initialValue;
    public readonly TDiff Value => _subtract.Value(Current, Previous);

    public void SetValue(TValue value)
    {
        Previous = Current;
        Current = value;
    }
}
