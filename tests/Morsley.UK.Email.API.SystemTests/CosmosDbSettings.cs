namespace Morsley.UK.Email.API.SystemTests;

public class CosmosDbSettings
{
    public string Endpoint { get; set; } = string.Empty;

    public string DatabaseName { get; set; } = string.Empty;

    public string PartitionKey { get; set; } = string.Empty;

    public string SentEmailsContainerName { get; set; } = string.Empty;

    public string ReceivedEmailsContainerName { get; set; } = string.Empty;
}