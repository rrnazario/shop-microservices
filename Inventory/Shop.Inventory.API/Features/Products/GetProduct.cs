using Carter;
using Carter.OpenApi;
using Marten;
using MediatR;
using Shop.Inventory.Domain.Products;

namespace Shop.Inventory.API.Features.Products;

public static class GetProduct
{
    internal record Query(Guid Id) : IRequest<Product>;

    private record Handler : IRequestHandler<Query, Product>
    {
        private readonly IQuerySession _session;

        public Handler(IQuerySession session)
        {
            _session = session;
        }

        public async Task<Product> Handle(Query request, CancellationToken cancellationToken)
        {
            var product = await _session.LoadAsync<Product>(request.Id,
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
            .Produces<Product>()
            .Produces(StatusCodes.Status404NotFound);
    }
}
