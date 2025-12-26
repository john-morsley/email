namespace Morsley.UK.Email.Persistence.Extensions;

public static class EmailMappingExtensions
{
    public static Documents.EmailDocument ToDocument(this Common.Models.EmailMessage message)
    {
        if (message is null) throw new ArgumentNullException(nameof(message));

        var document = new EmailDocument
        {
            From = message.From,
            To = new List<string>(message.To),
            Cc = new List<string>(message.Cc),
            Bcc = new List<string>(message.Bcc),
            //ReplyTo = emailMessage.ReplyTo,
            Subject = message.Subject,
            TextBody = message.TextBody,
            HtmlBody = message.HtmlBody,
            //CreatedAt = emailMessage.CreatedAt,
            SentAt = message.SentAt,
            //Status = emailMessage.Status,
            BatchNumber = message.BatchNumber
        };

        if (message.Id is not null)
        {
            document.Id = message.Id;
        }

        if (!string.IsNullOrEmpty(message.Id)) document.Id = message.Id;

        return document;
    }

    public static Common.Models.EmailMessage ToSentEmailMessage(this EmailDocument document)
    {
        return new Common.Models.EmailMessage
        {
            Id = document.Id,
            From = document.From,
            To = new List<string>(document.To),
            Cc = new List<string>(document.Cc),
            Bcc = new List<string>(document.Bcc),
            //ReplyTo = emailDocument.ReplyTo,
            Subject = document.Subject,
            TextBody = document.TextBody,
            HtmlBody = document.HtmlBody,
            //CreatedAt = emailDocument.CreatedAt,
            SentAt = document.SentAt,
            //Status = emailDocument.Status
            BatchNumber = document.BatchNumber
        };
    }

    /// <summary>
    /// Converts a collection of EmailDocuments to EmailMessages
    /// </summary>
    public static IEnumerable<Common.Models.EmailMessage> ToSentEmailMessages(this IEnumerable<EmailDocument> documents)
    {
        return documents.Select(x => x.ToSentEmailMessage());
    }
}