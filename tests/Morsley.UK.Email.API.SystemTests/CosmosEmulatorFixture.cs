namespace Morsley.UK.Email.API.SystemTests;

public class CosmosEmulatorFixture
{
    public CosmosDbContainer? CosmosContainer { get; private set; }

    public void CreateDockerContainer()
    {
        CosmosContainer = new CosmosDbBuilder()
            .WithImage("mcr.microsoft.com/cosmosdb/linux/azure-cosmos-emulator:latest")
            .Build();
    }

    public async Task Start()
    {
        if (CosmosContainer is not null)
        {
            await CosmosContainer.StartAsync();
        }
    }

    public async Task Stop()
    {
        if (CosmosContainer is not null)
        {
            await CosmosContainer.StopAsync();
        }
    }

    public async Task DestroyDockerContainer()
    {
        if (CosmosContainer is not null)
        {
            await CosmosContainer.DisposeAsync();
        }
    }

    public string Endpoint 
    { 
        get
        {
            var connectionString = CosmosContainer!.GetConnectionString();

            var parts = connectionString.Split(';', StringSplitOptions.RemoveEmptyEntries);
            var endpoint = parts.FirstOrDefault(p => p.StartsWith("AccountEndpoint="))?.Replace("AccountEndpoint=", "")?.TrimEnd('/'); ;

            if (endpoint is null) throw new InvalidOperationException("AccountEndpoint not found in connection string");

            return endpoint;
        } 
    }

    public string Key
    {
        get
        {
            var connectionString = CosmosContainer!.GetConnectionString();

            var parts = connectionString.Split(';', StringSplitOptions.RemoveEmptyEntries);            
            var key = parts.FirstOrDefault(p => p.StartsWith("AccountKey="))?.Replace("AccountKey=", "");

            if (key is null) throw new InvalidOperationException("AccountKey not found in connection string");

            return key;
        }
    }

    public async Task AddDatabase(string databaseId)
    {
        if (string.IsNullOrEmpty(databaseId)) throw new ArgumentNullException(nameof(databaseId));

        if (CosmosContainer is null) throw new InvalidOperationException(nameof(CosmosContainer));
        if (CosmosContainer.HttpClient is null) throw new InvalidOperationException(nameof(CosmosContainer.HttpClient));
        
        var clientOptions = new CosmosClientOptions
        {
            HttpClientFactory = () => CosmosContainer.HttpClient,
            ConnectionMode = ConnectionMode.Gateway,
            RequestTimeout = TimeSpan.FromSeconds(120)
        };

        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(120));
        var cosmosClient = new CosmosClient(Endpoint, Key, clientOptions);

