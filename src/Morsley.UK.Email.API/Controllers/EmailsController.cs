namespace Morsley.UK.Email.API.Controllers;

[ApiController]
[Route("api/emails")]
public class EmailsController(
    ILogger<EmailsController> logger,
    IEmailReader emailReader,
    IEmailReceivedPersistenceService receivedPersistenceService,
    IEmailSentPersistenceService sentPersistenceService) : ControllerBase
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

            var paginated = await receivedPersistenceService.GetPageAsync(pagination);

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

            var paginated = await sentPersistenceService.GetPageAsync(pagination);

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


    [HttpDelete]
    [Route("received/all")]
    public async Task<IActionResult> DeleteAllReceived()
    {
        logger.LogInformation("Deleting all received emails");

        try
        {
            await receivedPersistenceService.DeleteAllAsync();
            return NoContent();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting all recieved emails");
            return StatusCode(500, "An error occurred while deleting all recieved emails");
        }
    }

    [HttpDelete]
    [Route("sent/all")]
    public async Task<IActionResult> DeleteAllSent()
    {
        logger.LogInformation("Deleting all sent emails");

        try
        {
            await sentPersistenceService.DeleteAllAsync();
            return NoContent();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting all sent emails");
            return StatusCode(500, "An error occurred while deleting all sent emails");
        }
    }

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
        await receivedPersistenceService.SaveAsync(email);
    }

    private async Task PersistEmail(MimeMessage message, long batchNumber)
    {
        var email = message.ToSentEmailMessage();
        email.BatchNumber = batchNumber;
        await PersistEmail(email);
    }
}