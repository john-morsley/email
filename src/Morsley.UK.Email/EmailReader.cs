namespace Morsley.UK.Email;

public class EmailReader : IEmailReader
{
    private readonly IOptionsMonitor<ImapSettings> _options;

    public EmailReader(IOptionsMonitor<ImapSettings> options)
    {
        _options = options;
    }

    public async Task<IReadOnlyList<MimeMessage>> FetchAsync(CancellationToken token = default)
    {
        var settings = _options.CurrentValue;

        using var client = new ImapClient();

        // In production, validate the cert properly or pin it.
        // client.ServerCertificateValidationCallback = (sender, cert, chain, errors) => true;

        await client.ConnectAsync(
            settings.Server, 
            settings.Port,
            settings.UseSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTlsWhenAvailable,
            token);

        await client.AuthenticateAsync(settings.Username, settings.Password, token);

        // Folder selection (default INBOX)
        var folder = client.Inbox;
        if (!string.Equals(settings.Folder, "INBOX", StringComparison.OrdinalIgnoreCase))
        {
            // If custom, try to open by name (top-level). Adapt as needed for nested folders.
            folder = await client.GetFolderAsync(settings.Folder, token);
        }

        await folder.OpenAsync(FolderAccess.ReadWrite, token);

        var query = settings.OnlyUnseen ? SearchQuery.NotSeen : SearchQuery.All;
        var uids = await folder.SearchAsync(query, token);

        var messages = new List<MimeMessage>(uids.Count);
        foreach (var uid in uids)
        {
            var msg = await folder.GetMessageAsync(uid, token);
            messages.Add(msg);

            if (settings.MarkSeen) await folder.AddFlagsAsync(uid, MessageFlags.Seen, true, token);
        }

        await client.DisconnectAsync(true, token);

        return messages;
    }
}