
const int NumberOfReadAttempts = 5;
const int NumberOfSecondsInbetweenAttempts = 3;

Console.WriteLine("Running Morsley.UK.Email.Console\n");

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((ctx, cfg) =>
    {
        cfg.AddJsonFile("AppSettings.Console.json", optional: false, reloadOnChange: true);
        cfg.AddUserSecrets<Program>();
        cfg.AddEnvironmentVariables();
    })
    .ConfigureServices((ctx, services) => {
        services.AddEmailSender(ctx.Configuration);
        services.AddEmailReader(ctx.Configuration);
    })
    .Build();

var unique = Guid.NewGuid();
var emailTo = host.Services.GetRequiredService<IConfiguration>()["Data:ToAddress"];
var emailSubject = $"Test - {unique}";
var emailBody = $"Unique: {unique}";

var sender = host.Services.GetRequiredService<IEmailSender>();

var message = new Morsley.UK.Email.Common.Models.EmailMessage 
{ 
    To = [ emailTo ], 
    Subject = emailSubject, 
    TextBody = emailBody 
};
var empty = new Morsley.UK.Email.Common.Models.EmailMessage 
{ 
    To = [emailTo], 
    Subject = $"Blank - {DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss:fff")}", 
    TextBody = "Nothing" 
};

Console.WriteLine("============================== SENDING ==============================");
try
{
    Console.WriteLine("Sending email...");

    await sender.SendAsync(empty);
    await sender.SendAsync(empty);
        
    Console.WriteLine($"To: {emailTo}");
    Console.WriteLine($"Subject: {emailSubject}");
    Console.WriteLine($"Body: {emailBody}");

    await sender.SendAsync(message);

    await sender.SendAsync(empty);
    await sender.SendAsync(empty);

    Console.WriteLine("Successfully sent");
}
catch (Exception)
{
    Console.WriteLine("Sending failed unexpectedly!");
}
Console.WriteLine("============================== SENDING ==============================\n");

var reader = host.Services.GetRequiredService<IEmailReader>();

var emailFound = false;
var numberOfAttempts = 0;

Console.WriteLine("============================== READING ==============================");
do
{
    numberOfAttempts++;
    Console.WriteLine($"Attempt number {numberOfAttempts} of {NumberOfReadAttempts}");

    Console.Write($"Waiting for {NumberOfSecondsInbetweenAttempts} seconds");
    for (int i = 0; i < NumberOfSecondsInbetweenAttempts; i++)
    {
        Console.Write(".");
        await Task.Delay(TimeSpan.FromSeconds(1));
    }
    Console.WriteLine("");
    Console.WriteLine("Done waiting!");

    emailFound = await ReadEmails();

    if (emailFound) numberOfAttempts = NumberOfReadAttempts;
} 
while (numberOfAttempts < NumberOfReadAttempts);
Console.WriteLine("============================== READING ==============================\n");

async Task<bool> ReadEmails()
{
    var found = false;

    try
    {
        Console.WriteLine("Reading email(s)...");
        var emails = await reader.FetchAsync();
        Console.WriteLine("Successfully read");

        var count = 1;

        foreach (var email in emails)
        {
            var banner = new string('-', 15) + $" Email {count} " + new string('-', 15);
            Console.WriteLine(banner);

            Console.WriteLine($"Number {count++}:");
            Console.WriteLine($"Subject: {email.Subject}");
            var textBody = email.TextBody.TrimEnd('\n', '\r');
            Console.WriteLine($"Body (Text): {textBody}");
            if (email.Subject == emailSubject &&  textBody == emailBody)
            {
                Console.WriteLine(">>>>>>>>>> This is the email we're looking for! <<<<<<<<<<");
                found = true;
            }

            Console.WriteLine(new string('-', banner.Length));
        }
    }
    catch (Exception)
    {
        Console.WriteLine("Reading failed unexpectedly!");
    }

    return found;
}

Console.WriteLine("Press any key");
Console.ReadLine();