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
    //   And: We get that email with the API /api/get-all (GET)
    //  Then: The email should send successfully
    //   And: The email should be read successfully
    //   And: There should be a copy of the sent email in the database
    //   And: There should be a copy of the recieved email in the database
    //  Note: I understand we are doing 2 things here and not the usual 1,
    //        but it's the best way I can see to get a complete round trip tested.  
    public async Task Send_And_Get_Email()
    {
        // Arrange
        var fullDateTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fffffff");
        var sendableEmail = new SendableEmailMessage
        {
            To = new List<string> { _testSettings.TestEmailAddress },
            Subject = $"Integration Test Email - {fullDateTime}",
            TextBody = $"This is a test email from integration tests. ({fullDateTime})",
        };

        var json = JsonSerializer.Serialize(sendableEmail, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/email", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var responseObject = JsonSerializer.Deserialize<JsonElement>(responseContent);
        
        Assert.That(responseObject.TryGetProperty("id", out var idProperty), Is.True);
        Assert.That(idProperty.GetString(), Is.Not.Null.And.Not.Empty);
        
        // Verify Location header is set
        Assert.That(response.Headers.Location, Is.Not.Null);
        Assert.That(response.Headers.Location.ToString(), Does.Contain("api/email/"));

        // Verify the email was persisted in the database
        var emailId = idProperty.GetString()!;
        await VerifyEmailPersistedInDatabase(emailId, sendableEmail, fullDateTime);
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
}