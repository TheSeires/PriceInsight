namespace API.Services.Interfaces;

public interface ICategoryUpdateManager
{
    Task UpdateCategoriesAsync(CancellationToken cancellationToken);
}
