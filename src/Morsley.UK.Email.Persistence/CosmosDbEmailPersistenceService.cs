namespace Morsley.UK.Email.Persistence;

public class CosmosDbEmailPersistenceService : IEmailPersistenceService
{
    private readonly Container _container;
    private readonly ILogger<CosmosDbEmailPersistenceService> _logger;

    public CosmosDbEmailPersistenceService(
        CosmosClient cosmosClient, 
        IConfiguration configuration,
        ILogger<CosmosDbEmailPersistenceService> logger)
    {
        _logger = logger;
        
        var databaseName = configuration["CosmosDb:DatabaseName"] ?? "EmailDatabase";
        var containerName = configuration["CosmosDb:ContainerName"] ?? "Emails";
        
        _container = cosmosClient.GetContainer(databaseName, containerName);
    }

    public async Task SaveEmailAsync(Common.Models.SentEmailMessage email)
    {
        try
        {
            //_logger.LogInformation("Saving email with ID: {EmailId}", email.Id);
            
            var emailDocument = email.ToDocument();


            var response = await _container.UpsertItemAsync(
                emailDocument, 
                new PartitionKey(emailDocument.PartitionKey));
            
            //_logger.LogInformation("Successfully saved email with ID: {EmailId}. Request charge: {RequestCharge}", email.Id, response.RequestCharge);
        }
        catch (CosmosException ex)
        {
            //_logger.LogError(ex, "Failed to save email with ID: {EmailId}. Status: {Status}, Message: {Message}", email.Id, ex.StatusCode, ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            //_logger.LogError(ex, "Unexpected error saving email with ID: {EmailId}", email.Id);
            throw;
        }
    }

    public async Task SaveEmailsAsync(IEnumerable<Common.Models.SentEmailMessage> emails)
    {
        var emailList = emails.ToList();
        _logger.LogInformation("Saving {Count} emails to Cosmos DB", emailList.Count);

        var tasks = emailList.Select(SaveEmailAsync);
        
        try
        {
            await Task.WhenAll(tasks);
            _logger.LogInformation("Successfully saved all {Count} emails", emailList.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save some or all emails");
            throw;
        }
    }

    public async Task<Common.Models.SentEmailMessage?> GetEmailByIdAsync(string id)
    {
        try
        {
            _logger.LogInformation("Retrieving email with ID: {EmailId}", id);
            
            // Since we don't know the partition key for the email, we need to query across partitions
            var query = _container.GetItemQueryIterator<EmailDocument>(
                $"SELECT * FROM c WHERE c.id = '{id}'");
            
            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();
                var emailDocument = response.FirstOrDefault();
                if (emailDocument != null)
                {
                    _logger.LogInformation("Successfully retrieved email with ID: {EmailId}", id);
                    return emailDocument.ToSentEmailMessage();
                }
            }
            
            _logger.LogWarning("Email with ID: {EmailId} not found", id);
            return null;
        }
        catch (CosmosException ex)
        {
            _logger.LogError(ex, "Failed to retrieve email with ID: {EmailId}. Status: {Status}", 
                id, ex.StatusCode);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving email with ID: {EmailId}", id);
            throw;
        }
    }

    public async Task<IEnumerable<Common.Models.SentEmailMessage>> GetAllEmailsAsync()
    {
        try
        {
            _logger.LogInformation("Retrieving all emails from Cosmos DB");
            
            var query = _container.GetItemQueryIterator<EmailDocument>(
                "SELECT * FROM c ORDER BY c.CreatedAt DESC");
            
            var emailDocuments = new List<EmailDocument>();
            
            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();
                emailDocuments.AddRange(response.ToList());
            }
            
            var emails = emailDocuments.ToSentEmailMessages();
            _logger.LogInformation("Successfully retrieved {Count} emails", emails.Count());
            return emails;
        }
        catch (CosmosException ex)
        {
            _logger.LogError(ex, "Failed to retrieve emails. Status: {Status}", ex.StatusCode);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving emails");
            throw;
        }
    }

    public async Task<IEnumerable<Common.Models.SentEmailMessage>> GetEmailsByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            _logger.LogInformation("Retrieving emails from {StartDate} to {EndDate}", startDate, endDate);
            
            var query = _container.GetItemQueryIterator<EmailDocument>(
                $"SELECT * FROM c WHERE c.CreatedAt >= '{startDate:yyyy-MM-ddTHH:mm:ssZ}' AND c.CreatedAt <= '{endDate:yyyy-MM-ddTHH:mm:ssZ}' ORDER BY c.CreatedAt DESC");
            
            var emailDocuments = new List<EmailDocument>();
            
            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();
                emailDocuments.AddRange(response.ToList());
            }
            
            var emails = emailDocuments.ToSentEmailMessages();
            _logger.LogInformation("Successfully retrieved {Count} emails for date range", emails.Count());
            return emails;
        }
        catch (CosmosException ex)
        {
            _logger.LogError(ex, "Failed to retrieve emails by date range. Status: {Status}", ex.StatusCode);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving emails by date range");
            throw;
        }
    }

    public async Task<IEnumerable<Common.Models.SentEmailMessage>> GetEmailsByMonthAsync(int year, int month)
    {
        try
        {
            var partitionKey = $"{year:0000}-{month:00}";
            _logger.LogInformation("Retrieving emails for partition key: {PartitionKey}", partitionKey);
            
            var query = _container.GetItemQueryIterator<EmailDocument>(
                "SELECT * FROM c ORDER BY c.CreatedAt DESC",
                requestOptions: new QueryRequestOptions
                {
                    PartitionKey = new PartitionKey(partitionKey)
                });
            
            var emailDocuments = new List<EmailDocument>();
            
            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();
                emailDocuments.AddRange(response.ToList());
            }
            
            var emails = emailDocuments.ToSentEmailMessages();
            _logger.LogInformation("Successfully retrieved {Count} emails for {Year}-{Month}", emails.Count(), year, month);
            return emails;
        }
        catch (CosmosException ex)
        {
            _logger.LogError(ex, "Failed to retrieve emails for {Year}-{Month}. Status: {Status}", year, month, ex.StatusCode);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving emails for {Year}-{Month}", year, month);
            throw;
        }
    }

    public async Task<bool> DeleteEmailAsync(string id)
    {
        try
        {
            _logger.LogInformation("Deleting email with ID: {EmailId}", id);
            
            // First, find the email to get its partition key
            var email = await GetEmailByIdAsync(id);
            if (email == null)
            {
                _logger.LogWarning("Email with ID: {EmailId} not found for deletion", id);
                return false;
            }
            
            var emailDocument = email.ToDocument();
            await _container.DeleteItemAsync<EmailDocument>(
                id, 
                new PartitionKey(emailDocument.PartitionKey));
            
            _logger.LogInformation("Successfully deleted email with ID: {EmailId}", id);
            return true;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Email with ID: {EmailId} not found for deletion", id);
            return false;
        }
        catch (CosmosException ex)
        {
            _logger.LogError(ex, "Failed to delete email with ID: {EmailId}. Status: {Status}", 
                id, ex.StatusCode);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error deleting email with ID: {EmailId}", id);
            throw;
        }
    }
}
