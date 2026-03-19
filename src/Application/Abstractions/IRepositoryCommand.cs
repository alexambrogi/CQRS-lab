namespace CQRS.POC.Application.Abstractions
{
    public interface IRepositoryCommand<TEntity, TKey>
        where TEntity : class
    {
        //Task<TEntity?> GetByIdAsync(TKey id, CancellationToken ct = default);
        Task AddAsync(TEntity entity, CancellationToken ct = default);
        Task UpdateAsync(TEntity entity, CancellationToken ct = default);
        Task<int> SaveChangesAsync(CancellationToken ct = default);
    }
}
