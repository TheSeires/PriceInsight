namespace API.Models.Database.Interfaces;

public interface IEntity<TId>
    where TId : IEquatable<TId>
{
    TId Id { get; }
}
