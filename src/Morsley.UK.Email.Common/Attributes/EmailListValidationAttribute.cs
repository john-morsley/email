using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Morsley.UK.Email.Common.Attributes;

public class EmailListValidationAttribute : ValidationAttribute
{
    private static readonly Regex EmailRegex = new(
        @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public override bool IsValid(object? value)
    {
        if (value is not List<string> emails)
            return true; // Let other attributes handle null/type validation

        return emails.All(email => !string.IsNullOrWhiteSpace(email) && EmailRegex.IsMatch(email.Trim()));
    }

    public override string FormatErrorMessage(string name)
    {
        return $"All email addresses in {name} must be valid email addresses.";
    }
}
