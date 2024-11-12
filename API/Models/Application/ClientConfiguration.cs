namespace API.Models.Application;

public class ClientConfiguration
{
    public const string Scheme = "http";
    public const string PathBase = "localhost:5173";
    public const string BaseUrl = $"{Scheme}://{PathBase}";
}
