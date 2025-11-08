namespace Morsley.UK.Email.API.Controllers;

[ApiController]
[Route("api/emails")]
public class EmailsController(
    ILogger<EmailsController> logger,
    IEmailReader emailReader,
    IReceivedEmailPersistenceService receivedEmailPersistenceService) : ControllerBase
{
    [HttpGet]
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