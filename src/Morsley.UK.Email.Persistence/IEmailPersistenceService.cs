namespace Morsley.UK.Email.Persistence;

public interface IEmailPersistenceService
{
    Task<string> SaveAsync(EmailMessage email, CancellationToken cancellationToken = default);

    Task<EmailMessage?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    Task<PaginatedResponse<EmailMessage>> GetPageAsync(PaginationRequest pagination, CancellationToken cancellationToken = default);

    Task<bool> DeleteByIdAsync(string id, CancellationToken cancellationToken = default);

    Task DeleteAllAsync(CancellationToken cancellationToken = default);
}