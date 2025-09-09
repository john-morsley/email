namespace Morsley.UK.Email.Persistence;

public class CosmosDbOptions
{
    public string Endpoint { get; set; } = string.Empty;
    public string PrimaryKey { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
    public string SentEmailsContainerName { get; set; } = "sent-emails";
    public string ReceivedEmailsContainerName { get; set; } = "received-emails";
    
    // Keep for backward compatibility
    [Obsolete("Use SentEmailsContainerName instead")]
    public string ContainerName 
    { 
        get => SentEmailsContainerName; 
        set => SentEmailsContainerName = value; 
    }
}
