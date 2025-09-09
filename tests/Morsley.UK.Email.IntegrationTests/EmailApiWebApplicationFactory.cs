namespace Morsley.UK.Email.IntegrationTests;

public class EmailApiWebApplicationFactory : WebApplicationFactory<Program>
{
    //private DockerClient? _dockerClient;

    public async Task InitializeAsync()
    {
        // Wait for Cosmos DB to be ready (assumes it's already running via docker-compose)
        await WaitForCosmosDbReady();

        // Initialize Cosmos DB database and containers after the emulator is ready
        await SetupCosmosDbForTesting();
    }

    private async Task SetupCosmosDbForTesting()
    {
        try
        {
            Console.WriteLine("Starting Cosmos DB database initialization...");
            
            // Create a temporary service provider to initialize the database
            var services = new ServiceCollection();
            var configuration = new ConfigurationBuilder()
                .AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.Test.json"))
                .Build();
            
            services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Information));
            services.AddEmailPersistence(configuration);
            
            var serviceProvider = services.BuildServiceProvider();
            await serviceProvider.InitializeCosmosDbAsync(throwOnError: true);
            
            Console.WriteLine("Cosmos DB database initialization completed successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to initialize Cosmos DB database: {ex}");
            throw;
        }
    }

    public Task CleanupAsync()
    {
        // No cleanup needed since we're not managing containers
        return Task.CompletedTask;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Clear existing configuration sources to avoid conflicts
            config.Sources.Clear();
            
            // Add the test configuration from the test project directory
            var testConfigPath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.Test.json");
            config.AddJsonFile(testConfigPath, optional: false, reloadOnChange: true);
            
            // Add user secrets for sensitive configuration
            config.AddUserSecrets<EmailApiWebApplicationFactory>();
        });

        builder.ConfigureServices(services =>
        {
            // Replace the email sender with a mock for testing
            // Remove the real email sender registration
            //var emailSenderDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IEmailSender));
            //if (emailSenderDescriptor != null)
            //{
            //    services.Remove(emailSenderDescriptor);
            //}
            
            // Add a mock email sender that doesn't actually send emails
            //services.AddScoped<IEmailSender, MockEmailSender>();
            
            // Note: Cosmos DB initialization will happen after host creation to avoid blocking startup
        });

        builder.UseEnvironment("Test");
    }

    private async Task WaitForCosmosDbReady()
    {
        Console.WriteLine("Waiting for Cosmos DB to be ready (assuming it's running via docker-compose)...");
        var maxAttempts = 120; // 10 minutes
        var attempt = 0;

        // Check if Cosmos DB endpoint is ready
        using var httpClient = new HttpClient(new HttpClientHandler()
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
        });
        httpClient.Timeout = TimeSpan.FromSeconds(30);

        bool endpointResponding = false;
        
        while (attempt < maxAttempts)
        {
            try
            {
                // Try the main Cosmos DB endpoint
                var response = await httpClient.GetAsync("https://localhost:8081/");
                
                Console.WriteLine($"Cosmos DB endpoint response: {response.StatusCode}");
                
                // Check if we're getting responses (even 503 means it's starting up)
                if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
                {
                    if (!endpointResponding)
                    {
                        Console.WriteLine("Cosmos DB emulator is starting up (503 Service Unavailable)...");
                        endpointResponding = true;
                    }
                    // Continue waiting - 503 means it's initializing
                }
                else if (response.IsSuccessStatusCode || 
                         response.StatusCode == System.Net.HttpStatusCode.Unauthorized ||
                         response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    Console.WriteLine($"Cosmos DB endpoint is ready! Status: {response.StatusCode}");
                    
                    // Give it additional time to fully initialize all database services
                    Console.WriteLine("Waiting additional 10 seconds for Cosmos DB services to fully initialize...");
                    await Task.Delay(10000);
                    Console.WriteLine("Cosmos DB is fully ready for database operations");
                    return;
                }
                else
                {
                    Console.WriteLine($"Cosmos DB not ready yet. Status: {response.StatusCode}");
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Connection attempt {attempt + 1}: {ex.Message}");
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine($"Request timeout on attempt {attempt + 1}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error on attempt {attempt + 1}: {ex.Message}");
            }

            attempt++;
            await Task.Delay(endpointResponding ? 10000 : 5000); // Wait longer once we know it's starting
            Console.WriteLine($"Waiting for Cosmos DB... attempt {attempt}/{maxAttempts}");
        }

        throw new TimeoutException("Cosmos DB emulator did not start within the expected time");
    }
}