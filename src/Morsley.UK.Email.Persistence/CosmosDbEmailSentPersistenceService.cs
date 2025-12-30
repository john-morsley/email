namespace Morsley.UK.Email.Persistence;

public class CosmosDbEmailSentPersistenceService : CosmosDbEmailPersistenceService, IEmailSentPersistenceService
{
    public CosmosDbEmailSentPersistenceService(
        CosmosClient cosmosClient,
        CosmosDbOptions options, 
        ILoggerFactory loggerFactory) : base(
            cosmosClient,
            options.DatabaseName,
            options.SentEmailsContainerName,
            loggerFactory) { }    
}