namespace Morsley.UK.Email.Secrets;

public class MorsleyUkKeyVaultSecretManager : KeyVaultSecretManager
{
    private const string Prefix = "MorsleyUk--";

    public override bool Load(SecretProperties properties)
    {
        // Only load secrets that start with our prefix
        return properties.Name.StartsWith(Prefix);
    }

    public override string GetKey(KeyVaultSecret secret)
    {
        // Remove the prefix and replace -- with : for hierarchical keys
        return secret.Name
            .Substring(Prefix.Length)
            .Replace("--", ConfigurationPath.KeyDelimiter);
    }
}

public static class ServiceCollectionExtensions
{
    public static WebApplicationBuilder ConfigureAzureKeyVault(this WebApplicationBuilder builder)
    {
        var configuration = builder.Configuration;
        var keyVaultName = configuration["KeyVault:Name"];

        if (string.IsNullOrEmpty(keyVaultName)) throw new InvalidOperationException("KeyVault:Name is not configured");

        var keyVaultUri = new Uri($"https://{keyVaultName}.vault.azure.net/");

        // Try Managed Identity first (for Azure environments), fall back to client secret for local dev
        TokenCredential credential;
        var clientSecret = configuration["Azure:ClientSecret"];
        
        if (string.IsNullOrEmpty(clientSecret))
        {
            // Use Managed Identity (works in Azure App Service)
            Console.WriteLine("Using Managed Identity for Key Vault authentication");
            credential = new DefaultAzureCredential();
        }
        else
        {
            // Use Client Secret (for local development)
            var clientId = configuration["Azure:ClientId"];
            var tenantId = configuration["Azure:TenantId"];
            
            if (string.IsNullOrEmpty(clientId)) throw new InvalidOperationException("Azure:ClientId is not configured");
            if (string.IsNullOrEmpty(tenantId)) throw new InvalidOperationException("Azure:TenantId is not configured");
            
            Console.WriteLine("Using Client Secret for Key Vault authentication");
            credential = new ClientSecretCredential(tenantId, clientId, clientSecret);
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