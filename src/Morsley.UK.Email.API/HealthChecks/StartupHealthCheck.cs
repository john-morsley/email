using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Morsley.UK.Email.API.HealthChecks;

/// <summary>
/// Simple health check to verify the application has started successfully
/// </summary>
public class StartupHealthCheck : IHealthCheck
{
    private volatile bool _isStartupComplete;

    public void MarkStartupComplete()
    {
        _isStartupComplete = true;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        if (_isStartupComplete)
        {
            return Task.FromResult(HealthCheckResult.Healthy("Application startup completed"));
        }

        return Task.FromResult(HealthCheckResult.Unhealthy("Application is still starting up"));
    }
}
