namespace Morsley.UK.Email;

public class ImapSettings
{
    public string Server { get; set; } = "";

    public int Port { get; set; } = 0;

    public bool UseSsl { get; set; } = true;

    public string Username { get; set; } = "";

    public string Password { get; set; } = "";

    public string Folder { get; set; } = "INBOX";

    public bool OnlyUnseen { get; set; } = true;

    public bool MarkSeen { get; set; } = true;
}