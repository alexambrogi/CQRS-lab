using CQRS.POC.Domain.Entities;

namespace CQRS.POC.Application.Abstractions
{
    public interface IProductRepository : IRepositoryCommand<Product, Guid>
    {
        Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default);
    }
}
