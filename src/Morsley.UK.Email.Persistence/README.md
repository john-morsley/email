# Persistence

Azure Cosmos DB

Configuration expectations:

```json
{
  "CosmosDb": {
    "Endpoint": "https://localhost:8081",
    "Key": "[Stored in User Secrets]",
    "DatabaseName": "morsley-uk-db",
    "SentContainerName": "emails-sent",
    "ReceivedContainerName": "emails-inbox"
  },
  "morsley-uk-cosmos-db-primary-read-write-key": "[This value will come from the Azure KeyVault]",
  "morsley-uk-cosmos-db-secondary-read-write-key": "[This value will come from the Azure KeyVault]",
  "morsley-uk-cosmos-db-primary-read-key": "[This value will come from the Azure KeyVault]",
  "morsley-uk-cosmos-db-secondary-read-key": "[This value will come from the Azure KeyVault]",
}
```

## Local Development

During local development the Azure Cosmos DB Emulator from Microosft was used.
Once this application is running, it'll open open an administration web page on:

https://localhost:8081/_explorer/index.html

From here you can take the URI and Primary Key, and put them into the API projects User Secrets file.

### Remote Database

URI: Azure Cosmos DB Account -> Settings -> Keys -> URI  
Primary Key: Azure Cosmos DB Account -> Settings -> Keys  

Here you may select either primary or secondry key, but for this project it must be a read-write key.