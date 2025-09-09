namespace Morsley.UK.Email.Persistence;

public interface IEmailPersistenceService
{
    Task<string> SaveEmailAsync(Common.Models.EmailMessage email, CancellationToken cancellationToken = default);

    Task<IEnumerable<string>> SaveEmailsAsync(IEnumerable<Common.Models.EmailMessage> emails, CancellationToken cancellationToken = default);

    Task<Common.Models.EmailMessage?> GetEmailAsync(string id, CancellationToken cancellationToken = default);

    Task<IEnumerable<Common.Models.EmailMessage>> GetEmailsAsync(CancellationToken cancellationToken = default);

    Task<Common.Models.PaginatedResponse<Common.Models.EmailMessage>> GetEmailsAsync(Common.Models.PaginationRequest pagination, CancellationToken cancellationToken = default);

    Task<bool> DeleteEmailAsync(string id, CancellationToken cancellationToken = default);

    Task<int> DeleteEmailsAsync(CancellationToken cancellationToken = default);
}
