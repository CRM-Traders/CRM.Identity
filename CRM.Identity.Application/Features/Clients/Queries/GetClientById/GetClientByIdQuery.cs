using CRM.Identity.Application.Common.Specifications.Clients;
using CRM.Identity.Domain.Entities.Clients;
using CRM.Identity.Domain.Entities.Clients.Enums;

namespace CRM.Identity.Application.Features.Clients.Queries.GetClientById;

public sealed record GetClientByIdQuery(Guid Id) : IRequest<ClientDetailDto>;

public sealed record ClientDetailDto(
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
    bool HasInvestments,
    Guid AffiliateId,
    string? AffiliateName,
    DateTime? FTDTime,
    DateTime? LTDTime,
    DateTime? QualificationTime,
    DateTime RegistrationDate,
    string? RegistrationIP,
    string? Source,
    DateTime? LastLogin,
    DateTime? LastCommunication);

public sealed class GetClientByIdQueryHandler(
    IRepository<Client> clientRepository) : IRequestHandler<GetClientByIdQuery, ClientDetailDto>
{
    public async ValueTask<Result<ClientDetailDto>> Handle(GetClientByIdQuery request,
        CancellationToken cancellationToken)
    {
        var specification = new ClientByIdWithAffiliateSpecification(request.Id);
        var client = await clientRepository.FirstOrDefaultAsync(specification, cancellationToken);

        if (client == null)
        {
            return Result.Failure<ClientDetailDto>("Client not found", "NotFound");
        }

        var response = new ClientDetailDto(
            client.Id,
            client.FirstName,
            client.LastName,
            client.Email,
            client.Telephone,
            client.SecondTelephone,
            client.Skype,
            client.Country,
            client.Language,
            client.DateOfBirth,
            client.Status,
            client.KycStatusId,
            client.SalesStatus,
            client.IsProblematic,
            client.IsBonusAbuser,
            client.BonusAbuserReason,
            client.HasInvestments,
            client.AffiliateId,
            $"{client.Affiliate?.User!.FirstName} {client.Affiliate?.User!.LastName}",
            client.FTDTime,
            client.LTDTime,
            client.QualificationTime,
            client.RegistrationDate,
            client.RegistrationIP,
            client.Source,
            client.LastLogin,
            client.LastCommunication);

        return Result.Success(response);
    }
}