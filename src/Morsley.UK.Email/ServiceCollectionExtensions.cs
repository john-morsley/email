using Morsley.UK.Email.Models;

namespace Morsley.UK.Email;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEmailSender(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName = "SmtpSettings")
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var test = configuration.GetSection(sectionName);

        services
            .AddOptions<SmtpSettings>()
            .Bind(configuration.GetSection(sectionName))
            .Validate(s => !string.IsNullOrWhiteSpace(s.Server), "SmtpSettings:Server is required")
            .Validate(s => s.Port > 0, "SmtpSettings:Port must be greater than 0")
            .Validate(s => !string.IsNullOrWhiteSpace(s.Username), "SmtpSettings:Username is required")
            .Validate(s => !string.IsNullOrWhiteSpace(s.Password), "SmtpSettings:Password is required")
            .ValidateOnStart();

        services.AddSingleton<IEmailSender, EmailSender>();

        return services;
    }

    public static IServiceCollection AddEmailSender(
        this IServiceCollection services,
        Action<SmtpSettings> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        services
            .AddOptions<SmtpSettings>()
            .Configure(configure)
            .Validate(s => !string.IsNullOrWhiteSpace(s.Server), "Smtp:Host is required")
            .Validate(s => s.Port > 0, "Smtp:Port must be > 0")
            .ValidateOnStart();

        services.AddSingleton<IEmailSender, EmailSender>();

        return services;
    }

    public static IServiceCollection AddEmailReader(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName = "ImapSettings")
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services
            .AddOptions<ImapSettings>()
            .Bind(configuration.GetSection(sectionName))
            .Validate(s => !string.IsNullOrWhiteSpace(s.Server), "Imap:Server is required")
            .Validate(s => s.Port > 0, "Imap:Port must be > 0")
            .Validate(s => !string.IsNullOrWhiteSpace(s.Username), "SmtpSettings:Username is required")
            .Validate(s => !string.IsNullOrWhiteSpace(s.Password), "SmtpSettings:Password is required")

            .ValidateOnStart();

        services.AddSingleton<IEmailReader, EmailReader>();
        return services;
    }

    public static IServiceCollection AddEmailReader(
        this IServiceCollection services,
        Action<ImapSettings> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        services
            .AddOptions<ImapSettings>()
            .Configure(configure)
            .Validate(s => !string.IsNullOrWhiteSpace(s.Server), "Mail:Host is required")
            .Validate(s => s.Port > 0, "Mail:Port must be > 0")
            .Validate(s => !string.IsNullOrWhiteSpace(s.Username), "SmtpSettings:Username is required")
            .Validate(s => !string.IsNullOrWhiteSpace(s.Password), "SmtpSettings:Password is required")
            .ValidateOnStart();

        services.AddSingleton<IEmailReader, EmailReader>();
        return services;
    }
}