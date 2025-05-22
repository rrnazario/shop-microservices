using Shop.Inventory.Domain.SeedWork;

namespace Shop.Inventory.Domain.Products;

public partial class Product
    : Entity, IAggregate
{
    public Guid ProductId { get; set; }
    public string Name { get; private set; }
    public string Description { get; private set; }

    public Stock ProductStock { get; init; } = new();

    // For Marten
    Product()
    {
    }

    public Product(Guid productId, string name, string description)
    {
        Id = Guid.NewGuid();

        var productIdEvent = new ProductCreatedEvent(Id, productId);
        RaiseEvent(productIdEvent);
        Apply(productIdEvent);

        var nameChanged = new NameChangedEvent(name);
        RaiseEvent(nameChanged);
        Apply(nameChanged);

        var descriptionChanged = new DescriptionChangedEvent(description);
        RaiseEvent(descriptionChanged);
        Apply(descriptionChanged);
    }

    public void AddStock(int amount)
    {
        var @event = new StockAmountAdded(amount);
        Apply(@event);

        RaiseEvent(@event);
    }

    public void TakeFromStock(int amount)
    {
        var @event = new StockAmountTaken(amount);
        Apply(@event);

        RaiseEvent(@event);
    }
}