namespace Morsley.UK.Email.API.Controllers;

[ApiController]
[Route("api/email")]
public class EmailController(ILogger<EmailController> logger) : ControllerBase
{
    [HttpGet(Name = "get-all")]
    public IEnumerable<EmailMessage> Get()
    {
        // ToDo --> Log call

        var emails = new List<EmailMessage>();

        // ToDo --> Read emails from an email reader service.

        for (int i = 1; i <= 5; i++)
        {
            var email = new EmailMessage();

            emails.Add(email);
        }

        return emails;
    }

    [HttpPost(Name = "send")]
    public IActionResult Post([FromBody] EmailMessage email)
    {
        // ToDo --> Log call

        // ToDo --> Send the email using an email sender service.

        return Ok();
    }
}