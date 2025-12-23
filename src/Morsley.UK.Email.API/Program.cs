var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.AddAzureWebAppDiagnostics();

builder.Configuration.AddUserSecrets<Program>();
builder.ConfigureAzureKeyVault();

builder
    .Services
        .AddOptions<SmtpSettings>()
        .Bind(builder.Configuration.GetSection("SmtpSettings"))
        .ValidateOnStart();

builder.Services.AddSingleton<IValidateOptions<ImapSettings>, ImapSettingsValidator>();
builder.Services.AddSingleton<IValidateOptions<SmtpSettings>, SmtpSettingsValidator>();

builder.Services.AddControllersWithViews();

builder.Services.AddEmailReader(builder.Configuration);
builder.Services.AddEmailSender(builder.Configuration);
builder.Services.AddEmailPersistence(builder.Configuration);

var startupHealthCheck = new StartupHealthCheck();
builder.Services.AddSingleton(startupHealthCheck);
builder.Services.AddHealthChecks()
    .AddCheck("startup", () => startupHealthCheck.CheckHealthAsync(new HealthCheckContext()).Result)
    .AddCheck<CosmosDbHealthCheck>("cosmosdb", tags: new[] { "ready", "db" });

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

var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Application starting up - Morsley UK Email API");

using (var scope = app.Services.CreateScope())
{
    await scope.ServiceProvider.InitializeCosmosDbAsync(throwOnError: false);
}

startupHealthCheck.MarkStartupComplete();
logger.LogInformation("Application started successfully");

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

app.MapControllers();

app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = check => check.Name == "startup",
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.Run();

public partial class Program { }