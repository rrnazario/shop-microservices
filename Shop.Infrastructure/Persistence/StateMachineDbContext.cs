using Microsoft.EntityFrameworkCore;
using Shop.Domain.Model;
using Shop.Infrastructure.Model;

namespace Shop.Infrastructure.Persistence;

public sealed class StateMachineDbContext : DbContext
{
    public DbSet<Product> Products { get; set; }
    public DbSet<OutboxMessage> OutboxMessages { get; set; }

    public StateMachineDbContext(DbContextOptions<DbContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>();
        modelBuilder.Entity<OutboxMessage>();

        base.OnModelCreating(modelBuilder);
    }
}
