using API.Models.Application;
using API.Services.Interfaces;
using MailKit.Net.Smtp;
using MimeKit;

namespace API.Services;

public class EmailSender : IEmailSender
{
    private readonly ILogger _logger;
    private readonly EmailServiceSettings _settings;

    private readonly MailboxAddress _senderMailboxAddress;

    public EmailSender(IConfiguration configuration, ILogger<EmailSender> logger)
    {
        var emailServiceSettings = configuration
            .GetSection(nameof(EmailServiceSettings))
            .Get<EmailServiceSettings>();
        ArgumentNullException.ThrowIfNull(emailServiceSettings);

        _settings = emailServiceSettings;
        _senderMailboxAddress = new MailboxAddress(_settings.SenderName, _settings.SenderEmail);

        _logger = logger;

        ValidateSettings();
    }

    public async Task<bool> SendEmailAsync(string email, string subject, string htmlMessage)
    {
        if (ValidateSettings() == false)
            return false;

        var message = new MimeMessage();
        message.From.Add(_senderMailboxAddress);
        message.To.Add(new MailboxAddress(string.Empty, email));
        message.Subject = subject;
        message.Body = new TextPart("html") { Text = htmlMessage };

        try
        {
            using var smtpClient = new SmtpClient();
            await smtpClient.ConnectAsync(_settings.SmtpHost, _settings.SmtpPort);
            await smtpClient.AuthenticateAsync(_settings.SenderEmail, _settings.SenderPassword);
            await smtpClient.SendAsync(message);
            await smtpClient.DisconnectAsync(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.ToString());
            return false;
        }

        return true;
    }

    public bool ValidateSettings()
    {
        if (string.IsNullOrWhiteSpace(_settings.SenderEmail))
        {
            _logger.LogError("SenderEmail is not set!");
            return false;
        }

        if (string.IsNullOrWhiteSpace(_settings.SenderPassword))
        {
            _logger.LogError("SenderPassword is not set!");
            return false;
        }

        if (string.IsNullOrWhiteSpace(_settings.SmtpHost))
        {
            _logger.LogError("SmtpHost is not set!");
            return false;
        }

        if (_settings.SmtpPort == -1)
        {
            _logger.LogError("SmtpPort is not set!");
            return false;
        }

        return true;
    }
}
