using Shop.Domain.Products;
using Shop.Domain.SeedWork;
using Shop.Infrastructure.Persistence;

namespace Shop.Infrastructure.Repositories;

public class ProductRepository
    : IProductRepository
{
    private readonly DatabaseContext _databaseContext;

    public ProductRepository(DatabaseContext databaseContext)
    {
        _databaseContext = databaseContext;
    }

    public async Task<Guid> AddAsync(Product entity, CancellationToken cancellationToken = default)
    {
        var product = await _databaseContext.FindAsync<Product>(entity.Id, cancellationToken);

        if (product is not null)
        {
            throw new EntityAlreadyExistsException<Product>(product.Id);
        }

        var result = await _databaseContext.Set<Product>().AddAsync(entity, cancellationToken);

        return result.Entity.Id;
    }
}
