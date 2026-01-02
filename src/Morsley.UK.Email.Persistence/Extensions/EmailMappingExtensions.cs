namespace Morsley.UK.Email.Persistence.Extensions;

public static class EmailMappingExtensions
{
    public static EmailDocument ToDocument(this EmailMessage message)
    {
        if (message is null) throw new ArgumentNullException(nameof(message));

        var document = new EmailDocument
        {
            From = message.From,
            To = new List<string>(message.To),
            Cc = new List<string>(message.Cc),
            Bcc = new List<string>(message.Bcc),
            Subject = message.Subject,
            TextBody = message.TextBody,
            HtmlBody = message.HtmlBody,
            BatchNumber = message.BatchNumber
        };

        if (!string.IsNullOrEmpty(message.Id)) document.Id = message.Id;

        if (message.CreatedUtc is not null) document.CreatedUtc = (DateTime)message.CreatedUtc;

        return document;
    }

    public static EmailMessage ToSentEmailMessage(this EmailDocument document)
    {
        return new EmailMessage
        {
            Id = document.Id,
            From = document.From,
            To = new List<string>(document.To),
            Cc = new List<string>(document.Cc),
            Bcc = new List<string>(document.Bcc),
            Subject = document.Subject,
            TextBody = document.TextBody,
            HtmlBody = document.HtmlBody,            
            BatchNumber = document.BatchNumber,
            CreatedUtc = document.CreatedUtc
        };
    }

    /// <summary>
    /// Converts a collection of EmailDocuments to EmailMessages
    /// </summary>
    public static IEnumerable<EmailMessage> ToSentEmailMessages(this IEnumerable<EmailDocument> documents)
    {
        return documents.Select(x => x.ToSentEmailMessage());
    }
}