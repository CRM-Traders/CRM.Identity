namespace CRM.Identity.Application.Common.Services.Auth;

public interface ISecretValidationService
{
    ValueTask<Result<ValidatedSecret>> ValidateAsync(string secretKey, string? clientIp = null,
        CancellationToken cancellationToken = default);
}

public interface IUsageTracker
{
    ValueTask TrackAsync(Guid secretId, string? clientIp = null);
}

public readonly record struct ValidatedSecret(
    Guid SecretId,
    Guid AffiliateId,
    string AffiliateName);