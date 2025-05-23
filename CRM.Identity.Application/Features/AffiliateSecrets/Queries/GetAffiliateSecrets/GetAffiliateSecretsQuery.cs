using CRM.Identity.Application.Common.Specifications.AffiliateSecrets;
using CRM.Identity.Domain.Entities.Affiliate;

namespace CRM.Identity.Application.Features.AffiliateSecrets.Queries.GetAffiliateSecrets;

public sealed record GetAffiliateSecretsQuery(
    Guid? AffiliateId,
    bool? IsActive,
    bool? IsExpired,
    int PageNumber = 1,
    int PageSize = 10) : IRequest<GetAffiliateSecretsQueryResponse>;

public sealed record GetAffiliateSecretsQueryResponse(
    List<AffiliateSecretDto> AffiliateSecrets,
    int TotalCount,
    int PageNumber,
    int PageSize);

public sealed record AffiliateSecretDto(
    Guid Id,
    Guid AffiliateId,
    string? AffiliateName,
    string SecretKey,
    DateTimeOffset ExpirationDate,
    string? IpRestriction,
    bool IsActive,
    int UsedCount,
    bool IsExpired,
    DateTimeOffset CreatedAt);

public sealed class GetAffiliateSecretsQueryHandler(
    IRepository<AffiliateSecret> affiliateSecretRepository)
    : IRequestHandler<GetAffiliateSecretsQuery, GetAffiliateSecretsQueryResponse>
{
    public async ValueTask<Result<GetAffiliateSecretsQueryResponse>> Handle(GetAffiliateSecretsQuery request,
        CancellationToken cancellationToken)
    {
        var specification = new AffiliateSecretsFilterSpecification(
            request.AffiliateId,
            request.IsActive,
            request.IsExpired,
            request.PageNumber,
            request.PageSize,
            includeAffiliate: true);


        var affiliateSecrets = await affiliateSecretRepository.ListAsync(specification, cancellationToken);
        var totalCount = await affiliateSecretRepository.CountAsync(new AffiliateSecretsCountSpecification(
            request.AffiliateId,
            request.IsActive,
            request.IsExpired), cancellationToken);

        var affiliateSecretDtos = affiliateSecrets.Select(s => new AffiliateSecretDto(
            s.Id,
            s.AffiliateId,
            $"{s.Affiliate?.User!.FirstName} {s.Affiliate?.User!.LastName}",
            s.SecretKey,
            s.ExpirationDate,
            s.IpRestriction,
            s.IsActive,
            s.UsedCount,
            s.IsExpired(),
            s.CreatedAt)).ToList();

        var response = new GetAffiliateSecretsQueryResponse(
            affiliateSecretDtos,
            totalCount,
            request.PageNumber,
            request.PageSize);

        return Result.Success(response);
    }
}