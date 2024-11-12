namespace API.Services.Interfaces;

public interface IEmailSender
{
    Task<bool> SendEmailAsync(string email, string subject, string htmlMessage);
}
