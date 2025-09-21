var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddUserSecrets<Program>();

builder.ConfigureAzureKeyVault();

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

public partial class Program { }