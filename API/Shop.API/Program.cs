using Shop.API.DI;
using Shop.Infrastructure.DI;

var builder = WebApplication.CreateBuilder(args);

builder.PerformConfigurations();
builder.AddInfrastructure();

builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

app.UseHttpsRedirection();

app.UseConfigurations();

app.Run();