        try
        {
            await cosmosClient.CreateDatabaseIfNotExistsAsync(databaseId, cancellationToken: cts.Token);
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task RemoveDatabase(string databaseId)
    {
        if (string.IsNullOrEmpty(databaseId)) throw new ArgumentNullException(nameof(databaseId));

        if (CosmosContainer is null) throw new InvalidOperationException(nameof(CosmosContainer));
        if (CosmosContainer.HttpClient is null) throw new InvalidOperationException(nameof(CosmosContainer.HttpClient));
        
        var clientOptions = new CosmosClientOptions
        {
            HttpClientFactory = () => CosmosContainer.HttpClient,
            ConnectionMode = ConnectionMode.Gateway,
            RequestTimeout = TimeSpan.FromSeconds(120)
        };

        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(120));
        var cosmosClient = new CosmosClient(Endpoint, Key, clientOptions);

        try
        {
            var database = cosmosClient.GetDatabase(databaseId);
            await database.DeleteAsync(cancellationToken: cts.Token);
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            // Database doesn't exist - this is fine for a cleanup operation
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task AddContainer(string databaseId, string containerId, string partitionKeyPath)
    {
        if (string.IsNullOrEmpty(databaseId)) throw new ArgumentNullException(nameof(databaseId));
        if (string.IsNullOrEmpty(containerId)) throw new ArgumentNullException(nameof(containerId));
        if (string.IsNullOrEmpty(partitionKeyPath)) throw new ArgumentNullException(nameof(partitionKeyPath));

        if (CosmosContainer is null) throw new InvalidOperationException(nameof(CosmosContainer));
        if (CosmosContainer.HttpClient is null) throw new InvalidOperationException(nameof(CosmosContainer.HttpClient));
        
        var clientOptions = new CosmosClientOptions
        {
            HttpClientFactory = () => CosmosContainer.HttpClient,
            ConnectionMode = ConnectionMode.Gateway,
            RequestTimeout = TimeSpan.FromSeconds(120)
        };

        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(120));
        var cosmosClient = new CosmosClient(Endpoint, Key, clientOptions);

        try
        {
            var database = cosmosClient.GetDatabase(databaseId);
            await database.CreateContainerIfNotExistsAsync(containerId, partitionKeyPath, cancellationToken: cts.Token);
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task RemoveContainer(string databaseId, string containerId)
    {
        if (string.IsNullOrEmpty(databaseId)) throw new ArgumentNullException(nameof(databaseId));
        if (string.IsNullOrEmpty(containerId)) throw new ArgumentNullException(nameof(containerId));

        if (CosmosContainer is null) throw new InvalidOperationException(nameof(CosmosContainer));
        if (CosmosContainer.HttpClient is null) throw new InvalidOperationException(nameof(CosmosContainer.HttpClient));
        
        var clientOptions = new CosmosClientOptions
        {
            HttpClientFactory = () => CosmosContainer.HttpClient,
            ConnectionMode = ConnectionMode.Gateway,
            RequestTimeout = TimeSpan.FromSeconds(120)
        };

        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(120));
        var cosmosClient = new CosmosClient(Endpoint, Key, clientOptions);

        try
        {
            var container = cosmosClient.GetContainer(databaseId, containerId);
            await container.DeleteContainerAsync(cancellationToken: cts.Token);
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            // Container doesn't exist - this is fine for a cleanup operation
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<T?> GetItemAsync<T>(string databaseId, string containerId, string partitionKey, string entityId)
    {
        if (string.IsNullOrEmpty(databaseId)) throw new ArgumentNullException(nameof(databaseId));
        if (string.IsNullOrEmpty(containerId)) throw new ArgumentNullException(nameof(containerId));
        if (string.IsNullOrEmpty(partitionKey)) throw new ArgumentNullException(nameof(partitionKey));
        if (string.IsNullOrEmpty(entityId)) throw new ArgumentNullException(nameof(entityId));
        

        if (CosmosContainer is null) throw new InvalidOperationException(nameof(CosmosContainer));
        if (CosmosContainer.HttpClient is null) throw new InvalidOperationException(nameof(CosmosContainer.HttpClient));
        
        var clientOptions = new CosmosClientOptions
        {
            HttpClientFactory = () => CosmosContainer.HttpClient,
            ConnectionMode = ConnectionMode.Gateway,
            RequestTimeout = TimeSpan.FromSeconds(120)
        };

        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(120));
        var cosmosClient = new CosmosClient(Endpoint, Key, clientOptions);

        try
        {
            var container = cosmosClient.GetContainer(databaseId, containerId);
            var response = await container.ReadItemAsync<T>(entityId, new PartitionKey(partitionKey), cancellationToken: cts.Token);
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return default;
        }
        catch (Exception)
        {
            throw;
        }
    }

    //public async Task<Thing> AddThing(string databaseId, string containerId, string id, string name, string data)
    //{
    //    if (string.IsNullOrEmpty(databaseId)) throw new ArgumentNullException(nameof(databaseId));
    //    if (string.IsNullOrEmpty(containerId)) throw new ArgumentNullException(nameof(containerId));
    //    if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));
    //    if (string.IsNullOrEmpty(data)) throw new ArgumentNullException(nameof(data));

    //    if (CosmosContainer is null) throw new InvalidOperationException(nameof(CosmosContainer));
    //    if (CosmosContainer.HttpClient is null) throw new InvalidOperationException(nameof(CosmosContainer.HttpClient));
        
    //    var clientOptions = new CosmosClientOptions
    //    {
    //        HttpClientFactory = () => CosmosContainer.HttpClient,
    //        ConnectionMode = ConnectionMode.Gateway,
    //        RequestTimeout = TimeSpan.FromSeconds(120)
    //    };

    //    var cts = new CancellationTokenSource(TimeSpan.FromSeconds(120));
    //    var cosmosClient = new CosmosClient(Endpoint, Key, clientOptions);

    //    try
    //    {
    //        var container = cosmosClient.GetContainer(databaseId, containerId);
            
    //        var thing = new
    //        {
    //            id = id,
    //            Name = name,
    //            Data = data,
    //            Created = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fffff")
    //        };

    //        var response = await container.CreateItemAsync(thing, new PartitionKey(thing.id), cancellationToken: cts.Token);
            
    //        var added = new Thing
    //        {
    //            Id = response.Resource.id,
    //            Name = response.Resource.Name,
    //            Data = response.Resource.Data,
    //            Created = response.Resource.Created
    //        };
            
    //        return added;
    //    }
    //    catch (Exception)
    //    {
    //        throw;
    //    }
    //}
}