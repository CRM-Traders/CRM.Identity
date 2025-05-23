namespace CRM.Identity.Persistence.Repositories.Base;

public class Repository<TEntity> : IRepository<TEntity> where TEntity : Entity
{
    protected readonly ApplicationDbContext _dbContext;
    protected readonly DbSet<TEntity> _dbSet;

    public Repository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
        _dbSet = dbContext.Set<TEntity>();
    }

    public async Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FindAsync(new object[] { id }, cancellationToken);
    }


    public async Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TEntity>> ListAsync(ISpecification<TEntity> specification,
        CancellationToken cancellationToken = default)
    {
        return await ApplySpecification(specification).ToListAsync(cancellationToken);
    }

    public async Task<TEntity?> FirstOrDefaultAsync(ISpecification<TEntity> specification,
        CancellationToken cancellationToken = default)
    {
        return await ApplySpecification(specification).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
        return entity;
    }

    public Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        _dbContext.Entry(entity).State = EntityState.Modified;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        _dbSet.Remove(entity);
        return Task.CompletedTask;
    }

    public async Task<int> CountAsync(ISpecification<TEntity> specification,
        CancellationToken cancellationToken = default)
    {
        return await ApplySpecification(specification).CountAsync(cancellationToken);
    }

    public async Task<bool> AnyAsync(ISpecification<TEntity> specification,
        CancellationToken cancellationToken = default)
    {
        return await ApplySpecification(specification).AnyAsync(cancellationToken);
    }

    private IQueryable<TEntity> ApplySpecification(ISpecification<TEntity> specification)
    {
        return SpecificationEvaluator<TEntity>.GetQuery(_dbSet.AsQueryable(), specification);
    }

    public IQueryable<TEntity> GetQueryable()
    {
        return _dbSet.AsQueryable();
    }

    public async Task<List<TEntity>> GetByIdsAsync(List<Guid> ids, CancellationToken cancellationToken = default)
    {
        if (!ids.Any()) return new List<TEntity>();

        return await _dbSet.Where(x => ids.Contains(x.Id)).ToListAsync(cancellationToken);
    }

    public async Task<List<TEntity>> GetByEmailsAsync(List<string> emails,
        CancellationToken cancellationToken = default)
    {
        if (!emails.Any()) return new List<TEntity>();

        var lowerEmails = emails.Select(e => e.ToLower()).ToList();

        return await _dbSet
            .Where(x => lowerEmails.Contains(EF.Property<string>(x, "Email").ToLower()))
            .ToListAsync(cancellationToken);
    }

    public async Task<List<TEntity>> GetByEmailsOrUsernamesAsync(List<string> emails, List<string> usernames,
        CancellationToken cancellationToken = default)
    {
        if (!emails.Any() && !usernames.Any()) return new List<TEntity>();

        var lowerEmails = emails.Select(e => e.ToLower()).ToList();
        var lowerUsernames = usernames.Select(u => u.ToLower()).ToList();

        return await _dbSet
            .Where(x => lowerEmails.Contains(EF.Property<string>(x, "Email").ToLower()) ||
                        lowerUsernames.Contains(EF.Property<string>(x, "Username").ToLower()))
            .ToListAsync(cancellationToken);
    }
}