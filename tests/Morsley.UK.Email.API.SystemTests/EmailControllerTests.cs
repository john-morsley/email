namespace Morsley.UK.Email.API.SystemTests;

[TestFixture]
public class EmailApiTests : EmailApiTestsBase
{
    [Test]
    // Given: We have a valid email
    //  When: That email is sent with the API /api/send (POST)
    //  Then: The email should be sent successfully
    //   And: There should be a copy of the sent email in the database
    public async Task Send_Email()
    {
        // Arrange
        var to = new List<string> { TestSettings.TestEmailAddress };
        var fullDateTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fffffff");
        var subject = $"Morsley.UK.Email.SystemTests - {fullDateTime}";
        var body = $"This is a test email from Morsley.UK.Email.SystemTests sent on {fullDateTime}";
        
        var sendableEmail = new SendableEmailMessage
        {
            To = to,
            Subject = subject,
            TextBody = body
        };

        var json = JsonSerializer.Serialize(sendableEmail, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        Console.WriteLine("Act 1... POST /api/email");

        // Act
        var sendResponse = await Client.PostAsync("/api/email", content);

        // Assert
        sendResponse.ShouldNotBeNull();
        sendResponse.EnsureSuccessStatusCode();
        sendResponse.StatusCode.ShouldBe(System.Net.HttpStatusCode.Created);

        var sendResponseContent = await sendResponse.Content.ReadAsStringAsync();
        Console.WriteLine($"POST /api/email response: {sendResponseContent}");
        var sendResponseObject = JsonSerializer.Deserialize<JsonElement>(sendResponseContent);

        sendResponseObject.TryGetProperty("id", out var sendIdProperty).ShouldBeTrue();
        var sentEmailId = sendIdProperty.GetString();
        sentEmailId.ShouldNotBeNullOrEmpty();

        // Verify Location header is set
        sendResponse.Headers.Location.ShouldNotBeNull();
        sendResponse.Headers.Location.ToString().ShouldContain($"api/email/{sentEmailId}");

        // Verify the email was persisted in the database
        await VerifySentEmailPersistedInDatabase(sentEmailId, to, subject, body);
    }

    [Test]
    // Given: We have a valid email
    //  When: That email is sent with the API /api/send (POST)
    //   And: We get that email with the API /api/get-all (GET)
    //  Then: The email should send successfully
    //   And: The email should be read successfully
    //   And: There should be a copy of the sent email in the database
    //   And: There should be a copy of the received email in the database
    //  Note: I understand we are doing 2 things here and not the usual 1,
    //        but it's the best way I can see to get a complete round trip tested.  
    public async Task Send_And_Read_Email()
    {
        // Send the email...

        // Arrange
        var to = new List<string> { TestSettings.TestEmailAddress };
        var fullDateTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fffffff");
        var subject = $"Morsley.UK.Email.SystemTests - {fullDateTime}";
        var body = $"This is a test email from integration tests. ({fullDateTime})";

        var sendableEmail = new SendableEmailMessage
        {
            To = to,
            Subject = subject,
            TextBody = body
        };

        var json = JsonSerializer.Serialize(sendableEmail, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        Console.WriteLine("Act 1... POST /api/email");

        // Act 1
        var sendResponse = await Client.PostAsync("/api/email", content);

        // Assert 1
        sendResponse.ShouldNotBeNull();
        sendResponse.EnsureSuccessStatusCode();
        sendResponse.StatusCode.ShouldBe(System.Net.HttpStatusCode.Created);

        var sendResponseContent = await sendResponse.Content.ReadAsStringAsync();
        Console.WriteLine($"POST /api/email response: {sendResponseContent}");
        var sendResponseObject = JsonSerializer.Deserialize<JsonElement>(sendResponseContent);

        sendResponseObject.TryGetProperty("id", out var sendIdProperty).ShouldBeTrue();
        var sentEmailId = sendIdProperty.GetString();
        sentEmailId.ShouldNotBeNullOrEmpty();
        
        // Verify Location header is set
        sendResponse.Headers.Location.ShouldNotBeNull();
        sendResponse.Headers.Location.ToString().ShouldContain($"api/email/{sentEmailId}");

        // Verify the email was persisted in the database
        await VerifySentEmailPersistedInDatabase(sentEmailId, to, subject, body);

        // Retreive the email...

        // As there might be a delay between sending and retrieving, the following process might need some retries...

        const int maximumNumberOfRetries = 10;
        var numberOfRetries = 0;

        EmailMessage? foundEmail = null;

        Console.WriteLine("Act 2... GET /api/emails");
        do
        {
            numberOfRetries++;
            Console.WriteLine($"Attempt: {numberOfRetries}");

            // Act 2...            
            var retrieveResponse = await Client.GetAsync("/api/emails");

            // Assert 2
            retrieveResponse.ShouldNotBeNull();
            retrieveResponse.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);

            var retrieveResponseContent = await retrieveResponse.Content.ReadAsStringAsync();
            Console.WriteLine($"GET /api/emails response: {retrieveResponseContent}");

            var pageOfEmails = JsonSerializer.Deserialize<PaginatedResponse<Common.Models.EmailMessage>>(retrieveResponseContent, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            if (pageOfEmails is not null)
            {
                pageOfEmails.Count.ShouldBeGreaterThanOrEqualTo(0);
                pageOfEmails.Page.ShouldBe(1);
                pageOfEmails.PageSize.ShouldBe(20);
                pageOfEmails.TotalItems.ShouldBeGreaterThanOrEqualTo(0);
                pageOfEmails.TotalPages.ShouldBeGreaterThanOrEqualTo(1);
                pageOfEmails.HasPrevious.ShouldBeFalse();

                // We need a mechanism for getting other pages...
                //for (int pageNumber = 0; pageNumber < pageOfEmails.TotalPages; pageNumber++)
                //{
                    // ToDo
                //}

                foreach (var receivedEmail in pageOfEmails.Items)
                {
                    if (receivedEmail.Subject == subject &&
                        receivedEmail.TextBody!.Contains(body))
                    {
                        foundEmail = receivedEmail;
                        numberOfRetries = maximumNumberOfRetries;
                    }
                }
            }

            if (foundEmail is null) await Task.Delay(1000); // Wait for a second before trying again.

        } while (numberOfRetries < maximumNumberOfRetries);

        if (foundEmail is null)
        {
            Assert.Fail("Could not match the sent email");
        }
        else
        {
            await VerifyReceivedEmailPersistedInDatabase(foundEmail.Id, to, subject, body);
            Assert.Pass("Matched the sent email");
        }
    }

    //[Test]
    //public async Task SendEmail_ShouldReturnBadRequest_WhenInvalidEmailProvided()
    //{
    //    // Arrange - Missing required fields
    //    var invalidEmail = new SendableEmailMessage
    //    {
    //        To = new List<string>(), // Empty To list should be invalid
    //        Subject = "", // Empty subject should be invalid
    //        TextBody = "Test body"
    //    };

    //    var json = JsonSerializer.Serialize(invalidEmail, new JsonSerializerOptions
    //    {
    //        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    //    });
    //    var content = new StringContent(json, Encoding.UTF8, "application/json");

    //    // Act
    //    var response = await _client.PostAsync("/api/email", content);

    //    // Assert
    //    Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.BadRequest));
    //}

    private async Task VerifySentEmailPersistedInDatabase(string emailId, List<string> to, string subject, string body)
    {
        ArgumentNullException.ThrowIfNull(emailId);

        to.ShouldNotBeNull();
        to.Any().ShouldBeTrue();        
        subject.ShouldNotBeNullOrEmpty();
        body.ShouldNotBeNullOrEmpty();
        emailId.ShouldNotBeNull();

        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.Test.json")
            .AddUserSecrets<EmailApiTests>()
            .Build();

        var databaseId = configuration.GetValue<string>("CosmosDb:DatabaseName");
        var containerId = configuration.GetValue<string>("CosmosDb:SentEmailsContainerName");

        var persistedSentEmail = await CosmosFixture.GetItemAsync<EmailMessage>(databaseId, containerId, emailId, emailId);
               
        // Verify the email was found
        persistedSentEmail.ShouldNotBeNull("Email should be persisted in the sent database");

        // Verify email content matches what was sent
        persistedSentEmail.To.ShouldBe(to, "To recipients did not match");
        persistedSentEmail.Subject.ShouldBe(subject, "Subject did not match");
        persistedSentEmail.TextBody.ShouldBe(body, "Text body not match");        
        
        // Verify the email has a sent timestamp
        persistedSentEmail.SentAt.ShouldNotBeNull("Email should have a SentAt timestamp");

        // Verify the email has a batch number
        persistedSentEmail.BatchNumber.ShouldBeNull("Email should NOT have a BatchNumber");
    }

    private async Task VerifyReceivedEmailPersistedInDatabase(string emailId, List<string> to, string subject, string body)
    {
        ArgumentNullException.ThrowIfNull(emailId);        

        to.ShouldNotBeNull();
        to.Any().ShouldBeTrue();
        subject.ShouldNotBeNullOrEmpty();
        body.ShouldNotBeNullOrEmpty();

        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.Test.json")
            .AddUserSecrets<EmailApiTests>()
            .Build();

        var databaseId = configuration.GetValue<string>("CosmosDb:DatabaseName");
        var containerId = configuration.GetValue<string>("CosmosDb:ReceivedEmailsContainerName");

        var persistedReceivedEmail = await CosmosFixture.GetItemAsync<EmailMessage>(databaseId, containerId, emailId, emailId);

        // Verify the email was found
        persistedReceivedEmail.ShouldNotBeNull("Email should be persisted in the sent database");

        // Verify email content matches what was sent
        persistedReceivedEmail.To.ShouldBe(to, "To recipients did not match");
        persistedReceivedEmail.Subject.ShouldBe(subject, "Subject did not match");
        persistedReceivedEmail.TextBody.Trim('\r', '\n').ShouldBe(body, "Text body not match");

        // Verify the email has a sent timestamp
        persistedReceivedEmail.SentAt.ShouldNotBeNull("Email should have a SentAt timestamp");

        // Verify the email has a batch number
        persistedReceivedEmail.BatchNumber.ShouldNotBeNull("Email should have a BatchNumber");
    }
}