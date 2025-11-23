using Morsley.UK.Email.Models;

namespace Morsley.UK.Email.API.Validators;

public class SmtpSettingsValidator : IValidateOptions<SmtpSettings>
{
    public ValidateOptionsResult Validate(string? name, SmtpSettings options)
    {
        var errors = new List<string>();

        if (options.Username == "[Stored in User Secrets]")
        {
            errors.Add("SmtpSettings.Username is not configured. Please set it in user secrets.");
        }

        if (options.Password == "[Stored in User Secrets]")
        {
            errors.Add("SmtpSettings.Password is not configured. Please set it in user secrets.");
        }

        if (options.FromAddress == "[Stored in User Secrets]")
        {
            errors.Add("SmtpSettings.FromAddress is not configured. Please set it in user secrets.");
        }

        //if (options.ToAddress == "[Stored in User Secrets]")
        //{
        //    errors.Add("SmtpSettings.ToAddress is not configured. Please set it in user secrets.");
        //}

        if (errors.Any())
        {
            return ValidateOptionsResult.Fail(errors);
        }

        return ValidateOptionsResult.Success;
    }
}