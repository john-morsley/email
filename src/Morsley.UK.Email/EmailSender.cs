namespace Morsley.UK.Email;

public class EmailSender : IEmailSender
{
    private readonly IOptionsMonitor<SmtpSettings> _options;

    public EmailSender(IOptionsMonitor<SmtpSettings> options)
    {
        _options = options;
    }

    public async Task SendAsync(Common.Models.EmailMessage message, CancellationToken token = default)
    {
        var settings = _options.CurrentValue;
        
        var mime = new MimeMessage();
        mime.From.Add(new MailboxAddress(settings.FromName, settings.FromAddress));
        foreach (var a in message.To) mime.To.Add(MailboxAddress.Parse(a));
        foreach (var a in message.Cc) mime.Cc.Add(MailboxAddress.Parse(a));
        foreach (var a in message.Bcc) mime.Bcc.Add(MailboxAddress.Parse(a));
        //if (!string.IsNullOrWhiteSpace(message.ReplyTo)) mime.ReplyTo.Add(MailboxAddress.Parse(message.ReplyTo));
        mime.ReplyTo.Add(MailboxAddress.Parse(settings.FromAddress));
        mime.Subject = message.Subject ?? "";

        var body = new BodyBuilder { TextBody = message.TextBody, HtmlBody = message.HtmlBody };
        mime.Body = body.ToMessageBody();

        using var client = new SmtpClient { Timeout = settings.TimeoutSeconds * 1000 };

        var secure =
            settings.UseSsl ? SecureSocketOptions.SslOnConnect :
            settings.UseStartTls ? SecureSocketOptions.StartTls :
            SecureSocketOptions.StartTlsWhenAvailable;

        // Skip certificate validation if configured (for development/testing only)
        if (settings.SkipCertificateValidation)
        {
            client.ServerCertificateValidationCallback = (sender, cert, chain, errors) => true;
        }

        try
        {
            await client.ConnectAsync(settings.Server, settings.Port, secure, token);

            if (!string.IsNullOrEmpty(settings.Username) &&
                !string.IsNullOrEmpty(settings.Password))
            {
                await client.AuthenticateAsync(settings.Username, settings.Password, token);
            }

            await client.SendAsync(mime, token);
        }
        catch (Exception)
        {
            throw;
        }
        finally
        {
            await client.DisconnectAsync(true, token);
        }

        message.From = settings.FromAddress;
    }
}