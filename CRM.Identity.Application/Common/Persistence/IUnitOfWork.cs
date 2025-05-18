namespace CRM.Identity.Application.Common.Persistence;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<TResult> ExecuteInTransactionAsync<TResult>(
    Func<Task<TResult>> operation,
    CancellationToken cancellationToken = default);
}