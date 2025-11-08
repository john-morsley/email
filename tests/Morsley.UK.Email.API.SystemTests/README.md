Morsley.UK.Email.API.SystemTests
================================

The tests here test the complete API endpoints:

- Sending/recieving emails via the email servers IMAP/POP3 capabilites.
- Storing of these emails in an Azure Cosmos database.

If debugging, and you want to look at the Azure Cosmos DB Emulator from Microsoft UI screen, open Docker.
Locate the container, and note its port number. It be mapping port number 8081.
Then visit the following URL, replacing the mapped port number:

https://localhost:[Port Number]/_explorer/index.html

Sequence Diagram
----------------

```mermaid
sequenceDiagram
    participant Test as System Test
    participant Docker as Docker
    participant CosmosDB as Cosmos DB Emulator<br/>(within Docker Container)
    participant API as Morsley.UK.Email.API<br/>(System Under Test)
    
    Note over Test: OneTimeSetUp
    Test->>Test: Create Cosmos Emulator Fixture
    
    Test->>Docker: Create & Start Container
    Docker-->>Test: Container Started
    
    Test->>CosmosDB: Start Cosmos DB Emulator
    CosmosDB-->>Test: Emulator Running
    
    Test->>CosmosDB: Create Database
    
    Note over Test: SetUp (Before Each Test)
    Test->>CosmosDB: Remove (if necessary) & Create Containers
    Test->>API: Create WebApplicationFactory
    Test->>Test: Create HttpClient

    Note over Test: Test 1: Send_Email
    Test->>API: POST /api/email<br/>(Send Email)
    Note over API: Send via SMTP
    API->>CosmosDB: Persist to Sent Container
    CosmosDB-->>API: Email Persisted
    API-->>Test: 201 Created (with ID & Location)
    Test->>CosmosDB: Verify Email in Sent Container
    CosmosDB-->>Test: Email Retrieved
    Test->>Test: Assert Email Content as Expected

    Note over Test: Test 2: Send_And_Read_Email
    Test->>API: POST /api/email<br/>(Send Email)
    Note over API: Send via SMTP
    API->>CosmosDB: Persist to Sent Container
    CosmosDB-->>API: Email Persisted
    API-->>Test: 201 Created (with ID & Location)
    Test->>CosmosDB: Verify Email in Sent Container
    CosmosDB-->>Test: Sent Email Retrieved
    Test->>Test: Assert Email Content as Expected
    
    Note over Test: Wait & Retry Loop (up to 10 attempts)
    loop Retry until found or max attempts
        Test->>API: GET /api/emails<br/>(Retrieve page of Emails)
        Note over API: Fetch via IMAP
        API->>CosmosDB: Persist to Inbox Container
        CosmosDB-->>API: Emails Persisted
        API-->>Test: 200 OK (Paginated Response)
        Test->>Test: Search for Matching Email<br/>(by Subject & Body)
        alt Email Not Found
            Test->>Test: Wait 1 second
        else Email Found
            Test->>Test: Exit Loop
        end
    end
    
    Test->>CosmosDB: Verify Email in Inbox Container
    CosmosDB-->>Test: Received Email Retrieved
    Test->>Test: Assert Email Content Matches

    Note over Test: TearDown (After Each Test)
    Test->>Test: Dispose HttpClient & Factory

    Note over Test: OneTimeTearDown
    Test->>CosmosDB: Remove Database
    CosmosDB-->>Test: Database Removed
    Test->>CosmosDB: Stop Cosmos DB Emulator
    CosmosDB-->>Test: Emulator Stopped
    Test->>Docker: Destroy Container
    Docker-->>Test: Container Destroyed
```