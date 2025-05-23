using CRM.Identity.Application.Common.Models.Grids;
using CRM.Identity.Application.Features.Leads.Commands.ConvertLeadToClient;
using CRM.Identity.Application.Features.Leads.Commands.CreateLead;
using CRM.Identity.Application.Features.Leads.Commands.ImportLeads;
using CRM.Identity.Application.Features.Leads.Commands.UpdateLead;
using CRM.Identity.Application.Features.Leads.Queries.ExportLeads;
using CRM.Identity.Application.Features.Leads.Queries.GenerateLeadTemplate;
using CRM.Identity.Application.Features.Leads.Queries.GetLeadById;
using CRM.Identity.Application.Features.Leads.Queries.GetLeads;
using CRM.Identity.Domain.Entities.Clients.Enums;
using CRM.Identity.Infrastructure.Attributes;

namespace CRM.Identity.Api.Controllers;

public class LeadsController(IMediator _send) : BaseController(_send)
{
    [HttpPost]
    //[Permission(40, "Create Lead", "Leads", ActionType.C, RoleConstants.All)]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IResult> CreateLead([FromBody] CreateLeadCommand command, CancellationToken cancellationToken)
    {
        return await SendAsync(command, cancellationToken);
    }

    [HttpPut("{id}")]
    //[Permission(41, "Update Lead", "Leads", ActionType.E, RoleConstants.All)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IResult> UpdateLead(Guid id, [FromBody] UpdateLeadCommand command,
        CancellationToken cancellationToken)
    {
        if (id != command.Id)
            return Results.BadRequest("ID mismatch");

        return await SendAsync(command, cancellationToken);
    }

    [HttpPost("{leadId}/convert-to-client")]
    //[Permission(42, "Convert Lead to Client", "Leads", ActionType.E, RoleConstants.AllExceptUser)]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IResult> ConvertLeadToClient(Guid leadId, [FromBody] ConvertLeadToClientCommand command,
        CancellationToken cancellationToken)
    {
        if (leadId != command.LeadId)
            return Results.BadRequest("Lead ID mismatch");

        return await SendAsync(command, cancellationToken);
    }

    [HttpGet]
    //[Permission(43, "View Leads", "Leads", ActionType.V, RoleConstants.All)]
    [ProducesResponseType(typeof(GridResponse<LeadDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IResult> GetLeads([FromQuery] GetLeadsQuery request,
        CancellationToken cancellationToken = default)
    {
        return await SendAsync(request, cancellationToken);
    }

    [HttpGet("{id}")]
    //[Permission(44, "View Lead Details", "Leads", ActionType.V, RoleConstants.All)]
    [ProducesResponseType(typeof(LeadDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IResult> GetLeadById(Guid id, CancellationToken cancellationToken)
    {
        return await SendAsync(new GetLeadByIdQuery(id), cancellationToken);
    }

    [HttpPost("import")]
    //[Permission(45, "Import Leads", "Leads", ActionType.C, RoleConstants.AllExceptUser)]
    [ProducesResponseType(typeof(ImportLeadsResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IResult> ImportLeads(IFormFile file, CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
            return Results.BadRequest("File is required");

        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream, cancellationToken);
        var fileContent = memoryStream.ToArray();

        return await SendAsync(new ImportLeadsCommand(fileContent), cancellationToken);
    }

    [HttpGet("export")]
    //[Permission(46, "Export Leads", "Leads", ActionType.V, RoleConstants.AllExceptUser)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IResult> ExportLeads(
        [FromQuery] string? searchTerm,
        [FromQuery] ClientStatus? status,
        [FromQuery] bool? isProblematic,
        [FromQuery] string? country,
        [FromQuery] string? source,
        CancellationToken cancellationToken)
    {
        var result = await _send.Send(new ExportLeadsQuery(searchTerm, status, isProblematic, country, source),
            cancellationToken);

        if (!result.IsSuccess)
            return ToResult(result);

        var fileName = $"leads_export_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx";
        return Results.File(result.Value!, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileName);
    }


    [HttpGet("by-country/{country}")]
    //[Permission(47, "View Leads by Country", "Leads", ActionType.V, RoleConstants.All)]
    [ProducesResponseType(typeof(GridResponse<LeadDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IResult> GetLeadsByCountry(
        string country,
        [FromQuery] GetLeadsQuery request,
        CancellationToken cancellationToken = default)
    {
        return await SendAsync(request, cancellationToken);
    }


    [HttpGet("by-source/{source}")]
    //[Permission(48, "View Leads by Source", "Leads", ActionType.V, RoleConstants.All)]
    [ProducesResponseType(typeof(GridResponse<LeadDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IResult> GetLeadsBySource(
        string source,
        [FromQuery] GetLeadsQuery request,
        CancellationToken cancellationToken = default)
    {
        return await SendAsync(request, cancellationToken);
    }

    [HttpGet("conversion-candidates")]
    //[Permission(49, "View Lead Conversion Candidates", "Leads", ActionType.V, RoleConstants.AllExceptUser)]
    [ProducesResponseType(typeof(GridResponse<LeadDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IResult> GetLeadConversionCandidates(
        [FromQuery] GetLeadsQuery request,
        CancellationToken cancellationToken = default)
    {
        // Filter for conversion candidates (Active, not problematic)
        var filters = new Dictionary<string, GridFilterItem>
        {
            ["Status"] = new() { Field = "Status", Operator = "equals", Value = ClientStatus.Active },
            ["IsProblematic"] = new() { Field = "IsProblematic", Operator = "equals", Value = false }
        };

        return await SendAsync(request, cancellationToken);
    }

    [HttpGet("import-template")]
    //[Permission(50, "Download Lead Import Template", "Leads", ActionType.V, RoleConstants.AllExceptUser)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IResult> GetImportTemplate(CancellationToken cancellationToken)
    {
        var result = await _send.Send(new GenerateLeadTemplateQuery(), cancellationToken);

        if (!result.IsSuccess)
            return ToResult(result);

        var fileName = $"lead_import_template_{DateTime.UtcNow:yyyyMMdd}.xlsx";
        return Results.File(result.Value!, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileName);
    }
}