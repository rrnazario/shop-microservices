using System.Reflection;
using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Shop.Common;
using Shop.Common.Configurations;
using Shop.Common.StateMachines;
using Shop.Domain.Products;
using Shop.Domain.SeedWork;
using Shop.Infrastructure.Outbox;
using Shop.Infrastructure.Persistence;
using Shop.Infrastructure.Repositories;

namespace Shop.Infrastructure;

public static class InfrastructureDI
{
    public static void TryApplyMigrations(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
        dbContext.Database.Migrate();

        var sagaContext = scope.ServiceProvider.GetRequiredService<BuyProductStateMachineContext>();
        sagaContext.Database.Migrate();
    }

    public static void AddInfrastructure(this WebApplicationBuilder builder)
    {
        builder.AddDatabaseContext();

        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

        builder.AddRepositories();
        builder.AddMassTransit();

        //if (!builder.Environment.IsDevelopment())
        {
            builder.Services.AddQuartz(config =>
            {
                var jobKey = new JobKey(nameof(OutboxMessagesProcessorJob));

                config
                    .AddJob<OutboxMessagesProcessorJob>(jobKey)
                    .AddTrigger(trigger =>
                        trigger
                            .ForJob(jobKey)
                            .WithSimpleSchedule(schedule => schedule.WithIntervalInHours(5).RepeatForever()));
            });

            builder.Services.AddQuartzHostedService();
        }
    }

    private static void AddDatabaseContext(this WebApplicationBuilder builder)
    {
        var connectionString = builder.Configuration
            .GetConnectionString(EFPersistenceOptions.PersistenceSection);

        var options = builder.Configuration
            .GetSection(EFPersistenceOptions.PersistenceSection)
            .Get<EFPersistenceOptions>()!;

        builder.Services.AddDbContext<DatabaseContext>(ctx =>
        {
            ctx.UseNpgsql(connectionString, action =>
            {
                action.EnableRetryOnFailure(options.MaxRetryCount);
                action.CommandTimeout(options.CommandTimeout);
            });

            ctx.EnableDetailedErrors(options.EnableDetailedErrors);
            ctx.EnableSensitiveDataLogging(options.EnableSensitiveDataLogging);
        });

        builder.Services.AddDbContext<BuyProductStateMachineContext>(ctx =>
        {
            ctx.UseNpgsql(connectionString, action =>
            {
                action.MigrationsAssembly(Assembly.GetExecutingAssembly().GetName().Name);
                action.MigrationsHistoryTable($"__{nameof(JobServiceSagaDbContext)}");
            });
        });

        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
    }

    private static void AddRepositories(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IProductRepository, ProductRepository>();
    }

    private static void AddMassTransit(this WebApplicationBuilder builder)
    {
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
// #if DEBUG
//
//             .InMemoryRepository();
// #else
                .EntityFrameworkRepository(opt =>
                {
                    opt.ExistingDbContext<BuyProductStateMachineContext>();
                    opt.UsePostgres();
                });
//#endif
        });
    }
}