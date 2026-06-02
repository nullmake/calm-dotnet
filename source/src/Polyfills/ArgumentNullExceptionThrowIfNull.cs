#if !NET6_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace System;

/// <summary>
/// The static partial class of <see cref="ArgumentNullException"/>.
/// </summary>
#pragma warning disable MA0182 // Avoid unused internal types
internal static class ArgumentNullExceptionThrowIfNull
#pragma warning restore MA0182 // Avoid unused internal types
{
    extension(ArgumentNullException)
    {
        /// <summary>
        /// Throws an <see cref="ArgumentNullException"/> if <paramref name="argument"/> is null.
        /// </summary>
        /// <param name="argument">The reference type argument to validate as non-null.</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="argument"/> corresponds.</param>
        /// <exception cref="ArgumentNullException"><paramref name="argument"/> is null.</exception>
        public static void ThrowIfNull(
            [NotNull] object? argument,
            [CallerArgumentExpression(nameof(argument))] string? paramName = null)
        {
            _ = argument ?? throw new ArgumentNullException(paramName);
        }
    }
}
#endif
