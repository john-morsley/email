namespace Morsley.UK.Email.API.Controllers;

[ApiController]
[Route("api/email")]
public class EmailController(
    ILogger<EmailController> logger,
    IEmailSender emailSender,
    IEmailReceivedPersistenceService receivedPersistenceService,
    IEmailSentPersistenceService sentPersistenceService) : ControllerBase
{
    [HttpGet]
    [Route("received/id")]
    public async Task<IActionResult> GetReceivedByIdAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return BadRequest("Email ID cannot be null or empty");
        }

        logger.LogInformation("Getting email with ID: {EmailId}", id);

        try
        {
            var email = await receivedPersistenceService.GetByIdAsync(id);

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

    [HttpGet]
    [Route("sent/id")]
    public async Task<IActionResult> GetSentByIdAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return BadRequest("Email ID cannot be null or empty");
        }

        logger.LogInformation("Getting email with ID: {EmailId}", id);

        try
        {
            var email = await sentPersistenceService.GetByIdAsync(id);

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

    [HttpPost]
    [Route("send")]
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

            sent.From = message.From;
            //sent.SentAt = DateTime.UtcNow;
            
            var createdId = await sentPersistenceService.SaveAsync(sent);

            logger.LogInformation("Email sent and saved with ID: {EmailId}", createdId);

            return Created($"api/email/{createdId}", new { id = createdId });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error sending email");
            return StatusCode(500, "An error occurred while sending the email");
        }
    }

    [HttpDelete]
    [Route("received/id")]
    public async Task<IActionResult> DeleteReceivedById(string id)
    {
        logger.LogInformation("Deleting email with ID: {EmailId}", id);

        try
        {
            var deleted = await receivedPersistenceService.DeleteByIdAsync(id);

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

    [HttpDelete]
    [Route("sent/id")]
    public async Task<IActionResult> DeleteSentById(string id)
    {
        logger.LogInformation("Deleting email with ID: {EmailId}", id);

        try
        {
            var deleted = await sentPersistenceService.DeleteByIdAsync(id);

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
}