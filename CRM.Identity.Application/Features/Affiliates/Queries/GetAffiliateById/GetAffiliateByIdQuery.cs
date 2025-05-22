using CRM.Identity.Application.Common.Specifications.Clients;
using CRM.Identity.Domain.Entities.Affiliate;
using CRM.Identity.Domain.Entities.Clients;

namespace CRM.Identity.Application.Features.Affiliates.Queries.GetAffiliateById;

public sealed record GetAffiliateByIdQuery(Guid Id) : IRequest<AffiliateDetailDto>;

public sealed record AffiliateDetailDto(
    Guid Id,
    string Name,
    string Email,
    string? Phone,
    string? Website,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset? LastModifiedAt,
    int ClientsCount);

public sealed class GetAffiliateByIdQueryHandler(
    IRepository<Affiliate> affiliateRepository,
    IRepository<Client> clientRepository) : IRequestHandler<GetAffiliateByIdQuery, AffiliateDetailDto>
{
    public async ValueTask<Result<AffiliateDetailDto>> Handle(GetAffiliateByIdQuery request,
        CancellationToken cancellationToken)
    {
        var affiliate = await affiliateRepository.GetByIdAsync(request.Id, cancellationToken);
        if (affiliate == null)
        {
            return Result.Failure<AffiliateDetailDto>("Affiliate not found", "NotFound");
        }

        var clientsSpecification = new ClientsByAffiliateSpecification(request.Id);
        var clientsCount = await clientRepository.CountAsync(clientsSpecification, cancellationToken);

        var response = new AffiliateDetailDto(
            affiliate.Id,
            affiliate.Name,
            affiliate.Email,
            affiliate.Phone,
            affiliate.Website,
            affiliate.IsActive,
            affiliate.CreatedAt,
            affiliate.LastModifiedAt,
            clientsCount);

        return Result.Success(response);
    }
}