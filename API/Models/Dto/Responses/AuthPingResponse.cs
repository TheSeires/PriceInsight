namespace API.Models.Application.Dto.Responses;

public class AuthPingResponse
{
    public string Id { get; set; }
    public string Email { get; set; }
    public string Username { get; set; }
    public List<string> Roles { get; set; }

    public AuthPingResponse(string id, string email, string username, List<string> roles)
    {
        Id = id;
        Email = email;
        Username = username;
        Roles = roles;
    }
}