namespace Morsley.UK.Email.Persistence;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEmailPersistence(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName = "CosmosDb")
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        // Configure CosmosDB options
        services
            .AddOptions<CosmosDbOptions>()
            .Bind(configuration.GetSection(sectionName))
            .Validate(options => !string.IsNullOrWhiteSpace(options.Endpoint), "CosmosDb:Endpoint is required")
            .Validate(options => !string.IsNullOrWhiteSpace(options.PrimaryKey), "CosmosDb:PrimaryKey is required")
            .Validate(options => !string.IsNullOrWhiteSpace(options.DatabaseName), "CosmosDb:DatabaseName is required")
            .Validate(options => !string.IsNullOrWhiteSpace(options.ContainerName), "CosmosDb:ContainerName is required")
            .ValidateOnStart();

        // Register CosmosClient
        services.AddSingleton<CosmosClient>(serviceProvider =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<CosmosDbOptions>>().Value;
            var cosmosClientOptions = new CosmosClientOptions
            {
                SerializerOptions = new CosmosSerializationOptions
                {
                    PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
                }
            };
            return new CosmosClient(options.Endpoint, options.PrimaryKey, cosmosClientOptions);
        });

        // Register persistence service
        services.AddScoped<IEmailPersistenceService, CosmosDbEmailPersistenceService>();

        return services;
    }

    public static IServiceCollection AddEmailPersistence(
        this IServiceCollection services,
        Action<CosmosDbOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        // Configure CosmosDB options
        services
            .AddOptions<CosmosDbOptions>()
            .Configure(configure)
            .Validate(options => !string.IsNullOrWhiteSpace(options.Endpoint), "CosmosDb:Endpoint is required")
            .Validate(options => !string.IsNullOrWhiteSpace(options.PrimaryKey), "CosmosDb:PrimaryKey is required")
            .Validate(options => !string.IsNullOrWhiteSpace(options.DatabaseName), "CosmosDb:DatabaseName is required")
            .Validate(options => !string.IsNullOrWhiteSpace(options.ContainerName), "CosmosDb:ContainerName is required")
            .ValidateOnStart();

        // Register CosmosClient
        services.AddSingleton<CosmosClient>(serviceProvider =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<CosmosDbOptions>>().Value;
            var cosmosClientOptions = new CosmosClientOptions
            {
                SerializerOptions = new CosmosSerializationOptions
                {
                    PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
                }
            };
            return new CosmosClient(options.Endpoint, options.PrimaryKey, cosmosClientOptions);
        });

        // Register persistence service
        services.AddScoped<IEmailPersistenceService, CosmosDbEmailPersistenceService>();

        return services;
    }

    /// <summary>
    /// Initializes the CosmosDB database and container if they don't exist.
    /// Call this method during application startup.
    /// </summary>
    public static async Task InitializeCosmosDbAsync(this IServiceProvider serviceProvider, bool throwOnError = false)
    {
        try
        {
            var cosmosClient = serviceProvider.GetRequiredService<CosmosClient>();
            var options = serviceProvider.GetRequiredService<IOptions<CosmosDbOptions>>().Value;

            // Create database if it doesn't exist
            var databaseResponse = await cosmosClient.CreateDatabaseIfNotExistsAsync(
                options.DatabaseName,
                throughput: 400); // Set appropriate throughput for your needs

            // Create container if it doesn't exist
            var containerProperties = new ContainerProperties(options.ContainerName, "/partitionKey");
            await databaseResponse.Database.CreateContainerIfNotExistsAsync(containerProperties);
        }
        catch (Exception ex)
        {
            var logger = serviceProvider.GetService<ILogger<CosmosClient>>();
            logger?.LogWarning(ex, "Failed to initialize CosmosDB. This may be expected in development environments without CosmosDB emulator running.");
            
            if (throwOnError)
                throw;
        }
    }
}
