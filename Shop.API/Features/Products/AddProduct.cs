using Carter;
using Carter.OpenApi;
using FluentValidation;
using MassTransit;
using MediatR;
using Shop.Common.Contracts;
using Shop.Domain.Interfaces;
using Shop.Domain.Model;
using Shop.Domain.SeedWork;
using Shop.Infrastructure.Persistence;

namespace Shop.API.Features.Products;

public static class AddProduct
{
    internal record Command(string Name, string Description) : IRequest<Response>;
    internal record Response(Guid Id);

    record Handler : IRequestHandler<Command, Response>
    {
        private readonly IPublishEndpoint _publisher;
        private readonly IProductRepository _repository;
        private readonly IUnitOfWork _unitOfWork;

        public Handler(IPublishEndpoint publisher, IProductRepository repository, IUnitOfWork unitOfWork)
        {
            _publisher = publisher;
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Response> Handle(Command request, CancellationToken cancellationToken)
        {
            var newId = await _repository.AddAsync(new(request.Name, request.Description), cancellationToken);
            var response = new Response(newId);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _publisher.Publish(
                new ProductCreatedIntegrationEvent(
                    response.Id,
                    request.Name,
                    request.Description,
                    DateTime.UtcNow), cancellationToken);

            return response;
        }
    }

    class Validator
    : AbstractValidator<Command>
    {
        private readonly IServiceProvider _serviceProvider;
        public Validator(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;

            RuleFor(_ => _.Name)
                .NotEmpty()
                .WithMessage("Name must have a value")
                .Must(HaveUniqueName)
                .WithMessage("Name must be unique");

            RuleFor(_ => _.Description)
                .NotEmpty()
                .WithMessage("Description must have a value");
        }

        private bool HaveUniqueName(string name)
        {
            using var scope = _serviceProvider.CreateScope();

            var dbContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

            return !dbContext.Set<Product>().Any(p => p.Name.Equals(name));
        }
    }
}

public class AddProductModule : ICarterModule
{
    private const string Route = "/products";
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app
            .MapPost<AddProduct.Command>(
            Route,
            async (ISender sender, AddProduct.Command command) =>
            {
                var response = await sender.Send(command);

                return Results.Created(Route, response);
            })
           .IncludeInOpenApi()
           .Produces<AddProduct.Response>(StatusCodes.Status201Created)
           .ProducesProblem(StatusCodes.Status401Unauthorized)
           .ProducesProblem(StatusCodes.Status422UnprocessableEntity);
    }
}


