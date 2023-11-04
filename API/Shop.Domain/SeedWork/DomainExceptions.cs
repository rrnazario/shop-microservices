namespace Shop.Domain.SeedWork;

public class EntityAlreadyExistsException<T> : Exception
    where T : Entity
{
    public EntityAlreadyExistsException(Guid id)
        : base($"{typeof(T).Name} with Id = {id} already exists") { }
}
