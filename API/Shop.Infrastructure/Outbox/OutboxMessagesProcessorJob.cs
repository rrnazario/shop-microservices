using MediatR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Quartz;
using Shop.Domain.SeedWork;
using Shop.Infrastructure.Model;
using Shop.Infrastructure.Persistence;

namespace Shop.Infrastructure.Outbox;

[DisallowConcurrentExecution]
public sealed class OutboxMessagesProcessorJob : IJob
{
    private readonly DatabaseContext _context;
    private readonly IPublisher _publisher;

    public OutboxMessagesProcessorJob(
        DatabaseContext context,
        IPublisher publisher)
    {
        _context = context;
        _publisher = publisher;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var messages = await _context
            .Set<OutboxMessage>()
            .Where(_ => _.ProcessedAt == null)
            .Take(20)
            .ToListAsync(context.CancellationToken);

        foreach (var message in messages)
        {
            Console.WriteLine($"Processing message {message.Id}");
            var domainEvent = JsonConvert
                .DeserializeObject<IDomainEvent>(message.Content!, new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All,
                });

            if (domainEvent is null)
            {
                Console.Error.WriteLine("Failed to deserialize domain event");
                continue;
            }

            await _publisher.Publish(domainEvent);

            message.ProcessedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(context.CancellationToken);
    }
}
