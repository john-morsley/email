namespace Morsley.UK.Email;

public interface IEmailSender
{
    Task SendAsync(EmailMessage message, CancellationToken token = default);
}