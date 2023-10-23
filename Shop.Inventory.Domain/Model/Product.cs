using Shop.Inventory.Domain.SeedWork;

namespace Shop.Inventory.Domain.Model
{
    public class Product
        : Entity, IAggregate
    {
        public Guid ProductId { get; set; }
        public string Name { get; private set; }
        public string Description { get; private set; }

        public Stock ProductStock { get; init; } = new();

        // For Marten
        Product() { }

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

        void Apply(StockAmountAdded evt)
        {
            ProductStock.Amount += evt.AmountAdded;
        }

        void Apply(StockAmountTaken evt)
        {
            if ((ProductStock.Amount - evt.AmountTaken) <= 0)
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

        public void Apply(NameChangedEvent evt)
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
}

public record ProductCreatedEvent(Guid Id, Guid ProductId) : DomainEvent;
public record NameChangedEvent(string Name) : DomainEvent;
public record DescriptionChangedEvent(string Description) : DomainEvent;

public record StockAmountAdded(int AmountAdded) : DomainEvent;
public record StockAmountTaken(int AmountTaken) : DomainEvent;
