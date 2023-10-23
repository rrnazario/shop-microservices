using Newtonsoft.Json;
using Shop.Domain.SeedWork;
using Shop.Infrastructure.Model;

namespace Shop.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly DatabaseContext _dbContext;

    public UnitOfWork(DatabaseContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ConvertDomainEventToOutboxEvents();

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private void ConvertDomainEventToOutboxEvents()
    {
        var allEvents = _dbContext
            .ChangeTracker
            .Entries<Entity>()
            .Select(_ => _.Entity)
            .SelectMany(agg =>
            {
                var domainEvents = agg.GetEventsList();

                agg.ClearEvents();

                return domainEvents;
            })
            .Select(de => new OutboxMessage
            {
                OcurredAt = DateTime.UtcNow,
                Type = de.GetType().Name,
                Content = JsonConvert.SerializeObject(de, new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All
                })
            })
            .ToList();

        _dbContext.Set<OutboxMessage>().AddRange(allEvents);
    }
}