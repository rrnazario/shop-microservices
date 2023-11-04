using Asp.Versioning;
using Carter;
using Carter.OpenApi;
using Microsoft.OpenApi.Models;
using System.Reflection;

namespace Shop.API;

public static class ApplicationDI
{
  /// <summary>
  /// Will register Carter, Swagger, APIVersioning and Mediator properly
  /// </summary>
  public static void AddAplication(this WebApplicationBuilder builder)
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
  }

  public static void UseApplication(this WebApplication app)
  {
    if (app.Environment.IsDevelopment())
    {
      app.UseSwagger();
      app.UseSwaggerUI();
    }

    app.MapCarter();
  }
}
