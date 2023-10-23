using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quartz;
using Shop.Common.Configurations;
using Shop.Domain.Interfaces;
using Shop.Domain.SeedWork;
using Shop.Infrastructure.Jobs;
using Shop.Infrastructure.Persistence;
using Shop.Infrastructure.Repositories;

namespace Shop.Infrastructure.DI;

public static class InfrastructureDI
{
    public static void AddInfrastructure(this WebApplicationBuilder builder)
    {
        builder.Services.AddDbContext<DatabaseContext>(ctx =>
        {
            var connectionString = builder.Configuration.GetConnectionString(EFPersistenceOptions.PersistenceSection);
            var options = builder.Configuration
                    .GetSection(EFPersistenceOptions.PersistenceSection)
                    .Get<EFPersistenceOptions>()!;

            ctx.UseNpgsql(connectionString, action =>
            {
                action.EnableRetryOnFailure(options.MaxRetryCount);
                action.CommandTimeout(options.CommandTimeout);
            });

            ctx.EnableDetailedErrors(options.EnableDetailedErrors);
            ctx.EnableSensitiveDataLogging(options.EnableSensitiveDataLogging);
        });

        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

        builder.AddRepositories();

        if (!builder.Environment.IsDevelopment())
        {
            builder.Services.AddQuartz(config =>
            {
                var jobKey = new JobKey(nameof(OutboxMessagesProcessorJob));

                config
                .AddJob<OutboxMessagesProcessorJob>(jobKey)
                .AddTrigger(trigger =>
                    trigger
                    .ForJob(jobKey)
                    .WithSimpleSchedule(schedule => schedule.WithIntervalInSeconds(5).RepeatForever()));
            });

            builder.Services.AddQuartzHostedService();
        }
    }

    private static void AddRepositories(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IProductRepository, ProductRepository>();
    }
}
