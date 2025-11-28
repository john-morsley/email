namespace Morsley.UK.Email.Secrets;

public class AzureSettings
{
    public string? ClientId { get; set; }

    public string? TenantId { get; set; }

    public string? ClientSecret { get; set; }

    public string? ManagedIdentityClientId { get; set; }

    public bool HasClientSecretCredentials =>
        !string.IsNullOrEmpty(ClientId) &&
        !string.IsNullOrEmpty(TenantId) &&
        !string.IsNullOrEmpty(ClientSecret) &&
        ClientSecret != "[In User Secrets]";
}
