using Azure.Identity;
using Microsoft.Extensions.Azure;

var builder = WebApplication.CreateBuilder(args);

// Configure Key Vault for all environments with detailed error handling
var keyVaultUri = builder.Configuration["KeyVault:VaultUri"];
if (!string.IsNullOrEmpty(keyVaultUri))
{
    try
    {
        Console.WriteLine($"Attempting to connect to Key Vault: {keyVaultUri}");
        
        var credential = new DefaultAzureCredential(new DefaultAzureCredentialOptions
        {
            Diagnostics = { IsLoggingEnabled = true },
            ExcludeInteractiveBrowserCredential = true,
            ExcludeAzureCliCredential = false, // Keep Azure CLI enabled
            ExcludeVisualStudioCredential = true,
            ExcludeVisualStudioCodeCredential = true,
            ExcludeAzurePowerShellCredential = true,
            ExcludeManagedIdentityCredential = true,
            ExcludeEnvironmentCredential = true, // Exclude environment variables
            AdditionallyAllowedTenants = { "*" } // Allow all tenants
        });
        
        builder.Configuration.AddAzureKeyVault(
            new Uri(keyVaultUri),
            credential);
            
        Console.WriteLine("Key Vault configuration added successfully");
        
        // Test retrieving a value from Key Vault
        var testSecret = builder.Configuration["morsley-uk-test-secret"];
        if (!string.IsNullOrEmpty(testSecret))
        {
            Console.WriteLine("Successfully retrieved Key Vault secret: morsley-uk-test-secret");
        }
        else
        {
            Console.WriteLine("Could not retrieve Key Vault secret: morsley-uk-test-secret");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Key Vault error: {ex.Message}");
        Console.WriteLine($"Exception type: {ex.GetType().Name}");
        if (ex.InnerException != null)
        {
            Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
        }
        Console.WriteLine("Continuing without Key Vault...");
    }
}

builder.Services.AddControllersWithViews();

builder.Services.AddEmailReader(builder.Configuration);
builder.Services.AddEmailSender(builder.Configuration);
builder.Services.AddEmailPersistence(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Morsley UK Email API",
        Version = "v1",
        Description = "API for sending (SMTP) and reading (IMAP) emails."
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    await scope.ServiceProvider.InitializeCosmosDbAsync(throwOnError: false);
}

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Morsley UK Email API");
});

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

// Make the implicit Program class public for integration testing
public partial class Program { }