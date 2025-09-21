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
}