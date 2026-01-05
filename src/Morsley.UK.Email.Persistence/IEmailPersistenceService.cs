namespace Morsley.UK.Email.Persistence;

public interface IEmailPersistenceService
{
    Task<string> SaveAsync(EmailMessage email, CancellationToken cancellationToken);

    Task<EmailMessage?> GetByIdAsync(string id, CancellationToken cancellationToken);

    Task<PaginatedResponse<EmailMessage>> GetPageAsync(PaginationRequest pagination, CancellationToken cancellationToken);

    Task<bool> DeleteByIdAsync(string id, CancellationToken cancellationToken);

    Task DeleteAllAsync(CancellationToken cancellationToken);
}