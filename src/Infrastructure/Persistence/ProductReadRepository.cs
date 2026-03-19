using CQRS.POC.Application.Abstractions;
using CQRS.POC.Application.Common.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Linq;

namespace CQRS.POC.Infrastructure.Persistence
{
    public class ProductReadRepository : IProductReadRepository
    {
        private readonly AppDbContext _dbContext;

        public ProductReadRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ProductDTO?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _dbContext.Products
                .AsNoTracking()
                .Where(p => p.Id == id)
                .Select(p => new ProductDTO
                (
                    p.Id
                    , p.Name
                    , p.Description
                    , p.Price
                    , p.Stock
                    , p.IsActive
                    , p.CreatedAt
                )).FirstOrDefaultAsync(ct);
        }

        public async Task<PagedResult<ProductDTO>> GetPagedAsync(int page, int pageSize, string? searchTerm = null, CancellationToken ct = default)
        {
            //return await _dbContext.Products
            //    .AsNoTracking()
            //    .Where(p => string.IsNullOrEmpty(searchTerm) || p.Name.Contains(searchTerm))
            //    .Select(p => new ProductDTO
            //    {
            //        Id = p.Id,
            //        Name = p.Name,
            //        Description = p.Description,
            //        Price = p.Price
            //    })
            //    .ToPagedResultAsync(page, pageSize, ct);
            var query = _dbContext.Products.AsNoTracking().Where(p => p.IsActive);

            if (!string.IsNullOrWhiteSpace(searchTerm))
                query = query.Where(p =>
                    p.Name.Contains(searchTerm) ||
                    p.Description.Contains(searchTerm));

            var total = await query.CountAsync(ct);

            var items = await query
                .OrderBy(p => p.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new ProductDTO(
                    p.Id
                    , p.Name
                    , p.Description
                    , p.Price
                    , p.Stock
                    , p.IsActive
                    , p.CreatedAt)
                ).ToListAsync(ct);

            return new PagedResult<ProductDTO>(items, page, pageSize, total);
        }
    }
}
