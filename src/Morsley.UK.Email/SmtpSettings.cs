﻿namespace Morsley.UK.Email;

public class SmtpSettings
{
    public string Server { get; set; } = "";

    public int Port { get; set; } = 0;

    public bool UseSsl { get; set; } = false;
    
    public bool UseStartTls { get; set; } = true;

    public string Username { get; set; } = "";

    public string Password { get; set; } = "";

    public string FromName { get; set; } = "Morsley.UK.Email.EmailSender";

    public string FromAddress { get; set; } = "no-reply@example.uk";

    public int TimeoutSeconds { get; set; } = 30;
}