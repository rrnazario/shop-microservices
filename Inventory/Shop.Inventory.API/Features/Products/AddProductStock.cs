using Carter;
using Carter.OpenApi;
using Marten;
using MediatR;
using Shop.Inventory.Domain.Products;

namespace Shop.Inventory.API.Features.Products;

public static class AddProductStock
{
    internal record Command(Guid Id, int Amount) : IRequest;

    private record Handler : IRequestHandler<Command>
    {
        private readonly IDocumentStore _store;

        public Handler(IDocumentStore store)
        {
            _store = store;
        }

        public async Task Handle(Command request, CancellationToken cancellationToken)
        {
            await using var session = await _store.LightweightSerializableSessionAsync(cancellationToken);
            
            var product = await session.Events.AggregateStreamAsync<Product>(
                request.Id,
                token: cancellationToken);

            if (product is null)
            {
                //TODO: proper threatment for this
                throw new Exception("Not found");
            }

            product.AddStock(request.Amount);
            
            session.Events.Append(product.Id, product.GetEventsList());

            await session.SaveChangesAsync(cancellationToken);
            
            product.ClearEvents();
        }
    }
}

public class AddProductStockModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut(
            "/products/{id}/{amount}",
            async (Guid id, int amount, ISender sender) =>
            {
                await sender.Send(new AddProductStock.Command(id, amount));
            })
            .IncludeInOpenApi()
            .Produces(StatusCodes.Status200OK);
    }
}
