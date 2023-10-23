using Asp.Versioning;
using Carter;
using Carter.OpenApi;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Shop.API.Features.Products;
using Shop.Common;
using Shop.Common.StateMachines;
using System.Reflection;

namespace Shop.API.DI;

public static class ApplicationDI
{
    /// <summary>
    /// Will register Carter, Swagger, APIVersioning and Mediator properly
    /// </summary>
    public static void PerformConfigurations(this WebApplicationBuilder builder)
    {
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Description = "Shop API",
                Version = "v1",
                Title = "Shop API"
            });

            options.DocInclusionPredicate((_, description) =>
                description.ActionDescriptor.EndpointMetadata.Any(_ => _ is IIncludeOpenApi));
        });

        builder.Services.AddCarter();
        builder.Services.AddMediatR(c => c.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        builder.Services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.ReportApiVersions = true;
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ApiVersionReader = new HeaderApiVersionReader("X-Api-Version");
        });

        builder.Services.AddMassTransit(config =>
        {
            var mqOptions = builder.Configuration.GetSection("MessageQueue").Get<MQOptions>()!;
            config.SetKebabCaseEndpointNameFormatter();

            config.UsingRabbitMq((ctx, rabbitConfig) =>
            {
                rabbitConfig.Host(new Uri(mqOptions.Host), host =>
                {
                    host.Username(mqOptions.User);
                    host.Password(mqOptions.Password);
                });

                rabbitConfig.ConfigureEndpoints(ctx);
            });

            config.AddSagaStateMachine<BuyProductStateMachine, BuyProductState>()
                .InMemoryRepository();
        });
    }    

    public static void UseConfigurations(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.MapCarter();
    }
}
