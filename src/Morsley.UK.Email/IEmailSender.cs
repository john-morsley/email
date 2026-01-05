namespace Morsley.UK.Email;

public interface IEmailSender
{
    Task SendAsync(Common.Models.EmailMessage message, CancellationToken token);
}