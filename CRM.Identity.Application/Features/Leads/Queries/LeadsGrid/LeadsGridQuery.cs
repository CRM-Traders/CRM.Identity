using CRM.Identity.Application.Common.Models.Grids;
using CRM.Identity.Application.Common.Services.Grids;
using CRM.Identity.Domain.Entities.Clients.Enums;
using CRM.Identity.Domain.Entities.Leads;

namespace CRM.Identity.Application.Features.Leads.Queries.LeadsGrid;

public sealed class LeadsGridQuery : GridQueryBase, IRequest<GridResponse<LeadsGridQueryDto>>
{
}

public sealed record LeadsGridQueryDto(
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
    DateTime RegistrationDate,
    string? RegistrationIP,
    string? Source,
    DateTime? LastLogin,
    DateTime? LastCommunication);

public sealed class LeadsGridQueryHandler(
    IRepository<Lead> leadRepository,
    IGridService gridService) : IRequestHandler<LeadsGridQuery, GridResponse<LeadsGridQueryDto>>
{
    public async ValueTask<Result<GridResponse<LeadsGridQueryDto>>> Handle(
        LeadsGridQuery request,
        CancellationToken cancellationToken)
    {
        var query = leadRepository.GetQueryable();

        var result = await gridService.ProcessGridQuery(
            query,
            request,
            lead => new LeadsGridQueryDto(
                lead.Id,
                lead.FirstName,
                lead.LastName,
                lead.Email,
                lead.Telephone,
                lead.Country,
                lead.Language,
                lead.Status,
                lead.SalesStatus,
                lead.IsProblematic,
                lead.IsBonusAbuser,
                lead.RegistrationDate,
                lead.RegistrationIP,
                lead.Source,
                lead.LastLogin,
                lead.LastCommunication),
            cancellationToken);

        return Result.Success(result);
    }
}