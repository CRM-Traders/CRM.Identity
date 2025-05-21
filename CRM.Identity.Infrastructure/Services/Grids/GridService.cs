using System.Linq.Expressions;
using CRM.Identity.Application.Common.Models.Grids;
using CRM.Identity.Application.Common.Services.Grids;
using Microsoft.EntityFrameworkCore;

namespace CRM.Identity.Infrastructure.Services.Grids;

public class GridService : IGridService
{
    public async Task<GridResponse<TResult>> ProcessGridQuery<TEntity, TResult>(
        IQueryable<TEntity> query,
        GridQueryBase gridQuery,
        Func<TEntity, TResult> selector,
        CancellationToken cancellationToken = default)
        where TEntity : class
        where TResult : class
    {
        try
        {
            var searchableProperties = typeof(TEntity)
                .GetProperties()
                .Where(p => p.CanRead)
                .Select(p => p.Name)
                .ToArray();

            if (gridQuery.Filters?.Count > 0)
            {
                query = ApplyFilters(query, gridQuery.Filters);
            }

            if (!string.IsNullOrEmpty(gridQuery.GlobalFilter))
            {
                query = ApplyGlobalFilter(query, gridQuery.GlobalFilter, searchableProperties, gridQuery.VisibleColumns);
            }

            int totalCount = await query.CountAsync(cancellationToken);

            if (!string.IsNullOrEmpty(gridQuery.SortField))
            {
                query = ApplySorting(query, gridQuery.SortField, gridQuery.SortDirection ?? "asc");
            }

            query = ApplyPagination(query, gridQuery.PageIndex, gridQuery.PageSize);

            var items = await query
                .Select(entity => selector(entity))
                .ToListAsync(cancellationToken);

            int totalPages = (int)Math.Ceiling(totalCount / (double)gridQuery.PageSize);

            return new GridResponse<TResult>
            {
                Items = items,
                TotalCount = totalCount,
                PageIndex = gridQuery.PageIndex,
                PageSize = gridQuery.PageSize,
                TotalPages = totalPages
            };
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    public IQueryable<T> ApplyFilters<T>(
        IQueryable<T> query,
        Dictionary<string, GridFilterItem> filters) where T : class
    {
        foreach (var filter in filters.Values)
        {
            query = filter.Operator switch
            {
                "eq" => ApplyEquals(query, filter),
                "notEquals" => ApplyNotEquals(query, filter),
                "contains" => ApplyContains(query, filter),
                "startsWith" => ApplyStartsWith(query, filter),
                "endsWith" => ApplyEndsWith(query, filter),
                "greaterThan" => ApplyGreaterThan(query, filter),
                "greaterThanOrEquals" => ApplyGreaterThanOrEquals(query, filter),
                "lessThan" => ApplyLessThan(query, filter),
                "lessThanOrEquals" => ApplyLessThanOrEquals(query, filter),
                "in" => ApplyIn(query, filter),
                "between" => ApplyBetween(query, filter),
                "isNull" => ApplyIsNull(query, filter),
                "isNotNull" => ApplyIsNotNull(query, filter),
                _ => query
            };
        }

        return query;
    }

    public IQueryable<T> ApplyGlobalFilter<T>(
        IQueryable<T> query,
        string globalFilter,
        string[] searchableProperties,
        string[]? visibleColumns = null) where T : class
    {
        var propertiesToSearch = visibleColumns != null && visibleColumns.Length > 0
            ? searchableProperties.Intersect(visibleColumns, StringComparer.OrdinalIgnoreCase).ToArray()
            : searchableProperties;

        var parameter = Expression.Parameter(typeof(T), "x");
        Expression? combinedExpression = null;

        foreach (var propertyName in propertiesToSearch)
        {
            var propertyInfo = typeof(T).GetProperty(propertyName);
            if (propertyInfo == null) continue;

            var property = Expression.Property(parameter, propertyName);
            var propertyType = propertyInfo.PropertyType;

            var underlyingType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

            Expression? propertyExpression = null;

            if (underlyingType == typeof(string))
            {
                var propertyNotNull = Expression.NotEqual(property, Expression.Constant(null, propertyType));
                var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                var constant = Expression.Constant(globalFilter, typeof(string));
                var stringContains = Expression.Call(property, containsMethod!, constant);

                propertyExpression = Expression.AndAlso(propertyNotNull, stringContains);
            }
            else if (underlyingType == typeof(bool))
            {
                if (bool.TryParse(globalFilter, out var boolValue))
                {
                    var constant = Expression.Constant(boolValue, typeof(bool));
                    if (propertyType != typeof(bool))
                    {
                        var hasValueProperty = Expression.Property(property, "HasValue");
                        var valueProperty = Expression.Property(property, "Value");
                        var hasValue = Expression.IsTrue(hasValueProperty);
                        var valueEquals = Expression.Equal(valueProperty, constant);
                        propertyExpression = Expression.AndAlso(hasValue, valueEquals);
                    }
                    else
                    {
                        propertyExpression = Expression.Equal(property, constant);
                    }
                }
            }
            else if (underlyingType == typeof(int) || underlyingType == typeof(long) ||
                     underlyingType == typeof(decimal) || underlyingType == typeof(double) ||
                     underlyingType == typeof(float))
            {
                if (decimal.TryParse(globalFilter, out var numericValue))
                {
                    object convertedValue;
                    try
                    {
                        convertedValue = Convert.ChangeType(numericValue, underlyingType);
                    }
                    catch
                    {
                        continue;
                    }

                    if (propertyType != underlyingType)
                    {
                        var hasValueProperty = Expression.Property(property, "HasValue");
                        var valueProperty = Expression.Property(property, "Value");
                        var hasValue = Expression.IsTrue(hasValueProperty);
                        var valueEquals = Expression.Equal(valueProperty, Expression.Constant(convertedValue, underlyingType));
                        propertyExpression = Expression.AndAlso(hasValue, valueEquals);
                    }
                    else
                    {
                        propertyExpression = Expression.Equal(property, Expression.Constant(convertedValue, underlyingType));
                    }
                }
            }
            else if (underlyingType == typeof(DateTime))
            {
                if (DateTime.TryParse(globalFilter, out var dateValue))
                {
                    if (propertyType != typeof(DateTime))
                    {
                        var hasValueProperty = Expression.Property(property, "HasValue");
                        var valueProperty = Expression.Property(property, "Value");
                        var hasValue = Expression.IsTrue(hasValueProperty);
                        var valueEquals = Expression.Equal(valueProperty, Expression.Constant(dateValue, typeof(DateTime)));
                        propertyExpression = Expression.AndAlso(hasValue, valueEquals);
                    }
                    else
                    {
                        propertyExpression = Expression.Equal(property, Expression.Constant(dateValue, typeof(DateTime)));
                    }
                }
            }

            if (propertyExpression != null)
            {
                combinedExpression = combinedExpression == null
                    ? propertyExpression
                    : Expression.OrElse(combinedExpression, propertyExpression);
            }
        }

        if (combinedExpression != null)
        {
            var lambda = Expression.Lambda<Func<T, bool>>(combinedExpression, parameter);
            return query.Where(lambda);
        }

        return query;
    }

    public IQueryable<T> ApplySorting<T>(
        IQueryable<T> query,
        string sortField,
        string sortDirection) where T : class
    {
        var parameter = Expression.Parameter(typeof(T), "x");
        var property = Expression.Property(parameter, sortField);
        var lambda = Expression.Lambda(property, parameter);

        var method = sortDirection.ToLower() == "desc"
            ? "OrderByDescending"
            : "OrderBy";

        var genericMethod = typeof(Queryable).GetMethods()
            .Where(m => m.Name == method && m.IsGenericMethodDefinition)
            .FirstOrDefault(m => m.GetParameters().Length == 2)
            ?.MakeGenericMethod(typeof(T), property.Type);

        if (genericMethod != null)
        {
            return (IQueryable<T>)genericMethod.Invoke(null, new object[] { query, lambda })!;
        }

        return query;
    }

    public IQueryable<T> ApplyPagination<T>(
        IQueryable<T> query,
        int pageIndex,
        int pageSize) where T : class
    {
        return query.Skip(pageIndex * pageSize).Take(pageSize);
    }

    #region Filter Helpers

    public static object GetValue(string input)
    {
        if (int.TryParse(input, out var intResult))
            return intResult;

        if (double.TryParse(input, out var doubleResult))
            return doubleResult;

        if (bool.TryParse(input, out var boolResult))
            return boolResult;

        if (DateTimeOffset.TryParse(input, out var dateResult))
            return dateResult;

        return input;
    }

    private IQueryable<T> ApplyEquals<T>(IQueryable<T> query, GridFilterItem filter) where T : class
    {
        var parameter = Expression.Parameter(typeof(T), "x");
        var property = Expression.Property(parameter, filter.Field);
        var constant = Expression.Constant(GetValue(filter.Value!.ToString()!));
        var equals = Expression.Equal(property, constant);
        var lambda = Expression.Lambda<Func<T, bool>>(equals, parameter);
        return query.Where(lambda);
    }

    private IQueryable<T> ApplyNotEquals<T>(IQueryable<T> query, GridFilterItem filter) where T : class
    {
        var parameter = Expression.Parameter(typeof(T), "x");
        var property = Expression.Property(parameter, filter.Field);
        var constant = Expression.Constant(GetValue(filter.Value!.ToString()!));
        var equals = Expression.NotEqual(property, constant);
        var lambda = Expression.Lambda<Func<T, bool>>(equals, parameter);
        return query.Where(lambda);
    }

    private IQueryable<T> ApplyContains<T>(IQueryable<T> query, GridFilterItem filter) where T : class
    {
        var parameter = Expression.Parameter(typeof(T), "x");
        var property = Expression.Property(parameter, filter.Field);
        var propertyType = typeof(T).GetProperty(filter.Field)?.PropertyType;

        if (propertyType == typeof(string))
        {
            var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
            var constant = Expression.Constant(filter.Value?.ToString());
            var call = Expression.Call(property, containsMethod!, constant);
            var lambda = Expression.Lambda<Func<T, bool>>(call, parameter);
            return query.Where(lambda);
        }
        else
        {
            var toString = Expression.Call(property,
                typeof(object).GetMethod("ToString") ?? throw new InvalidOperationException("ToString method not found"));
            var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
            var constant = Expression.Constant(filter.Value?.ToString());
            var call = Expression.Call(toString, containsMethod!, constant);
            var lambda = Expression.Lambda<Func<T, bool>>(call, parameter);
            return query.Where(lambda);
        }
    }

    private IQueryable<T> ApplyStartsWith<T>(IQueryable<T> query, GridFilterItem filter) where T : class
    {
        var parameter = Expression.Parameter(typeof(T), "x");
        var property = Expression.Property(parameter, filter.Field);
        var startsWithMethod = typeof(string).GetMethod("StartsWith", new[] { typeof(string) });
        var constant = Expression.Constant(filter.Value?.ToString());
        var call = Expression.Call(property, startsWithMethod!, constant);
        var lambda = Expression.Lambda<Func<T, bool>>(call, parameter);
        return query.Where(lambda);
    }

    private IQueryable<T> ApplyEndsWith<T>(IQueryable<T> query, GridFilterItem filter) where T : class
    {
        var parameter = Expression.Parameter(typeof(T), "x");
        var property = Expression.Property(parameter, filter.Field);
        var endsWithMethod = typeof(string).GetMethod("EndsWith", new[] { typeof(string) });
        var constant = Expression.Constant(filter.Value?.ToString());
        var call = Expression.Call(property, endsWithMethod!, constant);
        var lambda = Expression.Lambda<Func<T, bool>>(call, parameter);
        return query.Where(lambda);
    }

    private IQueryable<T> ApplyGreaterThan<T>(IQueryable<T> query, GridFilterItem filter) where T : class
    {
        var parameter = Expression.Parameter(typeof(T), "x");
        var property = Expression.Property(parameter, filter.Field);
        var constant = Expression.Constant(filter.Value);
        var greaterThan = Expression.GreaterThan(property, constant);
        var lambda = Expression.Lambda<Func<T, bool>>(greaterThan, parameter);
        return query.Where(lambda);
    }

    private IQueryable<T> ApplyGreaterThanOrEquals<T>(IQueryable<T> query, GridFilterItem filter) where T : class
    {
        var parameter = Expression.Parameter(typeof(T), "x");
        var property = Expression.Property(parameter, filter.Field);
        var constant = Expression.Constant(filter.Value);
        var greaterThanOrEqual = Expression.GreaterThanOrEqual(property, constant);
        var lambda = Expression.Lambda<Func<T, bool>>(greaterThanOrEqual, parameter);
        return query.Where(lambda);
    }

    private IQueryable<T> ApplyLessThan<T>(IQueryable<T> query, GridFilterItem filter) where T : class
    {
        var parameter = Expression.Parameter(typeof(T), "x");
        var property = Expression.Property(parameter, filter.Field);
        var constant = Expression.Constant(filter.Value);
        var lessThan = Expression.LessThan(property, constant);
        var lambda = Expression.Lambda<Func<T, bool>>(lessThan, parameter);
        return query.Where(lambda);
    }

    private IQueryable<T> ApplyLessThanOrEquals<T>(IQueryable<T> query, GridFilterItem filter) where T : class
    {
        var parameter = Expression.Parameter(typeof(T), "x");
        var property = Expression.Property(parameter, filter.Field);
        var constant = Expression.Constant(filter.Value);
        var lessThanOrEqual = Expression.LessThanOrEqual(property, constant);
        var lambda = Expression.Lambda<Func<T, bool>>(lessThanOrEqual, parameter);
        return query.Where(lambda);
    }

    private IQueryable<T> ApplyIn<T>(IQueryable<T> query, GridFilterItem filter) where T : class
    {
        var parameter = Expression.Parameter(typeof(T), "x");
        var property = Expression.Property(parameter, filter.Field);
        var containsMethod = typeof(Enumerable).GetMethods()
            .First(m => m.Name == "Contains" && m.GetParameters().Length == 2)
            .MakeGenericMethod(property.Type);
        var constant = Expression.Constant(filter.Values);
        var call = Expression.Call(containsMethod, constant, property);
        var lambda = Expression.Lambda<Func<T, bool>>(call, parameter);
        return query.Where(lambda);
    }

    private IQueryable<T> ApplyBetween<T>(IQueryable<T> query, GridFilterItem filter) where T : class
    {
        if (filter.Values?.Length != 2)
            return query;

        var parameter = Expression.Parameter(typeof(T), "x");
        var property = Expression.Property(parameter, filter.Field);
        var lowerConstant = Expression.Constant(filter.Values[0]);
        var upperConstant = Expression.Constant(filter.Values[1]);

        var greaterThanOrEqual = Expression.GreaterThanOrEqual(property, lowerConstant);
        var lessThanOrEqual = Expression.LessThanOrEqual(property, upperConstant);
        var between = Expression.AndAlso(greaterThanOrEqual, lessThanOrEqual);

        var lambda = Expression.Lambda<Func<T, bool>>(between, parameter);
        return query.Where(lambda);
    }

    private IQueryable<T> ApplyIsNull<T>(IQueryable<T> query, GridFilterItem filter) where T : class
    {
        var parameter = Expression.Parameter(typeof(T), "x");
        var property = Expression.Property(parameter, filter.Field);
        var isNull = Expression.Equal(property, Expression.Constant(null));
        var lambda = Expression.Lambda<Func<T, bool>>(isNull, parameter);
        return query.Where(lambda);
    }

    private IQueryable<T> ApplyIsNotNull<T>(IQueryable<T> query, GridFilterItem filter) where T : class
    {
        var parameter = Expression.Parameter(typeof(T), "x");
        var property = Expression.Property(parameter, filter.Field);
        var isNotNull = Expression.NotEqual(property, Expression.Constant(null));
        var lambda = Expression.Lambda<Func<T, bool>>(isNotNull, parameter);
        return query.Where(lambda);
    }

    private Expression BuildStringFilterExpression(Expression property, string filterValue)
    {
        var toString = Expression.Call(property,
            typeof(object).GetMethod("ToString") ?? throw new InvalidOperationException("ToString method not found"));
        var containsMethod = typeof(string).GetMethod("Contains",
            new[] { typeof(string) }) ?? throw new InvalidOperationException("Contains method not found");
        var constant = Expression.Constant(filterValue, typeof(string));

        return Expression.Call(toString, containsMethod, constant);
    }

    private Expression BuildNumericFilterExpression(Expression property, string filterValue, Type propertyType)
    {
        if (decimal.TryParse(filterValue, out var numericValue))
        {
            var convertedValue = Convert.ChangeType(numericValue, propertyType);
            var constant = Expression.Constant(convertedValue, propertyType);
            return Expression.Equal(property, constant);
        }

        var toString = Expression.Call(property,
            typeof(object).GetMethod("ToString") ?? throw new InvalidOperationException("ToString method not found"));
        var containsMethod = typeof(string).GetMethod("Contains",
            new[] { typeof(string) }) ?? throw new InvalidOperationException("Contains method not found");
        var stringConstant = Expression.Constant(filterValue, typeof(string));

        return Expression.Call(toString, containsMethod, stringConstant);
    }

    private Expression BuildDateFilterExpression(Expression property, string filterValue)
    {
        if (DateTime.TryParse(filterValue, out var dateValue))
        {
            var constant = Expression.Constant(dateValue, typeof(DateTime));
            return Expression.Equal(property, constant);
        }

        var toString = Expression.Call(property,
            typeof(object).GetMethod("ToString") ?? throw new InvalidOperationException("ToString method not found"));
        var containsMethod = typeof(string).GetMethod("Contains",
            new[] { typeof(string) }) ?? throw new InvalidOperationException("Contains method not found");
        var stringConstant = Expression.Constant(filterValue, typeof(string));

        return Expression.Call(toString, containsMethod, stringConstant);
    }

    private Expression BuildBooleanFilterExpression(Expression property, string filterValue)
    {
        if (bool.TryParse(filterValue, out var boolValue))
        {
            var constant = Expression.Constant(boolValue, typeof(bool));
            return Expression.Equal(property, constant);
        }

        var toString = Expression.Call(property,
            typeof(object).GetMethod("ToString") ?? throw new InvalidOperationException("ToString method not found"));
        var equalsMethod = typeof(string).GetMethod("Equals",
            new[] { typeof(string) }) ?? throw new InvalidOperationException("Equals method not found");
        var stringConstant = Expression.Constant(filterValue.ToLower(), typeof(string));

        return Expression.Call(toString, equalsMethod, stringConstant);
    }

    #endregion
}