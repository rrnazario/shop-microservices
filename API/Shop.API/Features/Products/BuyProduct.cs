using Carter;
using Carter.OpenApi;
using MassTransit;
using Shop.Common.StateMachines;

namespace Shop.API.Features.Products;

public class BuyProductModule : ICarterModule
{
  public void AddRoutes(IEndpointRouteBuilder app)
  => app.MapPut(
          "/products/{productId}/buy/{amount}",
          async (Guid productId, int amount
          , IRequestClient<StartBuy> bus
          ) =>
          {
            var response = await bus.GetResponse<BuyProcessed, BuyCancelled>(new StartBuy(productId, amount));

            if (response.Is<BuyCancelled>(out var buyCancelled))
            {
              return Results.BadRequest(new { Message = buyCancelled.Message.Reason });
            }

            return Results.Ok();
          })
          .IncludeInOpenApi()
          .Produces(StatusCodes.Status201Created)
          .ProducesProblem(StatusCodes.Status401Unauthorized)
          .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

}
