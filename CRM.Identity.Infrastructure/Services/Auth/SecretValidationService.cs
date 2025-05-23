using System.Buffers;
using CRM.Identity.Application.Common.Specifications.AffiliateSecrets;
using CRM.Identity.Domain.Entities.Affiliate;
using Microsoft.Extensions.Caching.Memory;

namespace CRM.Identity.Infrastructure.Services.Auth;

public sealed class SecretValidationService(
    IRepository<AffiliateSecret> repository,
    IUsageTracker tracker,
    IMemoryCache cache,
    ILogger<SecretValidationService> logger)
    : ISecretValidationService
{
    private readonly ILogger<SecretValidationService> _logger = logger;

    private static readonly SearchValues<char> IpSeparators = SearchValues.Create([',', ';', '|']);

    public async ValueTask<Result<ValidatedSecret>> ValidateAsync(
        string secretKey,
        string? clientIp = null,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = $"secret:{secretKey}";

        if (cache.TryGetValue<ValidatedSecret?>(cacheKey, out var cached))
        {
            if (cached.HasValue)
            {
                _ = tracker.TrackAsync(cached.Value.SecretId, clientIp);
                return Result.Success(cached.Value);
            }

            return Result.Failure<ValidatedSecret>("Invalid secret", "Unauthorized");
        }

        var spec = new SecretByKeySpecification(secretKey);
        var secret = await repository.FirstOrDefaultAsync(spec, cancellationToken);

        if (secret is null)
        {
            cache.Set(cacheKey, (ValidatedSecret?)null, TimeSpan.FromMinutes(2));
            return Result.Failure<ValidatedSecret>("Invalid secret", "Unauthorized");
        }

        if (!secret.IsActive || secret.IsExpired())
        {
            cache.Set(cacheKey, (ValidatedSecret?)null, TimeSpan.FromMinutes(1));
            return Result.Failure<ValidatedSecret>("Secret expired", "Unauthorized");
        }

        if (!string.IsNullOrEmpty(secret.IpRestriction) &&
            !string.IsNullOrEmpty(clientIp) &&
            !IsIpAllowed(clientIp, secret.IpRestriction))
        {
            return Result.Failure<ValidatedSecret>("IP not allowed", "Forbidden");
        }

        var validated = new ValidatedSecret(
            secret.Id,
            secret.AffiliateId,
            $"{secret.Affiliate?.User?.FirstName} {secret.Affiliate?.User?.LastName}".Trim());

        cache.Set(cacheKey, validated, TimeSpan.FromMinutes(15));
        _ = tracker.TrackAsync(secret.Id, clientIp);

        return Result.Success(validated);
    }

    private static bool IsIpAllowed(string clientIp, string allowedIps)
    {
        var allowedSpan = allowedIps.AsSpan();
        var clientSpan = clientIp.AsSpan();

        while (allowedSpan.Length > 0)
        {
            var separatorIndex = allowedSpan.IndexOfAny(IpSeparators);
            var ipSpan = separatorIndex >= 0 ? allowedSpan[..separatorIndex] : allowedSpan;

            ipSpan = ipSpan.Trim();
            if (ipSpan.SequenceEqual(clientSpan) || ipSpan.SequenceEqual("*"))
                return true;

            if (separatorIndex < 0) break;
            allowedSpan = allowedSpan[(separatorIndex + 1)..];
        }

        return false;
    }
}