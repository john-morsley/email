using Morsley.UK.Email.Common.Model;

namespace Morsley.UK.Email.Persistence;

public interface IEmailPersistenceService
{
    Task SaveEmailAsync(EmailMessage email);
    Task SaveEmailsAsync(IEnumerable<EmailMessage> emails);
    Task<EmailMessage?> GetEmailByIdAsync(string id);
    Task<IEnumerable<EmailMessage>> GetAllEmailsAsync();
    Task<IEnumerable<EmailMessage>> GetEmailsByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<EmailMessage>> GetEmailsByMonthAsync(int year, int month);
    Task<bool> DeleteEmailAsync(string id);
}
