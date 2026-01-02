namespace Morsley.UK.Email.Persistence;

public abstract class CosmosDbEmailPersistenceService
{
    protected readonly Container _container;

    public CosmosDbEmailPersistenceService(
        CosmosClient cosmosClient,
        string databaseName,
        string containerName,
        ILoggerFactory loggerFactory)
    {
        _container = cosmosClient.GetContainer(databaseName, containerName);

        var t = GetType();
        Logger = loggerFactory.CreateLogger(t);
    }

    protected ILogger Logger { get; }

    public async Task<string> SaveAsync(Common.Models.EmailMessage email, CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogInformation("Saving email");

            var emailDocument = email.ToDocument();

            var response = await _container.UpsertItemAsync(
                emailDocument,
                new PartitionKey(emailDocument.PartitionKey));

            Logger.LogInformation("Successfully saved email with ID: {EmailId}. Request charge: {RequestCharge}", email.Id, response.RequestCharge);

            return response.Resource.Id;
        }
        catch (CosmosException ex)
        {
            Logger.LogError(ex, "Failed to save email: Status: {Status}, Message: {Message}", ex.StatusCode, ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Unexpected error saving email");
            throw;
        }
    }

    public async Task<Common.Models.EmailMessage?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogInformation("Retrieving email with ID: {EmailId}", id);

            // Since we don't know the partition key for the email, we need to query across partitions
            var query = _container.GetItemQueryIterator<EmailDocument>(
                $"""
                SELECT * " +
                  FROM c " +
                 WHERE c.id = '{id}'
                """);

            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();
                var emailDocument = response.FirstOrDefault();
                if (emailDocument != null)
                {
                    Logger.LogInformation("Successfully retrieved email with ID: {EmailId}", id);
                    return emailDocument.ToSentEmailMessage();
                }
            }

            Logger.LogWarning("Email with ID: {EmailId} not found", id);
            return null;
        }
        catch (CosmosException ex)
        {
            Logger.LogError(ex, "Failed to retrieve email with ID: {EmailId}. Status: {Status}", id, ex.StatusCode);
            throw;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Unexpected error retrieving email with ID: {EmailId}", id);
            throw;
        }
    }

    public async Task<PaginatedResponse<EmailMessage>> GetPageAsync(PaginationRequest pagination, CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogInformation("Retrieving emails with pagination - Page: {Page}, PageSize: {PageSize}", pagination.Page, pagination.PageSize);

            // First, get the total count
            var countQuery = _container.GetItemQueryIterator<int>("SELECT VALUE COUNT(1) FROM c");

            int totalCount = 0;
            while (countQuery.HasMoreResults)
            {
                var countResponse = await countQuery.ReadNextAsync();
                totalCount = countResponse.FirstOrDefault();
                break;
            }

            // Then get the paginated results
            var queryText =
                $"""
                  SELECT *
                    FROM c
                ORDER BY c.CreatedUtc DESC
                  OFFSET {pagination.Skip}
                   LIMIT {pagination.PageSize}
                """;

            var query = _container.GetItemQueryIterator<EmailDocument>(queryText);

            var pageOfEmailDocuments = new List<EmailDocument>();

            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();
                pageOfEmailDocuments.AddRange(response.ToList());
            }

            var pageOfEmailMessages = pageOfEmailDocuments.ToSentEmailMessages();

            var paginatedResponse = new Common.Models.PaginatedResponse<Common.Models.EmailMessage>
            {
                Items = pageOfEmailMessages,
                Page = pagination.Page,
                PageSize = pagination.PageSize,
                TotalItems = totalCount
            };

            Logger.LogInformation(
                "Successfully retrieved {Count} emails (Page {Page}/{TotalPages})",
                pageOfEmailMessages.Count(),
                paginatedResponse.Page,
                paginatedResponse.TotalPages);

            return paginatedResponse;
        }
        catch (CosmosException ex)
        {
            Logger.LogError(ex, "Failed to retrieve paginated emails. Status: {Status}", ex.StatusCode);
            throw;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Unexpected error retrieving paginated emails");
            throw;
        }
    }

    public async Task<bool> DeleteByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogInformation("Deleting email with ID: {EmailId}", id);

            // First, find the email to get its partition key
            var email = await GetByIdAsync(id, cancellationToken);
            if (email == null)
            {
                Logger.LogWarning("Email with ID: {EmailId} not found for deletion", id);
                return false;
            }

            var emailDocument = email.ToDocument();
            await _container.DeleteItemAsync<EmailDocument>(
                id,
                new PartitionKey(emailDocument.PartitionKey));

            Logger.LogInformation("Successfully deleted email with ID: {EmailId}", id);
            return true;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            Logger.LogWarning("Email with ID: {EmailId} not found for deletion", id);
            return false;
        }
        catch (CosmosException ex)
        {
            Logger.LogError(ex, "Failed to delete email with ID: {EmailId}. Status: {Status}", id, ex.StatusCode);
            throw;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Unexpected error deleting email with ID: {EmailId}", id);
            throw;
        }
    }
}