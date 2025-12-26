namespace Morsley.UK.Email.API.Extensions;

public static class MimeMessageExtensions
{
    public static EmailMessage ToSentEmailMessage(this MimeMessage mimeMessage)
    {
        var sentEmail = new EmailMessage
        {
            Subject = mimeMessage.Subject ?? string.Empty,
            SentAt = mimeMessage.Date.DateTime != DateTime.MinValue ? mimeMessage.Date.DateTime : DateTime.UtcNow
        };

        if (mimeMessage.From != null)
        {
            foreach (var address in mimeMessage.From.OfType<MailboxAddress>())
            {
                sentEmail.From = address.Address;
            }
        }

        if (mimeMessage.To != null)
        {
            foreach (var address in mimeMessage.To.OfType<MailboxAddress>())
            {
                sentEmail.To.Add(address.Address);
            }
        }

        if (mimeMessage.Cc != null)
        {
            foreach (var address in mimeMessage.Cc.OfType<MailboxAddress>())
            {
                sentEmail.Cc.Add(address.Address);
            }
        }

        if (mimeMessage.Bcc != null)
        {
            foreach (var address in mimeMessage.Bcc.OfType<MailboxAddress>())
            {
                sentEmail.Bcc.Add(address.Address);
            }
        }

        if (mimeMessage.Body != null)
        {
            if (mimeMessage.Body is TextPart textPart)
            {
                if (textPart.IsHtml)
                {
                    sentEmail.HtmlBody = textPart.Text;
                }
                else
                {
                    sentEmail.TextBody = textPart.Text;
                }
            }
            else if (mimeMessage.Body is Multipart multipart)
            {
                foreach (var part in multipart.OfType<TextPart>())
                {
                    if (part.IsHtml)
                    {
                        sentEmail.HtmlBody = part.Text;
                    }
                    else if (part.IsPlain)
                    {
                        sentEmail.TextBody = part.Text;
                    }
                }
            }
        }

        return sentEmail;
    }

    public static IEnumerable<EmailMessage> ToSentEmailMessages(this IEnumerable<MimeMessage> mimeMessages)
    {
        return mimeMessages.Select(x => x.ToSentEmailMessage());
    }
}
