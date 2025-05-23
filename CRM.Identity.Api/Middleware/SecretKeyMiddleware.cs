using CRM.Identity.Application.Common.Services.Auth;
using CRM.Identity.Infrastructure.Attributes;

namespace CRM.Identity.Api.Middleware;

public sealed class SecretKeyMiddleware(RequestDelegate next)
{
    private static readonly string[] SecretHeaderNames = ["X-Secret-Key", "X-API-Key"];

    public async Task InvokeAsync(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        if (endpoint?.Metadata?.GetMetadata<RequireSecretAttribute>() is null)
        {
            await next(context);
            return;
        }

        var validator = context.RequestServices.GetRequiredService<ISecretValidationService>();

        var secretKey = GetSecretKey(context.Request.Headers);
        if (string.IsNullOrEmpty(secretKey))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Secret key required");
            return;
        }

        var clientIp = GetClientIp(context);
        var result = await validator.ValidateAsync(secretKey, clientIp, context.RequestAborted);

        if (!result.IsSuccess)
        {
            context.Response.StatusCode = result.ErrorCode switch
            {
                "Forbidden" => 403,
                "Unauthorized" => 401,
                _ => 500
            };
            await context.Response.WriteAsync(result.Error ?? "Authentication failed");
            return;
        }

        context.Items["Secret"] = result.Value;
        context.Items["AffiliateId"] = result.Value.AffiliateId;

        await next(context);
    }

    private static string? GetSecretKey(IHeaderDictionary headers)
    {
        foreach (var headerName in SecretHeaderNames)
        {
            if (headers.TryGetValue(headerName, out var values))
            {
                return values.FirstOrDefault();
            }
        }

        return null;
    }

    private static string? GetClientIp(HttpContext context)
    {
#if DEBUG
        return "185.115.4.246";
#endif

        return context.Request.Headers["X-Forwarded-For"].FirstOrDefault()?.Split(',')[0].Trim()
               ?? context.Request.Headers["X-Real-IP"].FirstOrDefault()
               ?? context.Connection.RemoteIpAddress?.ToString();
    }
}