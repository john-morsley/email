namespace Morsley.UK.Email.API.Controllers;

[ApiController]
[Route("api/emails")]
public class EmailsController(
    ILogger<EmailsController> logger,
    IEmailReader emailReader,
    IEmailReceivedPersistenceService receivedEmailPersistenceService,
    IEmailSentPersistenceService sentEmailPersistenceService) : ControllerBase
{
    [HttpGet]
    [Route("received/page")]
    public async Task<IActionResult> GetReceivedPageAsync([FromQuery] PaginationRequest? pagination = null)
    {
        pagination ??= new PaginationRequest();

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        logger.LogInformation("Getting emails with pagination - Page: {Page}, PageSize: {PageSize}", pagination.Page, pagination.PageSize);

        try
        {
            await FetchAllAndPersist();

            var paginated = await receivedEmailPersistenceService.GetPageAsync(pagination);

            logger.LogInformation(
                "Retrieved {Count} emails (Page {Page}/{TotalPages})", 
                paginated.Count, 
                paginated.Page, 
                paginated.TotalPages);
            
            return Ok(paginated);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving emails");
            return StatusCode(500, "An error occurred while retrieving emails");
        }
    }

    [HttpGet]
    [Route("sent/page")]
    public async Task<IActionResult> GetSentPageAsync([FromQuery] PaginationRequest? pagination = null)
    {
        pagination ??= new PaginationRequest();

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        logger.LogInformation("Getting emails with pagination - Page: {Page}, PageSize: {PageSize}", pagination.Page, pagination.PageSize);

        try
        {
            await FetchAllAndPersist();

            var paginated = await sentEmailPersistenceService.GetPageAsync(pagination);

            logger.LogInformation(
                "Retrieved {Count} emails (Page {Page}/{TotalPages})",
                paginated.Count,
                paginated.Page,
                paginated.TotalPages);

            return Ok(paginated);
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

    private async Task FetchAllAndPersist()
    {
        var emails = await emailReader.FetchAsync();

        var batchNumber = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();

        foreach (var email in emails)
        {
            await PersistEmail(email, batchNumber);
        }

        //var receivedEmails = await receivedEmailPersistenceService.GetEmailsAsync();

        //return receivedEmails;
    }

    private async Task PersistEmail(EmailMessage email)
    {
        await receivedEmailPersistenceService.SaveAsync(email);
    }

    private async Task PersistEmail(MimeMessage message, long batchNumber)
    {
        var email = message.ToSentEmailMessage();
        email.BatchNumber = batchNumber;
        await PersistEmail(email);
    }
}