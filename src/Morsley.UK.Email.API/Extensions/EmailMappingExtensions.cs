namespace Morsley.UK.Email.API.Extensions;

public static class EmailMappingExtensions
{
    public static EmailMessage ToEmailMessage(this SendableEmailMessage sendable)
    {
        return new EmailMessage
        {
            To = new List<string>(sendable.To),
            Cc = new List<string>(sendable.Cc),
            Bcc = new List<string>(sendable.Bcc),
            Subject = sendable.Subject,
            TextBody = sendable.TextBody,
            HtmlBody = sendable.HtmlBody,
            SentAt = DateTime.UtcNow
        };
    }
     public static IEnumerable<EmailMessage> ToEmailMessages(this IEnumerable<SendableEmailMessage> sendableMessages)
    {
        return sendableMessages.Select(x => x.ToEmailMessage());
    }
}