namespace Morsley.UK.Email.Secrets;

public static class ServiceCollectionExtensions
{
    public static WebApplicationBuilder ConfigureAzureKeyVault(this WebApplicationBuilder builder)
    {
        var configuration = builder.Configuration;

        var clientId = configuration["Azure:ClientId"];
        var tenantId = configuration["Azure:TenantId"];
        var clientSecret = configuration["Azure:ClientSecret"];

        var keyVaultName = configuration["KeyVault:Name"];

        if (string.IsNullOrEmpty(clientId)) throw new InvalidOperationException("Azure:ClientId is not configured");
        if (string.IsNullOrEmpty(tenantId)) throw new InvalidOperationException("Azure:TenantId is not configured");
        if (string.IsNullOrEmpty(clientSecret)) throw new InvalidOperationException("Azure:ClientSecret is not configured");
        if (string.IsNullOrEmpty(keyVaultName)) throw new InvalidOperationException("KeyVault:Name is not configured");

        var keyVaultUri = new Uri($"https://{keyVaultName}.vault.azure.net/");

        var credential = new ClientSecretCredential(
            tenantId,
            clientId,
            clientSecret
        );

        try
        {
            builder.Configuration.AddAzureKeyVault(keyVaultUri, credential);
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