using Marten;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Shop.Inventory.Domain.Products;

namespace Shop.Inventory.Infrastructure;

public static class PersistenceDI
{
    public static void AddPersistence(this WebApplicationBuilder builder)
    {
        var config = builder.Configuration.GetSection(ESPersistenceOptions.PersistenceSection)
            .Get<ESPersistenceOptions>(a => a.BindNonPublicProperties = true)!;

        builder.Services.AddMarten(options =>
        {
            options.Connection(builder.Configuration.GetConnectionString(ESPersistenceOptions.PersistenceSection)!);
            options.DatabaseSchemaName = config.DatabaseSchemaName;
            options.AutoCreateSchemaObjects = config.AutoCreateSchemaObjects;

            if (builder.Environment.IsDevelopment())
            {
                options.CreateDatabasesForTenants(provider =>
                {
                    provider
                    .ForTenant()
                    .CheckAgainstPgDatabase()
                    .WithEncoding("UTF-8")
                    .ConnectionLimit(-1);
                });
            }

            options.Schema.For<Product>().DocumentAlias(nameof(Product));
        });
    }


}

record ESPersistenceOptions(
    string DatabaseSchemaName,
     Weasel.Core.AutoCreate AutoCreateSchemaObjects)
{
    public const string PersistenceSection = "ESPersistence";
}
