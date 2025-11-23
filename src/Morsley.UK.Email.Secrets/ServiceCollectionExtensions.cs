namespace Morsley.UK.Email.Secrets;

public static class ServiceCollectionExtensions
{
    public static WebApplicationBuilder ConfigureAzureKeyVault(this WebApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        var configuration = builder.Configuration;

        // Configure KeyVault settings
        builder.Services
            .AddOptions<KeyVaultSettings>()
            .Bind(configuration.GetSection("KeyVault"))
            .Validate(options => !string.IsNullOrWhiteSpace(options.Name), "KeyVault:Name is required")
            .ValidateOnStart();

        // Configure Azure settings
        builder.Services
            .AddOptions<AzureSettings>()
            .Bind(configuration.GetSection("Azure"))
            .ValidateOnStart();

        // Build a temporary service provider to get the options
        using var serviceProvider = builder.Services.BuildServiceProvider();
        var keyVaultSettings = serviceProvider.GetRequiredService<IOptions<KeyVaultSettings>>().Value;
        var azureSettings = serviceProvider.GetRequiredService<IOptions<AzureSettings>>().Value;

        var keyVaultUri = new Uri($"https://{keyVaultSettings.Name}.vault.azure.net/");

        // Try Managed Identity first (for Azure environments), fall back to client secret for local dev
        TokenCredential credential;
        
        if (azureSettings.HasClientSecretCredentials)
        {
            // Use Client Secret (for local development)
            Console.WriteLine("Using Client Secret for Key Vault authentication");
            credential = new ClientSecretCredential(
                azureSettings.TenantId!, 
                azureSettings.ClientId!, 
                azureSettings.ClientSecret!);
        }
        else
        {
            // Use Managed Identity (works in Azure App Service)
            Console.WriteLine("Using Managed Identity for Key Vault authentication");
            credential = new DefaultAzureCredential();
        }

        try
        {
            builder.Configuration.AddAzureKeyVault(keyVaultUri, credential, new MorsleyUkKeyVaultSecretManager());
            Console.WriteLine($"Successfully configured Azure Key Vault: {keyVaultUri}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to configure Azure Key Vault: {ex.Message}");
            throw;
        }

        return builder;
    }
}