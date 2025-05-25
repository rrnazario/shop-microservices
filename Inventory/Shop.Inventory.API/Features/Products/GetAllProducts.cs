using Carter;
using Carter.OpenApi;
using Marten;
using MediatR;
using Shop.Inventory.Domain.Products;

namespace Shop.Inventory.API.Features.Products;

public static class GetAllProducts
{
    internal record Query : IRequest<IReadOnlyCollection<Product>>;

    private record Handler : IRequestHandler<Query, IReadOnlyCollection<Product>>
    {
        private readonly IQuerySession _session;

        public Handler(IQuerySession session)
        {
            _session = session;
        }

        public async Task<IReadOnlyCollection<Product>> Handle(Query request, CancellationToken cancellationToken)
        {
            var products = await _session.Query<Product>()
                .ToListAsync(cancellationToken);

            return products;
        }
    }
}

public class GetAllProductsModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet(
            "/products",
            async (ISender sender) =>
            {
                var result = await sender.Send(new GetAllProducts.Query());

                return Results.Ok(result);
            })
            .IncludeInOpenApi()
            .Produces<Product[]>();
    }
}
