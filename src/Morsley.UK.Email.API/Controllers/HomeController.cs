using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using Morsley.UK.Email.Persistence;
using Morsley.UK.Email.API.Extensions;
using System.Collections.Generic;

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

        var cosmosHealth = await IsCosmoDbHealthy();
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
        details = new List<string>();

        var testSecret = _configuration["test-secret"];
        var expectedTestSecretValue = _configuration["ExpectedTestSecretValue"];

        if (testSecret != expectedTestSecretValue) return false;

        var morsleyUkCosmosDbPrimaryReadWriteKey = _configuration["morsley-uk-cosmos-db-primary-read-write-key"];
        details.Add($"morsley-uk-cosmos-db-primary-read-write-key: {morsleyUkCosmosDbPrimaryReadWriteKey.ToMaskedSecret()}");
        var morsleyUkCosmosDbSecondaryReadWriteKey = _configuration["morsley-uk-cosmos-db-secondary-read-write-key"];
        details.Add($"morsley-uk-cosmos-db-secondary-read-write-key: {morsleyUkCosmosDbSecondaryReadWriteKey.ToMaskedSecret()}");
        var morsleyUkCosmosDbPrimaryReadKey = _configuration["morsley-uk-cosmos-db-primary-read-key"];
        details.Add($"morsley-uk-cosmos-db-primary-read-key: {morsleyUkCosmosDbPrimaryReadKey.ToMaskedSecret()}");
        var morsleyUkCosmosDbSecondaryReadKey = _configuration["morsley-uk-cosmos-db-secondary-read-key"];
        details.Add($"morsley-uk-cosmos-db-secondary-read-key: {morsleyUkCosmosDbSecondaryReadKey.ToMaskedSecret()}");

        return true;
    }

    private async Task<(bool IsHealthy, IList<string> Details)> IsCosmoDbHealthy()
    {
        var results = new List<string>();
        var isHealthy = true;

        try
        {
            var account = await _cosmosClient.ReadAccountAsync();
            results.Add($"Account endpoint: {_cosmosClient.Endpoint}");

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