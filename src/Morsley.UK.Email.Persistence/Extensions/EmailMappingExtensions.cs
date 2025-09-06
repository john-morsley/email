namespace Morsley.UK.Email.Persistence.Extensions;

public static class EmailMappingExtensions
{
    public static Documents.EmailDocument ToDocument(this Common.Models.SendableEmailMessage emailMessage)
    {
        return new Documents.EmailDocument
        {
            //Id = emailMessage.Id,
            To = new List<string>(emailMessage.To),
            Cc = new List<string>(emailMessage.Cc),
            Bcc = new List<string>(emailMessage.Bcc),
            //ReplyTo = emailMessage.ReplyTo,
            Subject = emailMessage.Subject,
            TextBody = emailMessage.TextBody,
            HtmlBody = emailMessage.HtmlBody,
            //CreatedAt = emailMessage.CreatedAt,
            //SentAt = emailMessage.SentAt,
            //Status = emailMessage.Status
        };
    }

    public static Documents.EmailDocument ToDocument(this Common.Models.SentEmailMessage message)
    {
        if (message is null) throw new ArgumentNullException(nameof(message));

        var document = new Documents.EmailDocument
        {
            To = new List<string>(message.To),
            Cc = new List<string>(message.Cc),
            Bcc = new List<string>(message.Bcc),
            //ReplyTo = emailMessage.ReplyTo,
            Subject = message.Subject,
            TextBody = message.TextBody,
            HtmlBody = message.HtmlBody,
            //CreatedAt = emailMessage.CreatedAt,
            //SentAt = emailMessage.SentAt,
            //Status = emailMessage.Status
        };

        if (!string.IsNullOrEmpty(message.Id)) document.Id = message.Id;

        return document;
    }

    public static Common.Models.SentEmailMessage ToSentEmailMessage(this Documents.EmailDocument emailDocument)
    {
        return new Common.Models.SentEmailMessage
        {
            //Id = emailDocument.Id,
            To = new List<string>(emailDocument.To),
            Cc = new List<string>(emailDocument.Cc),
            Bcc = new List<string>(emailDocument.Bcc),
            //ReplyTo = emailDocument.ReplyTo,
            Subject = emailDocument.Subject,
            TextBody = emailDocument.TextBody,
            HtmlBody = emailDocument.HtmlBody,
            //CreatedAt = emailDocument.CreatedAt,
            //SentAt = emailDocument.SentAt,
            //Status = emailDocument.Status
        };
    }

    /// <summary>
    /// Converts a collection of EmailDocuments to EmailMessages
    /// </summary>
    public static IEnumerable<Common.Models.SentEmailMessage> ToSentEmailMessages(this IEnumerable<Documents.EmailDocument> emailDocuments)
    {
        return emailDocuments.Select(x => x.ToSentEmailMessage());
    }
}
