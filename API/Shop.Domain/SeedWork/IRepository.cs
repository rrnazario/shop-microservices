namespace Shop.Domain.SeedWork;

public interface IRepository<T>
    where T : Entity, IAggregate
{
    Task<Guid> AddAsync(T entity, CancellationToken cancellationToken = default);
}
