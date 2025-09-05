namespace Morsley.UK.Email.Persistence;

/// <summary>
/// Extension methods for mapping between EmailMessage (domain) and EmailDocument (persistence)
/// </summary>
public static class EmailMappingExtensions
{
    /// <summary>
    /// Converts EmailMessage (domain model) to EmailDocument (persistence model)
    /// </summary>
    public static EmailDocument ToDocument(this EmailMessage emailMessage)
    {
        return new EmailDocument
        {
            Id = emailMessage.Id,
            To = new List<string>(emailMessage.To),
            Cc = new List<string>(emailMessage.Cc),
            Bcc = new List<string>(emailMessage.Bcc),
            ReplyTo = emailMessage.ReplyTo,
            Subject = emailMessage.Subject,
            TextBody = emailMessage.TextBody,
            HtmlBody = emailMessage.HtmlBody,
            CreatedAt = emailMessage.CreatedAt,
            SentAt = emailMessage.SentAt,
            Status = emailMessage.Status
        };
    }

    /// <summary>
    /// Converts EmailDocument (persistence model) to EmailMessage (domain model)
    /// </summary>
    public static EmailMessage ToEmailMessage(this EmailDocument emailDocument)
    {
        return new EmailMessage
        {
            Id = emailDocument.Id,
            To = new List<string>(emailDocument.To),
            Cc = new List<string>(emailDocument.Cc),
            Bcc = new List<string>(emailDocument.Bcc),
            ReplyTo = emailDocument.ReplyTo,
            Subject = emailDocument.Subject,
            TextBody = emailDocument.TextBody,
            HtmlBody = emailDocument.HtmlBody,
            CreatedAt = emailDocument.CreatedAt,
            SentAt = emailDocument.SentAt,
            Status = emailDocument.Status
        };
    }

    /// <summary>
    /// Converts a collection of EmailDocuments to EmailMessages
    /// </summary>
    public static IEnumerable<EmailMessage> ToEmailMessages(this IEnumerable<EmailDocument> emailDocuments)
    {
        return emailDocuments.Select(doc => doc.ToEmailMessage());
    }
}
