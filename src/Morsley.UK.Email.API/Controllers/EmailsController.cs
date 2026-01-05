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
    public async Task<IActionResult> GetReceivedPageAsync([FromQuery] PaginationRequest? pagination = null, CancellationToken cancellationToken = default)
    {
        pagination ??= new PaginationRequest();

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        logger.LogInformation("Getting emails with pagination - Page: {Page}, PageSize: {PageSize}", pagination.Page, pagination.PageSize);

        try
        {
            await FetchAllAndPersist(cancellationToken);

            var paginated = await receivedPersistenceService.GetPageAsync(pagination, cancellationToken);

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
    public async Task<IActionResult> GetSentPageAsync([FromQuery] PaginationRequest? pagination = null, CancellationToken cancellationToken = default)
    {
        pagination ??= new PaginationRequest();

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        logger.LogInformation("Getting emails with pagination - Page: {Page}, PageSize: {PageSize}", pagination.Page, pagination.PageSize);

        try
        {
            await FetchAllAndPersist(cancellationToken);

            var paginated = await sentPersistenceService.GetPageAsync(pagination, cancellationToken);

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
    public async Task<IActionResult> DeleteAllReceived(CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Deleting all received emails");

        try
        {
            await receivedPersistenceService.DeleteAllAsync(cancellationToken);
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
    public async Task<IActionResult> DeleteAllSent(CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Deleting all sent emails");

        try
        {
            await sentPersistenceService.DeleteAllAsync(cancellationToken);
            return NoContent();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting all sent emails");
            return StatusCode(500, "An error occurred while deleting all sent emails");
        }
    }

    private async Task FetchAllAndPersist(CancellationToken cancellationToken = default)
    {
        var emails = await emailReader.FetchAsync(cancellationToken);

        var batchNumber = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();

        foreach (var email in emails)
        {
            await PersistEmail(email, batchNumber, cancellationToken);
        }
    }

    //private async Task PersistEmail(EmailMessage email, long batchNumber, CancellationToken cancellationToken = default)
    //{
    //    email.BatchNumber = batchNumber;
    //    await PersistEmail(email, cancellationToken);
    //}

    private async Task PersistEmail(EmailMessage email, CancellationToken cancellationToken)
    {
        await receivedPersistenceService.SaveAsync(email, cancellationToken);
    }
    private async Task PersistEmail(MimeMessage message, long batchNumber, CancellationToken cancellationToken = default)
    {
        var email = message.ToSentEmailMessage();
        email.BatchNumber = batchNumber;
        await PersistEmail(email, cancellationToken);
    }
}