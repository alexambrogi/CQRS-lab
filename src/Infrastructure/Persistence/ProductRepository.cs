using CQRS.POC.Application.Abstractions;
using CQRS.POC.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CQRS.POC.Infrastructure.Persistence
{
    public class ProductRepository : IProductRepository
    {
        private readonly AppDbContext _context;

        public ProductRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => await _context.Products.FirstOrDefaultAsync(p => p.Id == id, ct);

        public async Task AddAsync(Product product, CancellationToken ct = default)
            => await _context.Products.AddAsync(product, ct);

        public Task UpdateAsync(Product product, CancellationToken ct = default)
        {
            _context.Products.Update(product);
            return Task.CompletedTask;
        }

        public async Task<int> SaveChangesAsync(CancellationToken ct = default)
            => await _context.SaveChangesAsync(ct);
    }
}
