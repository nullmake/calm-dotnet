using System.Diagnostics.CodeAnalysis;

namespace Calm.Core.Messaging.Handlers;

/// <summary>
/// Calm handler
/// </summary>
/// <typeparam name="TCallback">The type of callback delegate.</typeparam>
internal abstract class Handler<TCallback> : IHandler
    where TCallback : Delegate
{
    /// <summary>
    /// The callback handler.
    /// </summary>
    protected TCallback Callback { get; }

    /// <inheritdoc/>
    Delegate IHandler.Callback => Callback;

    /// <inheritdoc/>
    public string Name => Callback.Method.Name;

    /// <summary>
    /// Initializes a new instance of the <see cref="MessageHandler{TMessage}"/> class.
    /// </summary>
    /// <param name="callback">The callback handler. Must be a method marked
    /// with <see cref="CalmHandlerAttribute"/>.</param>
    [SuppressMessage("Globalization", "CA1307:Specify StringComparison for clarity",
        Justification = "Because net472 and netstandard2.0 do not support StringComparison")]
    [SuppressMessage("Usage", "MA0001:StringComparison is missing",
        Justification = "Because net472 and netstandard2.0 do not support StringComparison")]
    protected Handler(TCallback callback)
    {
        var methodInfo = callback.Method;
        if (!Attribute.IsDefined(methodInfo, typeof(CalmHandlerAttribute)))
        {
            var attributeName = nameof(CalmHandlerAttribute).Replace("Attribute", "");
            throw new CalmSchemaException(
                $"Handler method '{methodInfo.Name}' must be marked with [{attributeName}] attribute. "
                + "Lambda expressions cannot be used as handlers because they cannot have attributes.");
        }
        if (methodInfo.IsGenericMethod)
        {
            throw new CalmSchemaException(
                $"Handler method '{methodInfo.Name}' must not be generic method.");
        }
        Callback = callback;
    }

    /// <summary>
    /// Determines whether the specified delegate matches the callback handler.
    /// Comparison is based on the method and target instance, allowing method groups to be matched.
    /// </summary>
    /// <param name="method">The delegate to compare.</param>
    /// <returns>true if the method and target are the same; otherwise, false.</returns>
    public bool Matches(Delegate method)
        => ReferenceEquals(Callback.Method, method.Method)
        && ReferenceEquals(Callback.Target, method.Target);
}
