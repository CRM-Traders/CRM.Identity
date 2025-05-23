namespace CRM.Identity.Application.Common.Abstractions.Mediators;

public interface IRepository<TEntity> where TEntity : Entity
{
    Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TEntity>> ListAsync(ISpecification<TEntity> specification,
        CancellationToken cancellationToken = default);

    Task<TEntity?> FirstOrDefaultAsync(ISpecification<TEntity> specification,
        CancellationToken cancellationToken = default);

    Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task<int> CountAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default);
    Task<bool> AnyAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default);
    IQueryable<TEntity> GetQueryable();
    Task<List<TEntity>> GetByIdsAsync(List<Guid> ids, CancellationToken cancellationToken = default);
    Task<List<TEntity>> GetByEmailsAsync(List<string> emails, CancellationToken cancellationToken = default);

    Task<List<TEntity>> GetByEmailsOrUsernamesAsync(List<string> emails, List<string> usernames,
        CancellationToken cancellationToken = default);
}