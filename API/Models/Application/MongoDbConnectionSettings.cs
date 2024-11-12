namespace API.Models.Application;

public class MongoDbConnectionSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
}
