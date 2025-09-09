namespace Morsley.UK.Email.API.Controllers;

[ApiController]
[Route("api/email")]
public class EmailController(
    ILogger<EmailController> logger,
    IEmailReader emailReader,
    IEmailSender emailSender,
    IReceivedEmailPersistenceService receivedEmailPersistenceService,
    ISentEmailPersistenceService sentEmailPersistenceService) : ControllerBase
{
    [HttpGet("all", Name = "all")]
    public async Task<IActionResult> GetAll([FromQuery] Common.Models.PaginationRequest? pagination = null)
    {
        // Use default pagination if not provided
        pagination ??= new Common.Models.PaginationRequest();

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        logger.LogInformation("Getting emails with pagination - Page: {Page}, PageSize: {PageSize}", pagination.Page, pagination.PageSize);

        try
        {
            // First ensure we have the latest emails persisted
            await GetAllAndPersist();

            // Get paginated emails from persistence
            var paginatedEmails = await receivedEmailPersistenceService.GetEmailsAsync(pagination);

            logger.LogInformation("Retrieved {Count} emails (Page {Page}/{TotalPages})", 
                paginatedEmails.Count, paginatedEmails.Page, paginatedEmails.TotalPages);
            
            return Ok(paginatedEmails);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving emails");
            return StatusCode(500, "An error occurred while retrieving emails");
        }
    }

    //[HttpGet("{id}", Name = "get-by-id")]
    //public async Task<IActionResult> GetById(string id)
    //{
    //    if (string.IsNullOrWhiteSpace(id))
    //    {
    //        return BadRequest("Email ID cannot be null or empty");
    //    }

    //    logger.LogInformation("Getting email with ID: {EmailId}", id);

    //    try
    //    {
    //        var email = await persistenceService.GetEmailByIdAsync(id);

    //        if (email == null)
    //        {
    //            return NotFound($"Email with ID {id} not found");
    //        }

    //        return Ok(email);
    //    }
    //    catch (Exception ex)
    //    {
    //        logger.LogError(ex, "Error retrieving email with ID: {EmailId}", id);
    //        return StatusCode(500, "An error occurred while retrieving the email");
    //    }
    //}

    [HttpPost(Name = "send")]
    public async Task<IActionResult> Send([FromBody] SendableEmailMessage sendable)
    {
        logger.LogInformation("Sending email with subject: {Subject}", sendable.Subject);

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var message = sendable.ToEmailMessage();

            await emailSender.SendAsync(message);
            
            var sent = sendable.ToEmailMessage();

            sent.SentAt = DateTime.UtcNow;
            
            var createdId = await sentEmailPersistenceService.SaveEmailAsync(sent);

            logger.LogInformation("Email sent and saved with ID: {EmailId}", createdId);
            return Created($"api/email/{createdId}", new { id = createdId });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error sending email");
            return StatusCode(500, "An error occurred while sending the email");
        }
    }

    //[HttpDelete("{id}", Name = "delete-by-id")]
    //public async Task<IActionResult> DeleteById(string id)
    //{
    //    logger.LogInformation("Deleting email with ID: {EmailId}", id);

    //    try
    //    {
    //        var deleted = await persistenceService.DeleteEmailAsync(id);
            
    //        if (!deleted)
    //        {
    //            return NotFound($"Email with ID {id} not found");
    //        }
            
    //        return NoContent();
    //    }
    //    catch (Exception ex)
    //    {
    //        logger.LogError(ex, "Error deleting email with ID: {EmailId}", id);
    //        return StatusCode(500, "An error occurred while deleting the email");
    //    }
    //}

    private async Task<IEnumerable<Common.Models.EmailMessage>> GetAllAndPersist()
    {
        var emails = await emailReader.FetchAsync();

        var batchNumber = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();

        foreach (var email in emails)
        {
            await PersistEmail(email, batchNumber);
        }

        var receivedEmails = await receivedEmailPersistenceService.GetEmailsAsync();

        return receivedEmails;
    }

    private async Task PersistEmail(Common.Models.EmailMessage email)
    {
        await receivedEmailPersistenceService.SaveEmailAsync(email);
    }

    private async Task PersistEmail(MimeKit.MimeMessage message, long batchNumber)
    {
        var email = message.ToSentEmailMessage();
        email.BatchNumber = batchNumber;
        await PersistEmail(email);
    }
}