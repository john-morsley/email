namespace Morsley.UK.Email.API.Controllers;

public class HomeController(
    ILogger<HomeController> logger,
    IConfiguration configuration) : Controller
{
    public IActionResult Index()
    {
        var testSecret = configuration["test-secret"];
        var morsleyUkCosmosDbPrimaryReadWriteKey = configuration["morsley-uk-cosmos-db-primary-read-write-key"];
        var morsleyUkCosmosDbSecondaryReadWriteKey = configuration["morsley-uk-cosmos-db-secondary-read-write-key"];
        var morsleyUkCosmosDbPrimaryReadKey = configuration["morsley-uk-cosmos-db-primary-read-key"];
        var morsleyUkCosmosDbSecondaryReadKey = configuration["morsley-uk-cosmos-db-secondary-read-key"];

        ViewBag.TestSecret = testSecret;
        ViewBag.MorsleyUkCosmosDbPrimaryReadWriteKey = morsleyUkCosmosDbPrimaryReadWriteKey;
        ViewBag.MorsleyUkCosmosDbSecondaryReadWriteKey = morsleyUkCosmosDbSecondaryReadWriteKey;
        ViewBag.MorsleyUkCosmosDbPrimaryReadKey = morsleyUkCosmosDbPrimaryReadKey;
        ViewBag.MorsleyUkCosmosDbSecondaryReadKey = morsleyUkCosmosDbSecondaryReadKey;

        return View();
    }
}