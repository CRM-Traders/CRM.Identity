using CRM.Identity.Application.Common.Models.Grids;
using CRM.Identity.Application.Common.Services.Grids;
using CRM.Identity.Domain.Entities.Clients;
using CRM.Identity.Domain.Entities.Clients.Enums;
using Microsoft.EntityFrameworkCore;

namespace CRM.Identity.Application.Features.Clients.Queries.ClientsGrid;

public sealed class ClientsGridQuery : GridQueryBase, IRequest<GridResponse<ClientsGridQueryDto>>
{
}

public sealed record ClientsGridQueryDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string? Telephone,
    string? Country,
    string? Language,
    ClientStatus Status,
    string? SalesStatus,
    bool IsProblematic,
    bool IsBonusAbuser,
    bool HasInvestments,
    Guid AffiliateId,
    string? AffiliateName,
    DateTime? FTDTime,
    DateTime? LTDTime,
    DateTime RegistrationDate,
    DateTime? LastLogin);

public sealed class ClientsGridQueryHandler(
    IRepository<Client> clientRepository,
    IGridService gridService) : IRequestHandler<ClientsGridQuery, GridResponse<ClientsGridQueryDto>>
{
    public async ValueTask<Result<GridResponse<ClientsGridQueryDto>>> Handle(
        ClientsGridQuery request,
        CancellationToken cancellationToken)
    {
        var query = clientRepository.GetQueryable()
            .Include(c => c.Affiliate!)
            .ThenInclude(a => a.User!);

        var result = await gridService.ProcessGridQuery(
            query,
            request,
            client => new ClientsGridQueryDto(
                client.Id,
                client.FirstName,
                client.LastName,
                client.Email,
                client.Telephone,
                client.Country,
                client.Language,
                client.Status,
                client.SalesStatus,
                client.IsProblematic,
                client.IsBonusAbuser,
                client.HasInvestments,
                client.AffiliateId,
                $"{client.Affiliate?.User?.FirstName} {client.Affiliate?.User?.LastName}".Trim(),
                client.FTDTime,
                client.LTDTime,
                client.RegistrationDate,
                client.LastLogin),
            cancellationToken);

        return Result.Success(result);
    }
}