namespace Morsley.UK.Email.API.Controllers
{
    public class HomeController(
            ILogger<HomeController> logger,
            IConfiguration configuration) : Controller
    {
        public IActionResult Index()
        {
            var testSecret = configuration["morsley-uk-test-secret"];
            ViewBag.TestSecret = testSecret;
            return View();
        }
    }
}