namespace Morsley.UK.Email.Models;

public class EmailMessage
{
    public List<string> To { get; set; } = new();

    public List<string> Cc { get; set; } = new();

    public List<string> Bcc { get; set; } = new();

    public string Subject { get; set; } = "";

    public string? TextBody { get; set; }

    public string? HtmlBody { get; set; }
}