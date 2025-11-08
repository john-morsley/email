namespace Morsley.UK.Email.API.SystemTests;

public abstract class EmailApiTestsBase
{
    public const string EmailApiEndpoint = "/api/email";
    public const string EmailsApiEndpoint = "/api/emails";

    protected CosmosEmulatorFixture CosmosFixture = null!;
    protected WebApplicationFactory<Program> Factory = null!;
    protected HttpClient Client = null!;
    protected TestSettings TestSettings = null!;
    protected CosmosDbSettings CosmosDbSettings = null!;
    protected Faker Faker = new Faker();

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        CosmosFixture = new CosmosEmulatorFixture();
        CosmosFixture.CreateDockerContainer();
        await CosmosFixture.Start();

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.Test.json")
            .AddUserSecrets<EmailApiTests>()
            .Build();

        TestSettings = new TestSettings();
        configuration.GetSection("TestSettings").Bind(TestSettings);

        CosmosDbSettings = new CosmosDbSettings();
        configuration.GetSection("CosmosDb").Bind(CosmosDbSettings);

        await CosmosFixture.AddDatabase(CosmosDbSettings.DatabaseName);
    }

    [SetUp]
    public async Task SetUp()
    {
        await CosmosFixture.RemoveContainer(CosmosDbSettings.DatabaseName, CosmosDbSettings.ReceivedEmailsContainerName);
        await CosmosFixture.RemoveContainer(CosmosDbSettings.DatabaseName, CosmosDbSettings.SentEmailsContainerName);

        await CosmosFixture.AddContainer(CosmosDbSettings.DatabaseName, CosmosDbSettings.ReceivedEmailsContainerName, "/id");
        await CosmosFixture.AddContainer(CosmosDbSettings.DatabaseName, CosmosDbSettings.SentEmailsContainerName, "/id");

        Factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                // Clear existing sources to avoid conflicts
                config.Sources.Clear();
                
                // Add the test configuration file first
                var testConfigPath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.Test.json");
                config.AddJsonFile(testConfigPath, optional: false, reloadOnChange: false);
                
                // Add user secrets for sensitive data (SMTP/IMAP credentials)
                config.AddUserSecrets<EmailApiTests>();
                
                // Override Cosmos settings with test container values
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["CosmosDb:Endpoint"] = CosmosFixture.Endpoint,
                    ["CosmosDb:PrimaryReadKey"] = CosmosFixture.Key,
                    ["CosmosDb:SecondaryReadKey"] = CosmosFixture.Key,
                    ["CosmosDb:PrimaryReadWriteKey"] = CosmosFixture.Key,
                    ["CosmosDb:SecondaryReadWriteKey"] = CosmosFixture.Key
                });
            });

            builder.ConfigureServices(services =>
            {
                // Remove the default CosmosClient registration
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(CosmosClient));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Register a new CosmosClient which uses the the test container's HttpClient
                services.AddSingleton<CosmosClient>(sp =>
                {
                    var clientOptions = new CosmosClientOptions
                    {
                        HttpClientFactory = () => CosmosFixture.CosmosContainer!.HttpClient!,
                        ConnectionMode = ConnectionMode.Gateway,
                        RequestTimeout = TimeSpan.FromSeconds(120)
                    };

                    return new CosmosClient(
                        CosmosFixture.Endpoint,
                        CosmosFixture.Key,
                        clientOptions);
                });
            });
        });

        Client = Factory.CreateClient();
    }

    [TearDown]
    public void TearDown()
    {
        Client?.Dispose();
        Factory?.Dispose();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        if (CosmosFixture != null)
        {
            await CosmosFixture.RemoveDatabase(CosmosDbSettings.DatabaseName);
            await CosmosFixture.Stop();
            await CosmosFixture.DestroyDockerContainer();
        }
    }   
}