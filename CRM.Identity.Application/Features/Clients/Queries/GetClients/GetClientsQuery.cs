using CRM.Identity.Application.Common.Specifications.Clients;
using CRM.Identity.Domain.Entities.Clients;
using CRM.Identity.Domain.Entities.Clients.Enums;

namespace CRM.Identity.Application.Features.Clients.Queries.GetClients;

public sealed record GetClientsQuery(
    string? SearchTerm,
    Guid? AffiliateId,
    ClientStatus? Status,
    bool? IsProblematic,
    bool? IsBonusAbuser,
    int PageNumber = 1,
    int PageSize = 10) : IRequest<GetClientsQueryResponse>;

public sealed record GetClientsQueryResponse(
    List<ClientDto> Clients,
    int TotalCount,
    int PageNumber,
    int PageSize);

public sealed record ClientDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string? Telephone,
    string? Country,
    ClientStatus Status,
    bool IsProblematic,
    bool IsBonusAbuser,
    Guid AffiliateId,
    string? AffiliateName,
    DateTime? FTDTime,
    DateTime RegistrationDate);

public sealed class GetClientsQueryHandler(
    IRepository<Client> clientRepository) : IRequestHandler<GetClientsQuery, GetClientsQueryResponse>
{
    public async ValueTask<Result<GetClientsQueryResponse>> Handle(GetClientsQuery request,
        CancellationToken cancellationToken)
    {
        var specification = new ClientsFilterSpecification(
            request.SearchTerm,
            request.AffiliateId,
            request.Status,
            request.IsProblematic,
            request.IsBonusAbuser,
            request.PageNumber,
            request.PageSize,
            includeAffiliate: true);


        var clients = await clientRepository.ListAsync(specification, cancellationToken);
        var totalCount = await clientRepository.CountAsync(new ClientsCountSpecification(
            request.SearchTerm,
            request.AffiliateId,
            request.Status,
            request.IsProblematic,
            request.IsBonusAbuser), cancellationToken);

        var clientDtos = clients.Select(c => new ClientDto(
            c.Id,
            c.FirstName,
            c.LastName,
            c.Email,
            c.Telephone,
            c.Country,
            c.Status,
            c.IsProblematic,
            c.IsBonusAbuser,
            c.AffiliateId,
            c.Affiliate?.Name,
            c.FTDTime,
            c.RegistrationDate)).ToList();

        var response = new GetClientsQueryResponse(
            clientDtos,
            totalCount,
            request.PageNumber,
            request.PageSize);

        return Result.Success(response);
    }
}