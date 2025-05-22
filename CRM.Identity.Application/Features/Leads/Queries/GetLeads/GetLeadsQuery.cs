using CRM.Identity.Application.Common.Specifications.Leads;
using CRM.Identity.Domain.Entities.Clients.Enums;
using CRM.Identity.Domain.Entities.Leads;

namespace CRM.Identity.Application.Features.Leads.Queries.GetLeads;

public sealed record GetLeadsQuery(
    string? SearchTerm,
    ClientStatus? Status,
    bool? IsProblematic,
    string? Country,
    string? Source,
    int PageNumber = 1,
    int PageSize = 10) : IRequest<GetLeadsQueryResponse>;

public sealed record GetLeadsQueryResponse(
    List<LeadDto> Leads,
    int TotalCount,
    int PageNumber,
    int PageSize);

public sealed record LeadDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string? Telephone,
    string? Country,
    string? Language,
    ClientStatus Status,
    bool IsProblematic,
    string? Source,
    DateTime RegistrationDate,
    DateTime? LastCommunication);

public sealed class GetLeadsQueryHandler(
    IRepository<Lead> leadRepository) : IRequestHandler<GetLeadsQuery, GetLeadsQueryResponse>
{
    public async ValueTask<Result<GetLeadsQueryResponse>> Handle(GetLeadsQuery request,
        CancellationToken cancellationToken)
    {
        var specification = new LeadsFilterSpecification(
            request.SearchTerm,
            request.Status,
            request.IsProblematic,
            request.Country,
            request.Source,
            request.PageNumber,
            request.PageSize);

        var leads = await leadRepository.ListAsync(specification, cancellationToken);
        var totalCount = await leadRepository.CountAsync(new LeadsCountSpecification(
            request.SearchTerm,
            request.Status,
            request.IsProblematic,
            request.Country,
            request.Source), cancellationToken);

        var leadDtos = leads.Select(l => new LeadDto(
            l.Id,
            l.FirstName,
            l.LastName,
            l.Email,
            l.Telephone,
            l.Country,
            l.Language,
            l.Status,
            l.IsProblematic,
            l.Source,
            l.RegistrationDate,
            l.LastCommunication)).ToList();

        var response = new GetLeadsQueryResponse(
            leadDtos,
            totalCount,
            request.PageNumber,
            request.PageSize);

        return Result.Success(response);
    }
}