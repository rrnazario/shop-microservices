using Shop.Domain.SeedWork;

namespace Shop.Domain.Model;

public class Product
    : Entity, IAggregate
{
    public string Name { get; private set; }
    public string Description { get; private set; }

    public Product(string name, string description)
    {
        Name = name;
        Description = description;

        RaiseEvent(new ProductCreatedEvent(name, description));
    }    
}

record NameChangedEvent(string Name) : IDomainEvent;
record DescriptionChangedEvent(string Description) : IDomainEvent;
record ProductCreatedEvent(string name, string Description) : IDomainEvent;
