//using Morsley.UK.Email.Common.Models;

//namespace Morsley.UK.Email.Common.Validation;

//public interface IEmailValidationService
//{
//    ValidationResult ValidateEmailMessage(SendableEmailMessage message);
//}

//public class EmailValidationService : IEmailValidationService
//{
//    public ValidationResult ValidateEmailMessage(SendableEmailMessage message)
//    {
//        var result = new ValidationResult();

//        // Check if at least one recipient exists
//        if (!message.To.Any() && !message.Cc.Any() && !message.Bcc.Any())
//        {
//            result.AddError("At least one recipient (To, Cc, or Bcc) is required");
//        }

//        // Validate total recipient count (prevent spam)
//        var totalRecipients = message.To.Count + message.Cc.Count + message.Bcc.Count;
//        if (totalRecipients > 100)
//        {
//            result.AddError("Total recipients cannot exceed 100");
//        }

//        // Ensure at least one body type is provided
//        if (string.IsNullOrWhiteSpace(message.TextBody) && string.IsNullOrWhiteSpace(message.HtmlBody))
//        {
//            result.AddError("Either TextBody or HtmlBody must be provided");
//        }

//        // Validate HTML body if provided
//        if (!string.IsNullOrWhiteSpace(message.HtmlBody))
//        {
//            if (!IsValidHtml(message.HtmlBody))
//            {
//                result.AddError("HTML body contains invalid HTML");
//            }
//        }

//        return result;
//    }

//    private static bool IsValidHtml(string html)
//    {
//        try
//        {
//            // Basic HTML validation - you might want to use a proper HTML parser like HtmlAgilityPack
//            return !html.Contains("<script>", StringComparison.OrdinalIgnoreCase) &&
//                   !html.Contains("javascript:", StringComparison.OrdinalIgnoreCase);
//        }
//        catch
//        {
//            return false;
//        }
//    }
//}

//public class ValidationResult
//{
//    public bool IsValid => !Errors.Any();
//    public List<string> Errors { get; } = new();

//    public void AddError(string error)
//    {
//        Errors.Add(error);
//    }
//}
