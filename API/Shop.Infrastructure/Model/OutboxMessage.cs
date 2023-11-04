using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Shop.Infrastructure.Model;

public record OutboxMessage
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity), Key]
    public Guid Id { get; set; }
    public string? Type { get; set; }
    public string? Content { get; set; }
    public DateTime OcurredAt { get; set; }
    public DateTime? ProcessedAt { get; set; } = null;
}
