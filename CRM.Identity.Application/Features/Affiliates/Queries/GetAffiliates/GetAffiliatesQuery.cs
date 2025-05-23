using CRM.Identity.Application.Common.Models.Grids;
using CRM.Identity.Application.Common.Services.Grids;
using CRM.Identity.Domain.Entities.Affiliate;
using Microsoft.EntityFrameworkCore;

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
    string? UserFullName,
    DateTimeOffset CreatedAt);

public sealed class GetAffiliatesQueryHandler(
    IRepository<Affiliate> affiliateRepository,
    IGridService gridService) : IRequestHandler<GetAffiliatesQuery, GridResponse<AffiliateDto>>
{
    public async ValueTask<Result<GridResponse<AffiliateDto>>> Handle(GetAffiliatesQuery request,
        CancellationToken cancellationToken)
    {
        var query = affiliateRepository.GetQueryable()
            .Include(a => a.User);

        var result = await gridService.ProcessGridQuery(
            query,
            request,
            affiliate => new AffiliateDto(
                affiliate.Id,
                affiliate.User!.FirstName,
                affiliate.User.Email,
                affiliate.Phone,
                affiliate.Website,
                affiliate.IsActive,
                affiliate.User != null ? $"{affiliate.User.FirstName} {affiliate.User.LastName}" : null,
                affiliate.CreatedAt), cancellationToken);

        return Result.Success(result);
    }
}