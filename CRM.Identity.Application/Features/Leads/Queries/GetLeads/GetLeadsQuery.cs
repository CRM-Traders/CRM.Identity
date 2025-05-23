using CRM.Identity.Application.Common.Models.Grids;
using CRM.Identity.Application.Common.Services.Grids;
using CRM.Identity.Application.Common.Specifications.Leads;
using CRM.Identity.Domain.Entities.Clients.Enums;
using CRM.Identity.Domain.Entities.Leads;
using Microsoft.EntityFrameworkCore;

namespace CRM.Identity.Application.Features.Leads.Queries.GetLeads;

public sealed class GetLeadsQuery : GridQueryBase, IRequest<GridResponse<LeadDto>>
{
}

public sealed record LeadDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string? Telephone,
    string? Country,
    ClientStatus Status,
    bool IsProblematic,
    bool IsBonusAbuser,
    string? UserFullName,
    DateTime RegistrationDate);

public sealed class GetLeadsQueryHandler(
    IRepository<Lead> leadRepository,
    IGridService gridService) : IRequestHandler<GetLeadsQuery, GridResponse<LeadDto>>
{
    public async ValueTask<Result<GridResponse<LeadDto>>> Handle(GetLeadsQuery request,
        CancellationToken cancellationToken)
    {
        var query = leadRepository.GetQueryable()
            .Include(l => l.User);

        var result = await gridService.ProcessGridQuery(
            query,
            request,
            lead => new LeadDto(
                lead.Id,
                lead.FirstName,
                lead.LastName,
                lead.Email,
                lead.Telephone,
                lead.Country,
                lead.Status,
                lead.IsProblematic,
                lead.IsBonusAbuser,
                lead.User != null ? $"{lead.User.FirstName} {lead.User.LastName}" : null,
                lead.RegistrationDate), cancellationToken);

        return Result.Success(result);
    }
}