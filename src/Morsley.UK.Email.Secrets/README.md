# Secrets

Azure Key Vault

Configuration expectations:

```json
{
  "Azure": {
    "TenantId": "0676ba93-d41f-4786-8c3f-0a683eaacaf7",
    "ClientId": "cfd17100-c23b-4ecb-8ee1-c4bd5c54e7ab",
    "ClientSecret": "[In User Secrets]"
  },
  "KeyVault": {
    "Name": "morsley-uk-key-vault"
  },
  "test-secret": "[This value will come from the Azure KeyVault]",
  "morsley-uk-cosmos-db-primary-read-write-key": "[This value will come from the Azure KeyVault]",
  "morsley-uk-cosmos-db-secondary-read-write-key": "[This value will come from the Azure KeyVault]",
  "morsley-uk-cosmos-db-primary-read-key": "[This value will come from the Azure KeyVault]",
  "morsley-uk-cosmos-db-secondary-read-key": "[This value will come from the Azure KeyVault]",
}
```