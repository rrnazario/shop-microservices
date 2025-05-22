using Shop.Domain.SeedWork;

namespace Shop.Domain.Products;

public class Product
    : Entity, IAggregate
{
    public string Name { get; private set; }
    public string Description { get; private set; }

    public Product(string name, string description)
    {
        Name = name;
        Description = description;

        RaiseEvent(new ProductCreatedDomainEvent(name, description));
    }
}

record ProductCreatedDomainEvent(string name, string Description) : IDomainEvent;
