using Shop.Inventory.API;
using Shop.Inventory.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.AddApplication();
builder.AddPersistence();

builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

app.UseHttpsRedirection();

app.UseApplication();

app.Run();
