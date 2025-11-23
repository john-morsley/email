namespace Morsley.UK.Email.API.HealthChecks;

/// <summary>
/// Health check for Cosmos DB connectivity and database/container availability
/// </summary>
public class CosmosDbHealthCheck : IHealthCheck
{
    private readonly CosmosClient _cosmosClient;
    private readonly CosmosDbOptions _options;
    private readonly ILogger<CosmosDbHealthCheck> _logger;

    public CosmosDbHealthCheck(
        CosmosClient cosmosClient,
        IOptions<CosmosDbOptions> options,
        ILogger<CosmosDbHealthCheck> logger)
    {
        _cosmosClient = cosmosClient ?? throw new ArgumentNullException(nameof(cosmosClient));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Read the database to verify connectivity
            var database = _cosmosClient.GetDatabase(_options.DatabaseName);
            var response = await database.ReadAsync(cancellationToken: cancellationToken);

            // Verify both containers exist
            var sentContainer = database.GetContainer(_options.SentEmailsContainerName);
            await sentContainer.ReadContainerAsync(cancellationToken: cancellationToken);

            var receivedContainer = database.GetContainer(_options.ReceivedEmailsContainerName);
            await receivedContainer.ReadContainerAsync(cancellationToken: cancellationToken);

            var data = new Dictionary<string, object>
            {
                { "endpoint", _options.Endpoint },
                { "database", _options.DatabaseName },
                { "sentEmailsContainer", _options.SentEmailsContainerName },
                { "receivedEmailsContainer", _options.ReceivedEmailsContainerName }
            };

            return HealthCheckResult.Healthy("CosmosDB is accessible and containers are available", data);
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning(ex, "CosmosDB database or container not found");
            return HealthCheckResult.Unhealthy("CosmosDB database or container not found", ex);
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            _logger.LogError(ex, "CosmosDB authentication failed");
            return HealthCheckResult.Unhealthy("CosmosDB authentication failed", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CosmosDB health check failed");
            return HealthCheckResult.Unhealthy("CosmosDB is not accessible", ex);
        }
    }
}
