namespace Shop.Infrastructure.Model;

public record OutboxMessage
{
    public Guid Id { get; set; }
    public string? Type { get; set; }
    public string? Content { get; set; }
    public DateTime OcurredAt { get; set; }
    public DateTime? ProcessedAt { get; set; } = null;
}
