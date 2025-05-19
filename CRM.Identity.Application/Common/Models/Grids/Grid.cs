namespace CRM.Identity.Application.Common.Models.Grids;

public class GridQueryBase
{
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SortField { get; set; }
    public string? SortDirection { get; set; }
    public string[]? VisibleColumns { get; set; }
    public string? GlobalFilter { get; set; }
    public Dictionary<string, GridFilterItem>? Filters { get; set; }
}

public class GridFilterItem
{
    public string Field { get; set; } = string.Empty;
    public string Operator { get; set; } = string.Empty;
    public object? Value { get; set; }
    public object[]? Values { get; set; }
}

public class GridResponse<T>
{
    public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();
    public int TotalCount { get; set; }
    public int PageIndex { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public bool HasPreviousPage => PageIndex > 0;
    public bool HasNextPage => PageIndex < TotalPages - 1;
}