namespace Morsley.UK.Email.Persistence.Documents;

/// <summary>
/// CosmosDB-specific document model for email persistence
/// </summary>
public class EmailDocument
{
    private DateTime _createdAt = DateTime.UtcNow;

    [JsonProperty("id")]
    public string Id { get; set; } = Uuid.NewDatabaseFriendly(UUIDNext.Database.Other).ToString();

    [JsonProperty("partitionKey")]
    public string PartitionKey => _createdAt.ToString("yyyy-MM");

    public List<string> To { get; set; } = new();

    public List<string> Cc { get; set; } = new();

    public List<string> Bcc { get; set; } = new();
    
    public string Subject { get; set; } = "";

    public string? TextBody { get; set; }

    public string? HtmlBody { get; set; }

    public DateTime CreatedAt 
    { 
        get => _createdAt;
        set 
        {
            _createdAt = value;
        }
    }

    public DateTime? SentAt { get; set; }

    public long? BatchNumber { get; set; }
}
