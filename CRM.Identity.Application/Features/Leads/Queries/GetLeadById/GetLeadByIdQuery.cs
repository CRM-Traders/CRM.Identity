using CRM.Identity.Domain.Entities.Clients.Enums;
using CRM.Identity.Domain.Entities.Leads;

namespace CRM.Identity.Application.Features.Leads.Queries.GetLeadById;

public sealed record GetLeadByIdQuery(Guid Id) : IRequest<LeadDetailDto>;

public sealed record LeadDetailDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string? Telephone,
    string? SecondTelephone,
    string? Skype,
    string? Country,
    string? Language,
    DateTime? DateOfBirth,
    ClientStatus Status,
    string? KycStatusId,
    string? SalesStatus,
    bool IsProblematic,
    bool IsBonusAbuser,
    string? BonusAbuserReason,
    DateTime RegistrationDate,
    string? RegistrationIP,
    string? Source,
    DateTime? LastLogin,
    DateTime? LastCommunication);

public sealed class GetLeadByIdQueryHandler(
    IRepository<Lead> leadRepository) : IRequestHandler<GetLeadByIdQuery, LeadDetailDto>
{
    public async ValueTask<Result<LeadDetailDto>> Handle(GetLeadByIdQuery request, CancellationToken cancellationToken)
    {
        var lead = await leadRepository.GetByIdAsync(request.Id, cancellationToken);

        if (lead == null)
        {
            return Result.Failure<LeadDetailDto>("Lead not found", "NotFound");
        }

        var response = new LeadDetailDto(
            lead.Id,
            lead.FirstName,
            lead.LastName,
            lead.Email,
            lead.Telephone,
            lead.SecondTelephone,
            lead.Skype,
            lead.Country,
            lead.Language,
            lead.DateOfBirth,
            lead.Status,
            lead.KycStatusId,
            lead.SalesStatus,
            lead.IsProblematic,
            lead.IsBonusAbuser,
            lead.BonusAbuserReason,
            lead.RegistrationDate,
            lead.RegistrationIP,
            lead.Source,
            lead.LastLogin,
            lead.LastCommunication);

        return Result.Success(response);
    }
}