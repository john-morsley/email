namespace Morsley.UK.Email.Common.Model;

/// <summary>
/// Represents the status of an email message
/// </summary>
public enum EmailStatus
{
    /// <summary>
    /// Email is in draft state and has not been sent
    /// </summary>
    Draft = 0,
    
    /// <summary>
    /// Email has been successfully sent
    /// </summary>
    Sent = 1,
    
    /// <summary>
    /// Email sending failed
    /// </summary>
    Failed = 2
}
