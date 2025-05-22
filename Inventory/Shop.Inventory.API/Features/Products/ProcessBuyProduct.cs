using Marten;
using MassTransit;
using Shop.Common.StateMachines;
using Shop.Inventory.Domain.Products;

namespace Shop.Inventory.API.Features.Products;

public class ProcessBuyConsumer : IConsumer<ProcessBuy>
{
    private readonly IDocumentStore _store;

    public ProcessBuyConsumer(IDocumentStore store)
    {
        _store = store;
    }

    public async Task Consume(ConsumeContext<ProcessBuy> context)
    {
        await using var session = await _store.LightweightSerializableSessionAsync();

        var product = await session.Events.AggregateStreamAsync<Product>(
                context.Message.ProductId,
                token: context.CancellationToken);

        if (product is null)
        {
            throw new Exception("Product not found.");
        }

        product.TakeFromStock(context.Message.Amount);

        session.Events.Append(product.Id, product.GetEventsList());
        product.ClearEvents();

        await session.SaveChangesAsync(context.CancellationToken);

        await context.RespondAsync(new BuyProcessed(context.Message.ProductId));
    }
}
