using Marten;
using MassTransit;
using Shop.Common.Contracts;
using Shop.Inventory.Domain.Products;

namespace Shop.Inventory.API.Features.Products;

public sealed class ProductCreatedConsumer
    : IConsumer<ProductCreatedIntegrationEvent>
{
    private readonly IDocumentStore _store;

    public ProductCreatedConsumer(IDocumentStore store)
    {
        _store = store;
    }

    public async Task Consume(ConsumeContext<ProductCreatedIntegrationEvent> context)
    {
        await using var session = await _store.LightweightSerializableSessionAsync();

        var product = new Product(context.Message.Id, context.Message.Name, context.Message.Description);

        session.Events.StartStream<Product>(context.Message.Id, product.GetEventsList());

        await session.SaveChangesAsync();

        product.ClearEvents();

        //TODO: add logger
        Console.WriteLine($"Product {context.Message.Id} created successfully");
    }
}
