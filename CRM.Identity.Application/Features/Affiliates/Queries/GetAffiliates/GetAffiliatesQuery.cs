using CRM.Identity.Application.Common.Specifications.Affiliates;
using CRM.Identity.Domain.Entities.Affiliate;

namespace CRM.Identity.Application.Features.Affiliates.Queries.GetAffiliates;

public sealed record GetAffiliatesQuery(
    string? SearchTerm,
    bool? IsActive,
    int PageNumber = 1,
    int PageSize = 10) : IRequest<GetAffiliatesQueryResponse>;

public sealed record GetAffiliatesQueryResponse(
    List<AffiliateDto> Affiliates,
    int TotalCount,
    int PageNumber,
    int PageSize);

public sealed record AffiliateDto(
    Guid Id,
    string Name,
    string Email,
    string? Phone,
    string? Website,
    bool IsActive,
    DateTimeOffset CreatedAt);

public sealed class GetAffiliatesQueryHandler(
    IRepository<Affiliate> affiliateRepository) : IRequestHandler<GetAffiliatesQuery, GetAffiliatesQueryResponse>
{
    public async ValueTask<Result<GetAffiliatesQueryResponse>> Handle(GetAffiliatesQuery request, CancellationToken cancellationToken)
    {
        var specification = new AffiliatesFilterSpecification(
            request.SearchTerm,
            request.IsActive,
            request.PageNumber,
            request.PageSize);

        var affiliates = await affiliateRepository.ListAsync(specification, cancellationToken);
        var totalCount = await affiliateRepository.CountAsync(new AffiliatesCountSpecification(request.SearchTerm, request.IsActive), cancellationToken);

        var affiliateDtos = affiliates.Select(a => new AffiliateDto(
            a.Id,
            a.Name,
            a.Email,
            a.Phone,
            a.Website,
            a.IsActive,
            a.CreatedAt)).ToList();

        var response = new GetAffiliatesQueryResponse(
            affiliateDtos,
            totalCount,
            request.PageNumber,
            request.PageSize);

        return Result.Success(response);
    }
}
