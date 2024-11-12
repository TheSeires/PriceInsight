namespace API.Models.Application.Dto.Responses;

public class LoginUserResponse
{
    public required string? Username { get; set; }
    public required string? Email { get; set; }
    public required List<string> Roles { get; set; }
}
