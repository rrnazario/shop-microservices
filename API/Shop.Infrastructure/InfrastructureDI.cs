using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quartz;
using Shop.Common;
using Shop.Common.Configurations;
using Shop.Common.StateMachines;
using Shop.Domain.Products;
using Shop.Domain.SeedWork;
using Shop.Infrastructure;
using Shop.Infrastructure.Jobs;
using Shop.Infrastructure.Persistence;
using Shop.Infrastructure.Repositories;

namespace Shop.Infrastructure;

public static class InfrastructureDI
{
    public static void AddInfrastructure(this WebApplicationBuilder builder)
    {
        builder.AddDatabaseContext();

        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

        builder.AddRepositories();
        builder.AddMassTransit();

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



    private static void AddDatabaseContext(this WebApplicationBuilder builder)
    {
        builder.Services.AddDbContext<DatabaseContext>(ctx =>
        {
            var connectionString = builder.Configuration
            .GetConnectionString(EFPersistenceOptions.PersistenceSection);

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
#if DEBUG

            .InMemoryRepository();
#else
            .EntityFrameworkRepository(opt =>
             {
                 var connectionString = builder.Configuration
                     .GetConnectionString(EFPersistenceOptions.PersistenceSection);

                 opt.ConcurrencyMode = ConcurrencyMode.Pessimistic;

                 opt.AddDbContext<DbContext, BuyProductStateMachineContext>((provider, builder) =>
                 {
                     builder.UseNpgsql(
                         connectionString,
                         m =>
                         {
                             m.MigrationsAssembly(Assembly.GetExecutingAssembly().GetName().Name);
                             m.MigrationsHistoryTable($"__{nameof(BuyProductStateMachineContext)}");
                         });
                 });
             });
#endif
        });
    }
}
