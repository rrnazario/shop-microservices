using Shop.Inventory.Domain.SeedWork;

namespace Shop.Inventory.Domain.Products;

public partial class Product
{
    private void Apply(StockAmountAdded evt)
    {
        ProductStock.Amount += evt.AmountAdded;
    }

    private void Apply(StockAmountTaken evt)
    {
        if (ProductStock.Amount - evt.AmountTaken <= 0)
        {
            throw new Exception("Stock not available");
        }

        ProductStock.Amount -= evt.AmountTaken;
    }

    private void Apply(ProductCreatedEvent evt)
    {
        Id = evt.Id;
        ProductId = evt.ProductId;
    }

    private void Apply(NameChangedEvent evt)
    {
        if (!string.IsNullOrEmpty(evt.Name) &&
            !evt.Name.Equals(Name, StringComparison.CurrentCultureIgnoreCase))
        {
            Name = evt.Name;
        }
    }

    private void Apply(DescriptionChangedEvent evt)
    {
        if (!string.IsNullOrEmpty(evt.Description) &&
            !evt.Description.Equals(Description, StringComparison.CurrentCultureIgnoreCase))
        {
            Description = evt.Description;
        }
    }
}

public record ProductCreatedEvent(Guid Id, Guid ProductId) : DomainEvent;

public record NameChangedEvent(string Name) : DomainEvent;

public record DescriptionChangedEvent(string Description) : DomainEvent;

public record StockAmountAdded(int AmountAdded) : DomainEvent;

public record StockAmountTaken(int AmountTaken) : DomainEvent;