namespace Morsley.UK.Email;

public class EmailMessage
{
    public List<string> To { get; set; } = new();

    public List<string> Cc { get; set; } = new();

    public List<string> Bcc { get; set; } = new();

    public string? ReplyTo { get; set; }

    public string Subject { get; set; } = "";

    public string? TextBody { get; set; }

    public string? HtmlBody { get; set; }
}
