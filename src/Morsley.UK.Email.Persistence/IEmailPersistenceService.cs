namespace Morsley.UK.Email.Persistence;

public interface IEmailPersistenceService
{
    Task SaveEmailAsync(Common.Models.SentEmailMessage email);

    //Task SaveEmailsAsync(IEnumerable<EmailMessage> emails);

    Task<Common.Models.SentEmailMessage?> GetEmailByIdAsync(string id);

    Task<IEnumerable<Common.Models.SentEmailMessage>> GetAllEmailsAsync();

    //Task<IEnumerable<EmailMessage>> GetEmailsByDateRangeAsync(DateTime startDate, DateTime endDate);

    //Task<IEnumerable<EmailMessage>> GetEmailsByMonthAsync(int year, int month);

    Task<bool> DeleteEmailAsync(string id);    
}
