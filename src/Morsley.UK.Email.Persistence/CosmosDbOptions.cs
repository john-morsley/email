namespace Morsley.UK.Email.Persistence;

public class CosmosDbOptions
{
    private const string LocalEmulatorEndpoint = "https://localhost:8081";
    private const string LocalEmulatorPrimaryKey = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";

    public bool UseLocalEmulator { get; set; } = false;

    private string _endpoint = string.Empty;

    public string Endpoint 
    {
        get
        {
            if (UseLocalEmulator) return LocalEmulatorEndpoint;
            if (string.IsNullOrEmpty(_endpoint)) throw new InvalidOperationException("Primary Key cannot be empty.");
            return _endpoint;
        }
        set
        {
            _endpoint = value;
        } 
    }

    private string _primaryReadWriteKey = string.Empty;

    public string PrimaryReadWriteKey
    {
        get
        {
            if (UseLocalEmulator) return LocalEmulatorPrimaryKey;
            if (string.IsNullOrEmpty(_primaryReadWriteKey)) throw new InvalidOperationException("Primary Read Write Key cannot be empty.");
            return _primaryReadWriteKey;
        }
        set
        {
            _primaryReadWriteKey = value;
        }
    }

    private string _secondaryReadWriteKey = string.Empty;

    public string SecondaryReadWriteKey
    {
        get
        {
            if (UseLocalEmulator) return LocalEmulatorPrimaryKey;
            if (string.IsNullOrEmpty(_secondaryReadWriteKey)) throw new InvalidOperationException("Secondary Read Write Key cannot be empty.");
            return _secondaryReadWriteKey;
        }
        set
        {
            _secondaryReadWriteKey = value;
        }
    }

    private string _primaryReadKey = string.Empty;

    public string PrimaryReadKey
    {
        get
        {
            if (UseLocalEmulator) return LocalEmulatorPrimaryKey;
            if (string.IsNullOrEmpty(_primaryReadKey)) throw new InvalidOperationException("Primary Read Key cannot be empty.");
            return _primaryReadKey;
        }
        set
        {
            _primaryReadKey = value;
        }
    }

    private string _secondaryReadKey = string.Empty;

    public string SecondaryReadKey
    {
        get
        {
            if (UseLocalEmulator) return LocalEmulatorPrimaryKey;
            if (string.IsNullOrEmpty(_secondaryReadKey)) throw new InvalidOperationException("Secondary Read Key cannot be empty.");
            return _secondaryReadKey;
        }
        set
        {
            _secondaryReadKey = value;
        }
    }

    public string DatabaseName { get; set; } = string.Empty;

    public string PartitionKey { get; set; } = "/id";

    public string SentEmailsContainerName { get; set; } = "emails-sent";

    public string ReceivedEmailsContainerName { get; set; } = "emails-received";
}