using CRM.Identity.Application.Common.Specifications.AffiliateSecrets;
using CRM.Identity.Domain.Entities.Affiliate;

namespace CRM.Identity.Application.Features.AffiliateSecrets.Queries.GetAffiliateSecretById;

public sealed record GetAffiliateSecretByIdQuery(Guid Id) : IRequest<AffiliateSecretDetailDto>;

public sealed record AffiliateSecretDetailDto(
    Guid Id,
    Guid AffiliateId,
    string? AffiliateName,
    string? AffiliateEmail,
    string SecretKey,
    string ApiKey,
    DateTimeOffset ExpirationDate,
    string? IpRestriction,
    bool IsActive,
    int UsedCount,
    bool IsExpired,
    DateTimeOffset CreatedAt,
    DateTimeOffset? LastModifiedAt);

public sealed class GetAffiliateSecretByIdQueryHandler(
    IRepository<AffiliateSecret> affiliateSecretRepository)
    : IRequestHandler<GetAffiliateSecretByIdQuery, AffiliateSecretDetailDto>
{
    public async ValueTask<Result<AffiliateSecretDetailDto>> Handle(GetAffiliateSecretByIdQuery request,
        CancellationToken cancellationToken)
    {
        var specification = new AffiliateSecretByIdWithAffiliateSpecification(request.Id);
        var affiliateSecret = await affiliateSecretRepository.FirstOrDefaultAsync(specification, cancellationToken);

        if (affiliateSecret == null)
        {
            return Result.Failure<AffiliateSecretDetailDto>("Affiliate secret not found", "NotFound");
        }

        var response = new AffiliateSecretDetailDto(
            affiliateSecret.Id,
            affiliateSecret.AffiliateId,
            affiliateSecret.Affiliate?.Name,
            affiliateSecret.Affiliate?.Email,
            affiliateSecret.SecretKey,
            affiliateSecret.ApiKey,
            affiliateSecret.ExpirationDate,
            affiliateSecret.IpRestriction,
            affiliateSecret.IsActive,
            affiliateSecret.UsedCount,
            affiliateSecret.IsExpired(),
            affiliateSecret.CreatedAt,
            affiliateSecret.LastModifiedAt);

        return Result.Success(response);
    }
}