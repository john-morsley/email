using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Morsley.UK.Email.API.Extensions;
using Morsley.UK.Email.API.Models;
using Morsley.UK.Email.Common.Models;
using Morsley.UK.Email.Persistence;
using Morsley.UK.Email.Persistence.Extensions;
using Shouldly;

namespace Morsley.UK.Email.IntegrationTests;

[TestFixture]
public class EmailControllerIntegrationTests
{
    private EmailApiWebApplicationFactory _factory = null!;
    private HttpClient _client = null!;
    private TestSettings _testSettings = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _factory = new EmailApiWebApplicationFactory();
        await _factory.InitializeAsync();
        _client = _factory.CreateClient();
        
        // Load test settings from configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.Test.json")
            .AddUserSecrets<EmailControllerIntegrationTests>()
            .Build();
            
        _testSettings = new TestSettings();
        configuration.GetSection("TestSettings").Bind(_testSettings);
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        _client?.Dispose();
        if (_factory != null)
        {
            await _factory.CleanupAsync();
            _factory.Dispose();
        }
    }

    [Test]
    // Given: We have a valid email
    //  When: That email is sent with the API /api/send (POST)
    //  Then: The email should be sent successfully
    //   And: There should be a copy of the sent email in the database
    public async Task Send_Email()
    {
        // Arrange
        var fullDateTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fffffff");
        var subject = $"Morsley.UK.Email.IntegrationTests Email - {fullDateTime}";
        var body = $"This is a test email from integration tests. ({fullDateTime})";

        var sendableEmail = new SendableEmailMessage
        {
            To = new List<string> { _testSettings.TestEmailAddress },
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
        var sendResponse = await _client.PostAsync("/api/email", content);

        // Assert
        sendResponse.ShouldNotBeNull();
        sendResponse.EnsureSuccessStatusCode();
        sendResponse.StatusCode.ShouldBe(System.Net.HttpStatusCode.Created);

        var sendResponseContent = await sendResponse.Content.ReadAsStringAsync();
        Console.WriteLine($"POST /api/email response: {sendResponseContent}");
        var sendResponseObject = JsonSerializer.Deserialize<JsonElement>(sendResponseContent);

        sendResponseObject.TryGetProperty("id", out var sendIdProperty).ShouldBeTrue();
        var sendEmailId = sendIdProperty.GetString();
        sendEmailId.ShouldNotBeNullOrEmpty();

        // Verify Location header is set
        sendResponse.Headers.Location.ShouldNotBeNull();
        sendResponse.Headers.Location.ToString().ShouldContain("api/email/");

        // Verify the email was persisted in the database
        var sentEmail = sendableEmail.ToEmailMessage();
        sentEmail.Id = sendEmailId;
        await VerifySentEmailPersistedInDatabase(sentEmail, subject, body);
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
        var fullDateTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fffffff");
        var subject = $"Morsley.UK.Email.IntegrationTests Email - {fullDateTime}";
        var body = $"This is a test email from integration tests. ({fullDateTime})";

        var sendableEmail = new SendableEmailMessage
        {
            To = new List<string> { _testSettings.TestEmailAddress },
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
        var sendResponse = await _client.PostAsync("/api/email", content);

        // Assert 1
        sendResponse.ShouldNotBeNull();
        sendResponse.EnsureSuccessStatusCode();
        sendResponse.StatusCode.ShouldBe(System.Net.HttpStatusCode.Created);

        var sendResponseContent = await sendResponse.Content.ReadAsStringAsync();
        Console.WriteLine($"POST /api/email response: {sendResponseContent}");
        var sendResponseObject = JsonSerializer.Deserialize<JsonElement>(sendResponseContent);

        sendResponseObject.TryGetProperty("id", out var sendIdProperty).ShouldBeTrue();
        var sendEmailId = sendIdProperty.GetString();
        sendEmailId.ShouldNotBeNullOrEmpty();
        
        // Verify Location header is set
        sendResponse.Headers.Location.ShouldNotBeNull();
        sendResponse.Headers.Location.ToString().ShouldContain("api/email/");

        // Verify the email was persisted in the database
        var sentEmail = sendableEmail.ToEmailMessage();
        sentEmail.Id = sendEmailId;
        await VerifySentEmailPersistedInDatabase(sentEmail, subject, body);

        // Retreive the email...

        // As there might be a delay between sending and retrieving, the following process might need some retries...

        const int maximumNumberOfRetries = 10;
        var numberOfRetries = 0;

        Common.Models.EmailMessage? foundEmail = null;

        Console.WriteLine("Act 2... GET /api/email/all");
        do
        {
            numberOfRetries++;
            Console.WriteLine($"Attempt: {numberOfRetries}");

            // Act 2...            
            var retrieveResponse = await _client.GetAsync("/api/email/all");

            // Assert 2
            retrieveResponse.ShouldNotBeNull();
            retrieveResponse.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);

            var retrieveResponseContent = await retrieveResponse.Content.ReadAsStringAsync();
            Console.WriteLine($"GET /api/email/all response: {retrieveResponseContent}");

            var receivedEmails = JsonSerializer.Deserialize<List<Common.Models.EmailMessage>>(retrieveResponseContent, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            if (receivedEmails is not null && receivedEmails.Count != 0)
            {
                foreach (var receivedEmail in receivedEmails)
                {
                    if (receivedEmail.Subject == subject &&
                        receivedEmail.TextBody!.Contains(body))
                    {
                        foundEmail = receivedEmail;
                        numberOfRetries = maximumNumberOfRetries;
                    }
                }
            }

            await Task.Delay(1000); // Wait for a second before trying again.

        } while (numberOfRetries < maximumNumberOfRetries);

        if (foundEmail is null)
        {
            Assert.Fail("Could not match the sent email");
        }
        else
        {
            await VerifyReceivedEmailPersistedInDatabase(foundEmail, subject, body);
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

    private async Task VerifySentEmailPersistedInDatabase(EmailMessage email, string subject, string body)
    {
        ArgumentNullException.ThrowIfNull(email);
        if (email.Id is null) { throw new ArgumentNullException("email.Id"); }

        email.ShouldNotBeNull();
        subject.ShouldNotBeNullOrEmpty();
        body.ShouldNotBeNullOrEmpty();

        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.Test.json")
            .AddUserSecrets<EmailControllerIntegrationTests>()
            .Build();
        
        services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Information));
        services.AddEmailPersistence(configuration);
        
        var serviceProvider = services.BuildServiceProvider();
        var persistenceService = serviceProvider.GetRequiredService<ISentEmailPersistenceService>();
        
        // Retrieve the email from the database
        var persistedEmail = await persistenceService.GetEmailAsync(email.Id);
        
        // Verify the email was found
        persistedEmail.ShouldNotBeNull("Email should be persisted in the sent database");
        
        // Verify email content matches what was sent
        persistedEmail.Subject.ShouldBe(email.Subject, "Subject did not match");
        persistedEmail.TextBody.ShouldBe(email.TextBody, "Text body not match");
        persistedEmail.To.ShouldBe(email.To, "To recipients did not match");
        
        // Verify the email has a sent timestamp
        persistedEmail.SentAt.ShouldNotBeNull("Email should have a SentAt timestamp");
    }

    private async Task VerifyReceivedEmailPersistedInDatabase(EmailMessage email, string subject, string body)
    {
        ArgumentNullException.ThrowIfNull(email);
        if (email.Id is null) { throw new ArgumentNullException("email.Id"); }

        email.ShouldNotBeNull();
        subject.ShouldNotBeNullOrEmpty();
        body.ShouldNotBeNullOrEmpty();

        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.Test.json")
            .AddUserSecrets<EmailControllerIntegrationTests>()
            .Build();

        services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Information));
        services.AddEmailPersistence(configuration);

        var serviceProvider = services.BuildServiceProvider();
        var persistenceService = serviceProvider.GetRequiredService<IReceivedEmailPersistenceService>();

        // Retrieve the email from the database
        var persistedEmail = await persistenceService.GetEmailAsync(email.Id);

        // Verify the email was found
        persistedEmail.ShouldNotBeNull("Email should be persisted in the received database");

        // Verify email content matches what was sent
        persistedEmail.Subject.ShouldBe(email.Subject, "Subject did not match");
        persistedEmail.TextBody.ShouldBe(email.TextBody, "Text body not match");
        persistedEmail.To.ShouldBe(email.To, "To recipients did not match");

        // Verify the email has a sent timestamp
        persistedEmail.BatchNumber.ShouldNotBeNull("Email should have a BatchNumber");
    }
}