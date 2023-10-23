using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shop.Common.StateMachines;

namespace Shop.Infrastructure.Persistence;

public sealed class BuyProductStateMachineContext 
    : SagaDbContext
{
    public BuyProductStateMachineContext(DbContextOptions<DbContext> options)
        : base(options) { }

    protected override IEnumerable<ISagaClassMap> Configurations
    {
        get { yield return new BuyProductStateMap(); }
    }
}

public class BuyProductStateMap :
    SagaClassMap<BuyProductState>
{
    protected override void Configure(EntityTypeBuilder<BuyProductState> entity, ModelBuilder model)
    {
        entity.Property(x => x.CurrentState).HasMaxLength(64);
        entity.Property(x => x.Amount);
        entity.Property(x => x.ProductId);
    }
}
