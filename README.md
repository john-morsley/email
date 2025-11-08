# Email

A simple library to handle emails

---

## Database (ComsosDB)

### Local Development

To run the database locally, you will need to have Azure Cosmos DB Emulator installed.

The database UI will be available at `https://localhost:8081/_explorer/index.html`

You can either ignore the security warning, or you can download the certificate from the UI and install it in your browser.

---

## User Secrets

```json
{
  "ImapSettings": {
    "Username": "",
    "Password": ""
  },
  "SmtpSettings": {
    "Username": "",
    "Password": "",
    "FromAddress": ""
  },
  "Data": {
    "ToAddress": ""
  },
  "CosmosDb": {
    "Endpoint": "https://localhost:8081",
    "Key": "",
    "DatabaseName": "morsley-uk-db"
  }
}
```

---

## Azure Key Vault

### Local Development

Download and install Azure CLI: https://learn.microsoft.com/en-us/cli/azure/?view=azure-cli-latest

Once installed, try

```PowerShell
az login
```

Adding these:

AZURE_CLIENT_ID=your-client-id
AZURE_CLIENT_SECRET=your-client-secret  
AZURE_TENANT_ID=your-tenant-id

---

## Pipleines

CI
--

When added to Azure DevOps, should be renamed 'Morsley-UK.Email.CI'.

CD
--

When added to Azure DevOps, should be renamed 'Morsley-UK.Email.CD'.

It requires the details of the Azure Subscription to be added to Azure DevOps.

For the deployment task to work, there needs to be an Azure Web App ready.

It also requires the details of the Azure Subscription to be added to Azure DevOps: morsley-uk-email

Azure DevOps --> email (Project) --> Project settings --> Service connections --> New service connection --> Azure Resource Manager --> Next --> [This will require a log in to Azure] --> [Select the details for the Web App] --> Call it morsley-uk-email

Required Azure Infrastructure:

- Subscription: Morsley UK
- Resource Group: morsley-uk-rg
- App Service Plan --> morsley-uk-asp
- Web App --> morsley-uk-email-api

