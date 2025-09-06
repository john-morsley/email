namespace Morsley.UK.Email;

public interface IEmailSender
{
    Task SendAsync(Common.Models.SendableEmailMessage message, CancellationToken token = default);
}