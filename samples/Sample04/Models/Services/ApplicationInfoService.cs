using Calm.Core;
using Microsoft.Extensions.Logging;
using Sample04.Models.Bus.Queries;

namespace Sample04.Models.Services;

internal class ApplicationInfoService(ICalm calm, ILogger<ApplicationInfoService> logger) : IDisposable
{
    private readonly ILogger _logger = logger;
    private readonly ICalm _calm = calm;

    #region IDisposable
    private bool _disposed;

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }
        _logger.LogInformation("Disposing.");

        // Unregister the CALM handlers for this instance.
        _calm.Unregister(this);

        _disposed = true;
        _logger.LogInformation("Disposed.");
    }
    #endregion

    [CalmHandler]
    private Task<ApplicationInfoQueryResult> HandleApplicationInfoQueryAsync(
        ApplicationInfoQuery query, CancellationToken token)
    {
        _logger.LogInformation("Handle query: {Query}", query);

        var name = typeof(ApplicationInfoService).Assembly.GetName();
        return Task.FromResult(new ApplicationInfoQueryResult
        {
            Name = name.Name ?? "",
            Version = name.Version?.ToString() ?? ""
        });
    }
}
