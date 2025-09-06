using Morsley.UK.Email.Common.Models;

namespace Morsley.UK.Email.API.Extensions;

public static class EmailMappingExtensions
{
    public static SentEmailMessage ToSentEmailMessage(this SendableEmailMessage sendable)
    {
        return new SentEmailMessage
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

    public static IEnumerable<SentEmailMessage> ToSentEmailMessages(this IEnumerable<SendableEmailMessage> sendableMessages)
    {
        return sendableMessages.Select(x => x.ToSentEmailMessage());
    }
}
