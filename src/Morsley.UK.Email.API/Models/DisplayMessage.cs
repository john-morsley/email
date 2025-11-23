namespace Morsley.UK.Email.API.Models;

public class DisplayMessage
{
    public string Message { get; set; } = string.Empty;

    public MessageType Type { get; set; } = MessageType.Neutral;

    public IEnumerable<string> Details { get; set; } = Enumerable.Empty<string>();
}

public enum MessageType
{
    Neutral,
    Good,
    Bad,
    Warning    
}