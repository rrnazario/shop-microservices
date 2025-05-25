using Shop.Inventory.Domain.SeedWork;

namespace Shop.Inventory.Domain.Products;

public partial class Product
{
    public void Apply(StockAmountAdded evt)
    {
        ProductStock.Amount += evt.AmountAdded;
    }

    public void Apply(StockAmountTaken evt)
    {
        if (ProductStock.Amount - evt.AmountTaken <= 0)
        {
            throw new Exception("Stock not available");
        }

        ProductStock.Amount -= evt.AmountTaken;
    }

    public void Apply(ProductCreatedEvent evt)
    {
        Id = evt.Id;
        ProductId = evt.ProductId;
    }

    public void Apply(ProductNameChangedEvent evt)
    {
        if (!string.IsNullOrEmpty(evt.Name) &&
            !evt.Name.Equals(Name, StringComparison.CurrentCultureIgnoreCase))
        {
            Name = evt.Name;
        }
    }

    public void Apply(DescriptionChangedEvent evt)
    {
        if (!string.IsNullOrEmpty(evt.Description) &&
            !evt.Description.Equals(Description, StringComparison.CurrentCultureIgnoreCase))
        {
            Description = evt.Description;
        }
    }
}

public record ProductCreatedEvent(Guid Id, Guid ProductId) : DomainEvent;

public record ProductNameChangedEvent(string Name) : DomainEvent;

public record DescriptionChangedEvent(string Description) : DomainEvent;

public record StockAmountAdded(int AmountAdded) : DomainEvent;

public record StockAmountTaken(int AmountTaken) : DomainEvent;