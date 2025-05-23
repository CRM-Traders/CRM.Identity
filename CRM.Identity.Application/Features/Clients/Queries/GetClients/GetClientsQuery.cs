using CRM.Identity.Application.Common.Models.Grids;
using CRM.Identity.Application.Common.Services.Grids;
using CRM.Identity.Application.Common.Specifications.Clients;
using CRM.Identity.Domain.Entities.Clients;
using CRM.Identity.Domain.Entities.Clients.Enums;
using Microsoft.EntityFrameworkCore;

namespace CRM.Identity.Application.Features.Clients.Queries.GetClients;

public sealed class GetClientsQuery : GridQueryBase, IRequest<GridResponse<ClientDto>>
{
}

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
    string? UserFullName,
    DateTime? FTDTime,
    DateTime RegistrationDate);

public sealed class GetClientsQueryHandler(
    IRepository<Client> clientRepository,
    IGridService gridService) : IRequestHandler<GetClientsQuery, GridResponse<ClientDto>>
{
    public async ValueTask<Result<GridResponse<ClientDto>>> Handle(GetClientsQuery request,
        CancellationToken cancellationToken)
    {
        var query = clientRepository.GetQueryable()
            .Include(c => c.Affiliate)
            .ThenInclude(x => x!.User)
            .Include(c => c.User);

        var result = await gridService.ProcessGridQuery(
            query,
            request,
            client => new ClientDto(
                client.Id,
                client.FirstName,
                client.LastName,
                client.Email,
                client.Telephone,
                client.Country,
                client.Status,
                client.IsProblematic,
                client.IsBonusAbuser,
                client.AffiliateId,
                $"{client.Affiliate?.User!.FirstName} {client.Affiliate?.User!.LastName}",
                client.User != null ? $"{client.User.FirstName} {client.User.LastName}" : null,
                client.FTDTime,
                client.RegistrationDate), cancellationToken);

        return Result.Success(result);
    }
}