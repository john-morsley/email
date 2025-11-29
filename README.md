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

It also requires the details of the Azure Subscription to be added to Azure DevOps: morsley-uk-email-sc (for Service Connection)

Azure DevOps --> email (Project) --> Project settings --> Service connections --> New service connection --> Azure Resource Manager --> Next --> [This will require a log in to Azure] --> [Select the details for the Web App] --> Call it morsley-uk-email-sc

Required Azure Infrastructure:

- Subscription: Morsley UK
- Resource Group: morsley-uk-rg
- App Service Plan --> morsley-uk-asp
- Web App --> morsley-uk-email-api

Important!

The App Service must be grant access to the Key Vault.

- Go to the App Service, morsley-uk-email.api
- Settings --> Identity --> System assigned --> On --> Save
- Azure will create a managed identity for the app.
- Copy the Ibject (principal) ID --> b3e19e98-1044-4501-a100-90181ea90de4
- Go to the Key Vault, morsley-uk-key-vault
- Access Control (IAM)
- Add --> Add Role Assignment
- Search: Key Vault Secrets User
- Select that role from the list
- Click Next
- Managed Identity
- Members --> Select the 'morsley-uk-email-api' App Service --> Select
- Note the Object ID should match the one generated above
- Review + assign