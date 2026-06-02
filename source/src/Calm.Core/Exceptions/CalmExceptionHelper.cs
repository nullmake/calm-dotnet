namespace Calm.Core.Exceptions;

/// <summary>
/// Provides helper methods for handling exceptions within CALM.
/// </summary>
internal static class CalmExceptionHelper
{
    /// <summary>
    /// Unwraps AggregateException to get the original exception if only one exists,
    /// otherwise returns a flattened AggregateException to preserve all errors.
    /// </summary>
    /// <param name="ex">The exception to unwrap.</param>
    /// <returns>
    /// The original exception if it's not an AggregateException;
    /// The single inner exception if it's an AggregateException with one child;
    /// Otherwise, a flattened AggregateException.
    /// </returns>
    public static Exception Unwrap(Exception ex)
    {
        if (ex is AggregateException aggregateException)
        {
            var flattened = aggregateException.Flatten();
            if (flattened is not null)
            {
                // If there's only one exception, return it directly for better usability.
                // If there are multiple, return the flattened AggregateException to preserve all details.
                return flattened.InnerExceptions.Count is 1
                    ? flattened.InnerExceptions[0]
                    : flattened;
            }
        }
        return ex;
    }
}
