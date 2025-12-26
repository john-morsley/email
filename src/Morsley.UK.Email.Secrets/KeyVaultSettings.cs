namespace Morsley.UK.Email.Secrets;

public class KeyVaultSettings
{
    private string _name = string.Empty;

    public string Name
    {
        get
        {
            if (string.IsNullOrEmpty(_name)) throw new InvalidOperationException("KeyVault:Name cannot be empty.");
            return _name;
        }
        set
        {
            _name = value;
        }
    }
}