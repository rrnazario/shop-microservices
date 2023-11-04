using Shop.Inventory.DI;
using Shop.Inventory.Infrastructure.DI;

var builder = WebApplication.CreateBuilder(args);

builder.AddApplicationDependencies();
builder.AddPersistence();

builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

app.UseHttpsRedirection();

app.UseApplication();

app.Run();
