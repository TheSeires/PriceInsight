using API.Models.Database;

namespace API.Services.Interfaces;

public interface ICachedProductService : ICachedRepository<Product, string> { }
