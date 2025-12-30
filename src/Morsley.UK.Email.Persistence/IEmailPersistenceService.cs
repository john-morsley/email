namespace Morsley.UK.Email.Persistence;

public interface IEmailPersistenceService
{
    Task<string> SaveAsync(Common.Models.EmailMessage email, CancellationToken cancellationToken = default);

    Task<Common.Models.EmailMessage?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    Task<Common.Models.PaginatedResponse<Common.Models.EmailMessage>> GetPageAsync(Common.Models.PaginationRequest pagination, CancellationToken cancellationToken = default);

    Task<bool> DeleteByIdAsync(string id, CancellationToken cancellationToken = default);
}