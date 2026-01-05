namespace Morsley.UK.Email;

public interface IEmailReader
{
    Task<IReadOnlyList<MimeMessage>> FetchAsync(CancellationToken token);
}