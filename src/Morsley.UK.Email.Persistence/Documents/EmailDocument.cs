namespace Morsley.UK.Email.Persistence.Documents;

public class EmailDocument
{
    private DateTime _createdUtc = DateTime.UtcNow;

    [JsonProperty("id")]
    public string Id { get; set; } = Uuid.NewDatabaseFriendly(UUIDNext.Database.Other).ToString();

    [JsonProperty("partitionKey")]
    public string PartitionKey => Id;

    public string From { get; set; } = "";

    public List<string> To { get; set; } = new();

    public List<string> Cc { get; set; } = new();

    public List<string> Bcc { get; set; } = new();
    
    public string Subject { get; set; } = "";

    public string? TextBody { get; set; }

    public string? HtmlBody { get; set; }

    public DateTime CreatedUtc
    { 
        get => _createdUtc;
        set 
        {
            _createdUtc = value;
        }
    }

    public DateTime? SentAt { get; set; }

    public long? BatchNumber { get; set; }
}