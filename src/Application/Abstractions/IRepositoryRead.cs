using CQRS.POC.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQRS.POC.Application.Abstractions
{
    public interface IRepositoryRead<TEntity, TKey> where TEntity : class
    {
        Task<TEntity?> GetByIdAsync(TKey id, CancellationToken ct = default);

        Task<PagedResult<TEntity>> GetPagedAsync( int page, int pageSize, string? searchTerm = null, CancellationToken ct = default);
    }



}
