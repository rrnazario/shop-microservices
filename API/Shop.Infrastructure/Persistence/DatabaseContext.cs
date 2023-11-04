using Microsoft.EntityFrameworkCore;
using Shop.Domain.Products;
using Shop.Infrastructure.Model;

namespace Shop.Infrastructure.Persistence;

public sealed class DatabaseContext : DbContext
{
    public DbSet<Product> Products { get; set; }
    public DbSet<OutboxMessage> OutboxMessages { get; set; }

    public DatabaseContext(DbContextOptions<DatabaseContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(product =>
        {
            product.HasKey(_ => _.Id);
            product.Property(_ => _.Id).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<OutboxMessage>(outbox =>
        {
            outbox.HasKey(_ => _.Id);
            outbox.Property(_ => _.Id).ValueGeneratedOnAdd();
        });

        base.OnModelCreating(modelBuilder);
    }
}
