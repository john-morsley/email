namespace Morsley.UK.Email.API.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IConfiguration _configuration;
    private readonly CosmosClient _cosmosClient;
    private readonly CosmosDbOptions _cosmosOptions;

    public HomeController(
        ILogger<HomeController> logger,
        IConfiguration configuration,
        CosmosClient cosmosClient,
        IOptions<CosmosDbOptions> cosmosDbOptions)
    {
        _logger = logger;
        _configuration = configuration;
        _cosmosClient = cosmosClient;
        _cosmosOptions = cosmosDbOptions.Value;

        ViewBag.KeyVaultResults = null;
        ViewBag.CosmosDbResults = null;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.IsKeyVaultHealthy = IsKeyVaultHealthy(out var keyVaultDetails);
        ViewBag.KeyVaultDetails = keyVaultDetails;

        var cosmosHealth = await IsCosmosDbHealthy();
        ViewBag.IsCosmosDbHealthy = cosmosHealth.IsHealthy;
        ViewBag.CosmosDbDetails = cosmosHealth.Details;

        return View();
    }

    public IActionResult Swagger()
    {
        return View();
    }

    private bool IsKeyVaultHealthy(out IList<string> details)
    {
        details = [];

        var morsleyUkCosmosDbEndpoint = _configuration["CosmosDb:Endpoint"];
        details.Add($"MorsleyUk--CosmosDb--Endpoint: {morsleyUkCosmosDbEndpoint.ToMaskedSecret()}");

        var morsleyUkCosmosDbPrimaryReadKey = _configuration["CosmosDb:PrimaryReadKey"];
        details.Add($"MorsleyUk--CosmosDb--PrimaryReadKey: {morsleyUkCosmosDbPrimaryReadKey.ToMaskedSecret()}");

        var morsleyUkCosmosDbSecondaryReadKey = _configuration["CosmosDb:SecondaryReadKey"];
        details.Add($"MorsleyUk--CosmosDb--SecondaryReadKey: {morsleyUkCosmosDbSecondaryReadKey.ToMaskedSecret()}");

        var morsleyUkCosmosDbPrimaryReadWriteKey = _configuration["CosmosDb:PrimaryReadWriteKey"];
        details.Add($"MorsleyUk--CosmosDb--PrimaryReadWriteKey: {morsleyUkCosmosDbPrimaryReadWriteKey.ToMaskedSecret()}");

        var morsleyUkCosmosDbSecondaryReadWriteKey = _configuration["CosmosDb:SecondaryReadWriteKey"];
        details.Add($"MorsleyUk--CosmosDb--SecondaryReadWriteKey: {morsleyUkCosmosDbSecondaryReadWriteKey.ToMaskedSecret()}");

        var morsleyUkImapSettingsPassword = _configuration["ImapSettings:Password"];
        details.Add($"MorsleyUk--ImapSettings--Password: {morsleyUkImapSettingsPassword.ToMaskedSecret()}");

        var morsleyUkImapSettingsUsername = _configuration["ImapSettings:Username"];
        details.Add($"MorsleyUk--ImapSettings--Username: {morsleyUkImapSettingsUsername.ToMaskedSecret()}");

        var morsleyUkSmtpSettingsPassword = _configuration["SmtpSettings:Password"];
        details.Add($"MorsleyUk--SmtpSettings--Password: {morsleyUkSmtpSettingsPassword.ToMaskedSecret()}");

        var morsleyUkSmtpSettingsUsername = _configuration["SmtpSettings:Username"];
        details.Add($"MorsleyUk--SmtpSettings--Username: {morsleyUkSmtpSettingsUsername.ToMaskedSecret()}");

        var morsleyUkSmtpSettingsFromAddress = _configuration["SmtpSettings:FromAddress"];
        details.Add($"MorsleyUk--SmtpSettings--FromAddress: {morsleyUkSmtpSettingsFromAddress.ToMaskedSecret()}");

        return true;
    }

    private async Task<(bool IsHealthy, IList<string> Details)> IsCosmosDbHealthy()
    {
        var results = new List<string>();
        var isHealthy = true;

        try
        {            
            if (_cosmosOptions.UseLocalEmulator)
            {
                results.Add($"Using Local Emulator: Yes");
            }
            else
            {
                results.Add($"Using Local Emulator: No");
            }

            var account = await _cosmosClient.ReadAccountAsync();
            results.Add($"Endpoint: {_cosmosClient.Endpoint}");

            var database = _cosmosClient.GetDatabase(_cosmosOptions.DatabaseName);
            var dbResponse = await database.ReadAsync();
            results.Add($"Database '{_cosmosOptions.DatabaseName}' status: {dbResponse.StatusCode}");

            var sentContainer = database.GetContainer(_cosmosOptions.SentEmailsContainerName);
            var sentResponse = await sentContainer.ReadContainerAsync();
            results.Add($"Container '{_cosmosOptions.SentEmailsContainerName}' status: {sentResponse.StatusCode}");

            var receivedContainer = database.GetContainer(_cosmosOptions.ReceivedEmailsContainerName);
            var receivedResponse = await receivedContainer.ReadContainerAsync();
            results.Add($"Container '{_cosmosOptions.ReceivedEmailsContainerName}' status: {receivedResponse.StatusCode}");
        }
        catch (CosmosException cex)
        {
            isHealthy = false;
            results.Add($"CosmosException: Status={(int)cex.StatusCode} {cex.StatusCode}; Message={cex.Message}");
        }
        catch (Exception ex)
        {
            isHealthy = false;
            results.Add($"Exception: {ex.GetType().Name}; Message={ex.Message}");
        }
        
        ViewBag.CosmosDbResults = results;

        return (isHealthy, results);
    }
}