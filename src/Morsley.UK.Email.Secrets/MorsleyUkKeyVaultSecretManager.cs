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