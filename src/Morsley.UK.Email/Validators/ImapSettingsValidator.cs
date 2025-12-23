namespace Morsley.UK.Email.API.Validators;

public class ImapSettingsValidator : IValidateOptions<ImapSettings>
{
    public ValidateOptionsResult Validate(string? name, ImapSettings options)
    {
        var errors = new List<string>();

        if (options.Server == "[Stored in User Secrets]")
        {
            errors.Add("ImapSettings.Server is not configured. Please set it in user secrets.");
        }

        if (options.Username == "[Stored in User Secrets]")
        {
            errors.Add("ImapSettings.Username is not configured. Please set it in user secrets.");
        }

        if (options.Password == "[Stored in User Secrets]")
        {
            errors.Add("ImapSettings.Password is not configured. Please set it in user secrets.");
        }

        if (options.Folder == "[Stored in User Secrets]")
        {
            errors.Add("ImapSettings.Folder is not configured. Please set it in user secrets.");
        }

        if (errors.Any())
        {
            return ValidateOptionsResult.Fail(errors);
        }

        return ValidateOptionsResult.Success;
    }
}