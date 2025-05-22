using Shop.API;
using Shop.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.AddAplication();
builder.AddInfrastructure();

builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

app.UseHttpsRedirection();

app.UseApplication();
app.TryApplyMigrations();

app.Run();
