using CRM.Identity.Application.Common.Models.Grids;
using CRM.Identity.Application.Common.Services.Grids;
using CRM.Identity.Application.Common.Specifications.Affiliates;
using CRM.Identity.Domain.Entities.Affiliate;

namespace CRM.Identity.Application.Features.Affiliates.Queries.GetAffiliates;

public sealed class GetAffiliatesQuery : GridQueryBase, IRequest<GridResponse<AffiliateDto>>
{
}

public sealed record AffiliateDto(
    Guid Id,
    string Name,
    string Email,
    string? Phone,
    string? Website,
    bool IsActive,
    DateTimeOffset CreatedAt);

public sealed class GetAffiliatesQueryHandler(
    IRepository<Affiliate> affiliateRepository, IGridService gridService) : IRequestHandler<GetAffiliatesQuery, GridResponse<AffiliateDto>>
{
    public async ValueTask<Result<GridResponse<AffiliateDto>>> Handle(GetAffiliatesQuery request, CancellationToken cancellationToken)
    {
        var query = affiliateRepository.GetQueryable();
        
        var result = await gridService.ProcessGridQuery(
            query, 
            request, 
            affiliate => new AffiliateDto(
            affiliate.Id,
            affiliate.Name,
            affiliate.Email,
            affiliate.Phone,
            affiliate.Website,
            affiliate.IsActive,
            affiliate.CreatedAt));

        return Result.Success(result);
    }
}
