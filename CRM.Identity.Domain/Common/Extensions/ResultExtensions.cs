namespace CRM.Identity.Domain.Common.Extensions;

public static class ResultExtensions
{
    public static async Task<Result<TNew>> MapAsync<TValue, TNew>(this Task<Result<TValue>> resultTask, Func<TValue, TNew> func)
    {
        var result = await resultTask.ConfigureAwait(false);
        return result.Map(func);
    }

    public static async Task<Result<TNew>> MapAsync<TValue, TNew>(this Result<TValue> result, Func<TValue, Task<TNew>> func)
    {
        if (result.IsFailure)
            return Result.Failure<TNew>(result.Error!, result.ErrorCode);

        try
        {
            var value = await func(result.Value!).ConfigureAwait(false);
            return Result.Success(value);
        }
        catch (Exception ex)
        {
            return Result.Failure<TNew>(ex.Message);
        }
    }

    public static async Task<Result<TNew>> MapAsync<TValue, TNew>(this Task<Result<TValue>> resultTask, Func<TValue, Task<TNew>> func)
    {
        var result = await resultTask.ConfigureAwait(false);
        return await result.MapAsync(func).ConfigureAwait(false);
    }

    public static async Task<Result<TNew>> BindAsync<TValue, TNew>(this Task<Result<TValue>> resultTask, Func<TValue, Result<TNew>> func)
    {
        var result = await resultTask.ConfigureAwait(false);
        return result.Bind(func);
    }

    public static async Task<Result<TNew>> BindAsync<TValue, TNew>(this Result<TValue> result, Func<TValue, Task<Result<TNew>>> func)
    {
        if (result.IsFailure)
            return Result.Failure<TNew>(result.Error!, result.ErrorCode);

        try
        {
            return await func(result.Value!).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            return Result.Failure<TNew>(ex.Message);
        }
    }

    public static async Task<Result<TNew>> BindAsync<TValue, TNew>(this Task<Result<TValue>> resultTask, Func<TValue, Task<Result<TNew>>> func)
    {
        var result = await resultTask.ConfigureAwait(false);
        return await result.BindAsync(func).ConfigureAwait(false);
    }

    public static T Match<TValue, T>(this Result<TValue> result, Func<TValue, T> onSuccess, Func<string, T> onFailure)
    {
        return result.IsSuccess ? onSuccess(result.Value!) : onFailure(result.Error!);
    }

    public static async Task<T> MatchAsync<TValue, T>(this Task<Result<TValue>> resultTask, Func<TValue, T> onSuccess, Func<string, T> onFailure)
    {
        var result = await resultTask.ConfigureAwait(false);
        return result.Match(onSuccess, onFailure);
    }

    public static async Task<T> MatchAsync<TValue, T>(this Task<Result<TValue>> resultTask, Func<TValue, Task<T>> onSuccess, Func<string, Task<T>> onFailure)
    {
        var result = await resultTask.ConfigureAwait(false);
        return result.IsSuccess
            ? await onSuccess(result.Value!).ConfigureAwait(false)
            : await onFailure(result.Error!).ConfigureAwait(false);
    }
}