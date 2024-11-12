namespace API.Services.Interfaces;

public interface IEmailContentBuilder
{
    string GenerateEmailConfirmation(string confirmationLink, string userName);
}
