Morsley.UK.Email.API
====================

This project requires the following:

1. A Cosmos DB (or an emulator)
2. An email server (supporting SMPT and IMAP)

The settings for these need to be in:

1. appsettings.json
2. secrets.json

```JSON
{
  "ImapSettings": {
    "Server": "mail.livemail.co.uk",
    "Port": 993,
    "UseSsl": true,
    "Username": "[Stored in User Secrets]",
    "Password": "[Stored in User Secrets]"
  },
  "SmtpSettings": {
    "Server": "smtp.livemail.co.uk",
    "Port": 465,
    "UseSsl": true,
    "Username": "[Stored in User Secrets]",
    "Password": "[Stored in User Secrets]",
    "FromAddress": "[Stored in User Secrets]"
  },
  "Data": {
    "ToAddress": "[Stored in User Secrets]"
  },
  "CosmosDb": {
    "Endpoint": "https://localhost:8081",
    "PrimaryKey": "[Stored in User Secrets]",
    "DatabaseName": "morsley-uk-db",
    "SentContainerName": "emails-sent",
    "ReceivedContainerName": "emails-inbox"
  },
  "Azure": {
    "TenantId": "0676ba93-d41f-4786-8c3f-0a683eaacaf7",
    "ClientId": "cfd17100-c23b-4ecb-8ee1-c4bd5c54e7ab",
    "ClientSecret": "[In User Secrets]"
  },
  "KeyVault": {
    "Name": "morsley-uk-key-vault"
  },
  "test-secret": "[This value will come from the Azure KeyVault]",
  "ExpectedTestSecretValue": "Password!1",
  "morsley-uk-cosmos-db-primary-read-write-key": "[This value will come from the Azure KeyVault]",
  "morsley-uk-cosmos-db-secondary-read-write-key": "[This value will come from the Azure KeyVault]",
  "morsley-uk-cosmos-db-primary-read-key": "[This value will come from the Azure KeyVault]",
  "morsley-uk-cosmos-db-secondary-read-key": "[This value will come from the Azure KeyVault]",
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```