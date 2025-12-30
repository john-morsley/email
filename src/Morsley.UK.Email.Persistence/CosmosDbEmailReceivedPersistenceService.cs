namespace Morsley.UK.Email.Persistence;

public class CosmosDbEmailReceivedPersistenceService : CosmosDbEmailPersistenceService, IEmailReceivedPersistenceService
{
    public CosmosDbEmailReceivedPersistenceService(
        CosmosClient cosmosClient,
        CosmosDbOptions options,
        ILoggerFactory loggerFactory) : base(
            cosmosClient,
            options.DatabaseName,
            options.ReceivedEmailsContainerName,
            loggerFactory) { }
}