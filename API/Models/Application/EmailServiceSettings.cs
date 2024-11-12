namespace API.Models.Application;

public class EmailServiceSettings
{
    public string SenderName { get; init; } = "";
    public string SenderEmail { get; init; } = "";
    public string SenderPassword { get; init; } = "";
    public string SmtpHost { get; init; } = "";
    public int SmtpPort { get; init; } = -1;
}
