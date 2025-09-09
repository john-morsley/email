namespace Morsley.UK.Email.Common.Models;

public class EmailMessage
{
    public string? Id { get; set; }

    public List<string> To { get; set; } = new();

    public List<string> Cc { get; set; } = new();

    public List<string> Bcc { get; set; } = new();

    public string Subject { get; set; } = "";

    public string? TextBody { get; set; }

    public string? HtmlBody { get; set; }

    public DateTime? SentAt { get; set; }

    public long? BatchNumber { get; set; }
}