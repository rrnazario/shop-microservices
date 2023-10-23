using Carter;
using Carter.OpenApi;
using Marten;
using MediatR;
using Shop.Inventory.Domain.Model;

namespace Shop.Inventory.API.Features.Products;

public static class GetProduct
{
    internal record Query(Guid Id) : IRequest<Product>;

    record Handler : IRequestHandler<Query, Product>
    {
        private readonly IDocumentStore _store;

        public Handler(IDocumentStore store)
        {
            _store = store;
        }

        public async Task<Product> Handle(Query request, CancellationToken cancellationToken)
        {
            using var session = await _store.LightweightSerializableSessionAsync();

            var product = await session.Events.AggregateStreamAsync<Product>(
                request.Id,
                token: cancellationToken);

            if (product is null)
            {
                //TODO: proper threatment for this
                throw new Exception("Not found");
            }

            return product!;
        }
    }
}

public class GetProductModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet(
            "/products/{id}",
            async (Guid id, ISender sender) =>
            {
                var result = await sender.Send(new GetProduct.Query(id));

                return Results.Ok(result);
            })
            .IncludeInOpenApi()
            .Produces<Product>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status204NoContent);
    }
}
