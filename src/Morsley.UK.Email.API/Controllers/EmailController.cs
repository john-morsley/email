namespace Morsley.UK.Email.API.Controllers;

[ApiController]
[Route("api/email")]
public class EmailController(
    ILogger<EmailController> logger,
    IEmailReader emailReader,
    IEmailSender emailSender,
    IEmailPersistenceService persistenceService) : ControllerBase
{
    [HttpGet("all", Name = "get-all")]
    public async Task<IActionResult> GetAll()
    {
        logger.LogInformation("Getting all emails");

        try
        {
            await GetAllAndPersist();

            // ToDo --> Read emails from an email reader service.
            var emails = await persistenceService.GetAllEmailsAsync();
            
            logger.LogInformation("Retrieved {Count} emails", emails.Count());
            return Ok(emails);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving emails");
            return StatusCode(500, "An error occurred while retrieving emails");
        }
    }

    [HttpGet("{id}", Name = "get-by-id")]
    public async Task<IActionResult> GetById(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return BadRequest("Email ID cannot be null or empty");
        }

        logger.LogInformation("Getting email with ID: {EmailId}", id);

        try
        {
            var email = await persistenceService.GetEmailByIdAsync(id);
            
            if (email == null)
            {
                return NotFound($"Email with ID {id} not found");
            }
            
            return Ok(email);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving email with ID: {EmailId}", id);
            return StatusCode(500, "An error occurred while retrieving the email");
        }
    }

    [HttpPost(Name = "send")]
    public async Task<IActionResult> Send([FromBody] Common.Models.SendableEmailMessage sendable)
    {
        logger.LogInformation("Sending email with subject: {Subject}", sendable.Subject);

        // Model validation (Data Annotations)
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            await emailSender.SendAsync(sendable);
            
            var sent = sendable.ToSentEmailMessage();

            sent.SentAt = DateTime.UtcNow;
            
            await persistenceService.SaveEmailAsync(sent);

            //logger.LogInformation("Email sent and saved with ID: {EmailId}", email.Id);
            return Ok(); // (new { Id = email.Id, Status = "Sent" });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error sending email");
            //email.Status = EmailStatus.Failed;
            //await persistenceService.SaveEmailAsync(email);
            return StatusCode(500, "An error occurred while sending the email");
        }
    }

    [HttpDelete("{id}", Name = "delete-by-id")]
    public async Task<IActionResult> DeleteById(string id)
    {
        logger.LogInformation("Deleting email with ID: {EmailId}", id);

        try
        {
            var deleted = await persistenceService.DeleteEmailAsync(id);
            
            if (!deleted)
            {
                return NotFound($"Email with ID {id} not found");
            }
            
            return NoContent();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting email with ID: {EmailId}", id);
            return StatusCode(500, "An error occurred while deleting the email");
        }
    }

    private async Task GetAllAndPersist()
    {
        var emails = await emailReader.FetchAsync();

        foreach (var email in emails)
        {
            await PersistEmail(email);
        }
    }

    private async Task PersistEmail(Common.Models.SentEmailMessage email)
    {
        await persistenceService.SaveEmailAsync(email);
    }

    private async Task PersistEmail(MimeKit.MimeMessage email)
    {
        var sentEmail = email.ToSentEmailMessage();
        await PersistEmail(email);
    }
}