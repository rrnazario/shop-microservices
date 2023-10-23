using Microsoft.EntityFrameworkCore;
using Shop.Domain.Model;
using OutboxMessage = Shop.Infrastructure.Model.OutboxMessage;

namespace Shop.Infrastructure.Persistence;

//public sealed class DatabaseContext : SagaDbContext
//{
//    public DbSet<Product> Products { get; set; }

//    protected override IEnumerable<ISagaClassMap> Configurations => throw new NotImplementedException();

//    public DatabaseContext(DbContextOptions<DatabaseContext> options)
//        : base(options) { }

//    protected override void OnModelCreating(ModelBuilder modelBuilder)
//    {
//        modelBuilder.Entity<Product>();

//        base.OnModelCreating(modelBuilder);
//    }
//}

//public class StateMachineMap : SagaClassMap<OrderStateInstance>
//{
//    protected override void Configure(EntityTypeBuilder<OrderStateInstance> entity, ModelBuilder model)
//    {
//        base.Configure(entity, model);
//    }
//}

public sealed class DatabaseContext : DbContext
{
    public DbSet<Product> Products { get; set; }
    public DbSet<OutboxMessage> OutboxMessages { get; set; }

    public DatabaseContext(DbContextOptions<DatabaseContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>();
        modelBuilder.Entity<OutboxMessage>();

        base.OnModelCreating(modelBuilder);
    }
}
