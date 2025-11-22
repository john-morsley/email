using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Morsley.UK.Email.API.Controllers;

/// <summary>
/// Health check controller for monitoring application status
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly HealthCheckService _healthCheckService;
    private readonly ILogger<HealthController> _logger;

    public HealthController(
        HealthCheckService healthCheckService,
        ILogger<HealthController> logger)
    {
        _healthCheckService = healthCheckService ?? throw new ArgumentNullException(nameof(healthCheckService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get overall health status of the application
    /// </summary>
    /// <returns>Health status report</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Get()
    {
        var report = await _healthCheckService.CheckHealthAsync();

        _logger.LogInformation("Health check performed: Status={Status}", report.Status);

        return report.Status == HealthStatus.Healthy 
            ? Ok(new
            {
                status = report.Status.ToString(),
                checks = report.Entries.Select(e => new
                {
                    name = e.Key,
                    status = e.Value.Status.ToString(),
                    description = e.Value.Description,
                    duration = e.Value.Duration.TotalMilliseconds,
                    data = e.Value.Data
                }),
                totalDuration = report.TotalDuration.TotalMilliseconds
            })
            : StatusCode(StatusCodes.Status503ServiceUnavailable, new
            {
                status = report.Status.ToString(),
                checks = report.Entries.Select(e => new
                {
                    name = e.Key,
                    status = e.Value.Status.ToString(),
                    description = e.Value.Description,
                    duration = e.Value.Duration.TotalMilliseconds,
                    error = e.Value.Exception?.Message,
                    data = e.Value.Data
                }),
                totalDuration = report.TotalDuration.TotalMilliseconds
            });
    }

    /// <summary>
    /// Liveness probe - checks if the application is running
    /// </summary>
    /// <returns>200 OK if alive</returns>
    [HttpGet("live")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Live()
    {
        var report = await _healthCheckService.CheckHealthAsync(check => check.Name == "startup");

        return report.Status == HealthStatus.Healthy 
            ? Ok(new { status = "Alive" })
            : StatusCode(StatusCodes.Status503ServiceUnavailable, new { status = "Starting" });
    }

    /// <summary>
    /// Readiness probe - checks if the application is ready to accept traffic
    /// </summary>
    /// <returns>200 OK if ready</returns>
    [HttpGet("ready")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Ready()
    {
        var report = await _healthCheckService.CheckHealthAsync(check => check.Tags.Contains("ready"));

        return report.Status == HealthStatus.Healthy 
            ? Ok(new
            {
                status = "Ready",
                checks = report.Entries.Select(e => new
                {
                    name = e.Key,
                    status = e.Value.Status.ToString()
                })
            })
            : StatusCode(StatusCodes.Status503ServiceUnavailable, new
            {
                status = "NotReady",
                checks = report.Entries.Select(e => new
                {
                    name = e.Key,
                    status = e.Value.Status.ToString(),
                    error = e.Value.Exception?.Message
                })
            });
    }
}
