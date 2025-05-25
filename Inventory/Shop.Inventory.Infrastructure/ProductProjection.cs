using Marten.Events.Aggregation;
using Shop.Inventory.Domain.Products;

namespace Shop.Inventory.Infrastructure;

public class ProductProjection : SingleStreamProjection<Product>
{
    public void Apply(ProductCreatedEvent @event, Product product)
        => product.Apply(@event);

    public void Apply(ProductNameChangedEvent @event, Product product)
        => product.Apply(@event);

    public void Apply(DescriptionChangedEvent @event, Product product)
        => product.Apply(@event);

    public void Apply(StockAmountAdded @event, Product product)
        => product.Apply(@event);

    public void Apply(StockAmountTaken @event, Product product)
        => product.Apply(@event);
}