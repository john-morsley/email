using UUIDNext;

namespace Morsley.UK.Email.Common.Model;

public class EmailMessage
{
    public string Id { get; set; } = Uuid.NewDatabaseFriendly(Database.Other).ToString();

    public List<string> To { get; set; } = new();

    public List<string> Cc { get; set; } = new();

    public List<string> Bcc { get; set; } = new();

    public string? ReplyTo { get; set; }

    public string Subject { get; set; } = "";

    public string? TextBody { get; set; }

    public string? HtmlBody { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? SentAt { get; set; }

    public EmailStatus Status { get; set; } = EmailStatus.Draft;
}
