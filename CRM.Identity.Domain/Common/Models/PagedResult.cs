namespace CRM.Identity.Domain.Common.Models;

public class PagedResult<TValue> : Result<IReadOnlyList<TValue>>
{
    public int TotalCount { get; }
    public int Page { get; }
    public int PageSize { get; }
    public int TotalPages { get; }

    public bool HasPreviousPage => Page > 1;

    public bool HasNextPage => Page < TotalPages;

    private PagedResult(IReadOnlyList<TValue> items, int totalCount, int page, int pageSize,
        bool isSuccess, string? error, string? errorCode,
        IReadOnlyDictionary<string, string[]>? validationErrors,
        IReadOnlyDictionary<string, object>? metadata)
        : base(items, isSuccess, error, errorCode, validationErrors, metadata)
    {
        TotalCount = totalCount;
        Page = page;
        PageSize = pageSize;
        TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
    }

    public static PagedResult<TValue> Success(IReadOnlyList<TValue> items, int totalCount, int page, int pageSize, Dictionary<string, object>? metadata = null)
    {
        return new PagedResult<TValue>(items, totalCount, page, pageSize, true, null, null, null, metadata?.AsReadOnly());
    }

    public new static PagedResult<TValue> Failure(string error, string? errorCode = null, Dictionary<string, object>? metadata = null)
    {
        return new PagedResult<TValue>(Array.Empty<TValue>().ToList().AsReadOnly(), 0, 1, 10, false, error, errorCode, null, metadata?.AsReadOnly());
    }
}