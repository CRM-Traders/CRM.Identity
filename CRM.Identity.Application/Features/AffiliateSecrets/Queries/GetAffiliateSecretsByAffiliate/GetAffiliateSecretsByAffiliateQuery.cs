using CRM.Identity.Application.Common.Specifications.AffiliateSecrets;
using CRM.Identity.Application.Features.AffiliateSecrets.Queries.GetAffiliateSecrets;
using CRM.Identity.Domain.Entities.Affiliate;

namespace CRM.Identity.Application.Features.AffiliateSecrets.Queries.GetAffiliateSecretsByAffiliate;

public sealed record GetAffiliateSecretsByAffiliateQuery(Guid AffiliateId) : IRequest<List<AffiliateSecretDto>>;

public sealed class GetAffiliateSecretsByAffiliateQueryHandler(
    IRepository<AffiliateSecret> affiliateSecretRepository) : IRequestHandler<GetAffiliateSecretsByAffiliateQuery, List<AffiliateSecretDto>>
{
    public async ValueTask<Result<List<AffiliateSecretDto>>> Handle(GetAffiliateSecretsByAffiliateQuery request, CancellationToken cancellationToken)
    {
        var specification = new AffiliateSecretsByAffiliateSpecification(request.AffiliateId,includeAffiliate: true);

        var affiliateSecrets = await affiliateSecretRepository.ListAsync(specification, cancellationToken);

        var affiliateSecretDtos = affiliateSecrets.Select(s => new AffiliateSecretDto(
            s.Id,
            s.AffiliateId,
            s.Affiliate?.Name,
            s.SecretKey,
            s.ApiKey,
            s.ExpirationDate,
            s.IpRestriction,
            s.IsActive,
            s.UsedCount,
            s.IsExpired(),
            s.CreatedAt)).ToList();

        return Result.Success(affiliateSecretDtos);
    }
}