using CRM.Identity.Application.Features.Clients.Commands.AssignClientToAffiliate;
using CRM.Identity.Application.Features.Clients.Commands.ChangeClientStatus;
using CRM.Identity.Application.Features.Clients.Commands.CreateClient;
using CRM.Identity.Application.Features.Clients.Commands.ImportClients;
using CRM.Identity.Application.Features.Clients.Commands.UpdateClient;
using CRM.Identity.Application.Features.Clients.Queries.ExportClients;
using CRM.Identity.Application.Features.Clients.Queries.GenerateClientTemplate;
using CRM.Identity.Application.Features.Clients.Queries.GetClientById;
using CRM.Identity.Application.Features.Clients.Queries.GetClients;
using CRM.Identity.Domain.Entities.Clients.Enums;
using CRM.Identity.Infrastructure.Attributes;

namespace CRM.Identity.Api.Controllers;

public class ClientsController(IMediator _send) : BaseController(_send)
{
    [HttpPost]
    [Permission(30, "Create Client", "Clients", ActionType.C, RoleConstants.AllExceptUser)]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IResult> CreateClient([FromBody] CreateClientCommand command, CancellationToken cancellationToken)
    {
        return await SendAsync(command, cancellationToken);
    }

    [HttpPut("{id}")]
    [Permission(31, "Update Client", "Clients", ActionType.E, RoleConstants.AllExceptUser)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IResult> UpdateClient(Guid id, [FromBody] UpdateClientCommand command,
        CancellationToken cancellationToken)
    {
        if (id != command.Id)
            return Results.BadRequest("ID mismatch");

        return await SendAsync(command, cancellationToken);
    }

    [HttpPut("{id}/status")]
    [Permission(32, "Change Client Status", "Clients", ActionType.E, RoleConstants.AllExceptUser)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IResult> ChangeClientStatus(Guid id, [FromBody] ChangeClientStatusCommand command,
        CancellationToken cancellationToken)
    {
        if (id != command.Id)
            return Results.BadRequest("ID mismatch");

        return await SendAsync(command, cancellationToken);
    }

    [HttpPut("{clientId}/assign-affiliate/{affiliateId}")]
    [Permission(33, "Assign Client to Affiliate", "Clients", ActionType.E, RoleConstants.AllExceptUser)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IResult> AssignClientToAffiliate(Guid clientId, Guid affiliateId,
        CancellationToken cancellationToken)
    {
        return await SendAsync(new AssignClientToAffiliateCommand(clientId, affiliateId), cancellationToken);
    }

    [HttpGet]
    [Permission(34, "View Clients", "Clients", ActionType.V, RoleConstants.All)]
    [ProducesResponseType(typeof(GetClientsQueryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IResult> GetClients(
        [FromQuery] string? searchTerm,
        [FromQuery] Guid? affiliateId,
        [FromQuery] ClientStatus? status,
        [FromQuery] bool? isProblematic,
        [FromQuery] bool? isBonusAbuser,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        return await SendAsync(
            new GetClientsQuery(searchTerm, affiliateId, status, isProblematic, isBonusAbuser, pageNumber, pageSize),
            cancellationToken);
    }

    [HttpGet("{id}")]
    [Permission(35, "View Client Details", "Clients", ActionType.V, RoleConstants.All)]
    [ProducesResponseType(typeof(ClientDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IResult> GetClientById(Guid id, CancellationToken cancellationToken)
    {
        return await SendAsync(new GetClientByIdQuery(id), cancellationToken);
    }

    [HttpPost("import")]
    [Permission(36, "Import Clients", "Clients", ActionType.C, RoleConstants.AllExceptUser)]
    [ProducesResponseType(typeof(ImportClientsResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IResult> ImportClients(IFormFile file, CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
            return Results.BadRequest("File is required");

        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream, cancellationToken);
        var fileContent = memoryStream.ToArray();

        return await SendAsync(new ImportClientsCommand(fileContent), cancellationToken);
    }

    [HttpGet("export")]
    [Permission(37, "Export Clients", "Clients", ActionType.V, RoleConstants.AllExceptUser)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IResult> ExportClients(
        [FromQuery] string? searchTerm,
        [FromQuery] Guid? affiliateId,
        [FromQuery] ClientStatus? status,
        [FromQuery] bool? isProblematic,
        [FromQuery] bool? isBonusAbuser,
        CancellationToken cancellationToken)
    {
        var result =
            await _send.Send(new ExportClientsQuery(searchTerm, affiliateId, status, isProblematic, isBonusAbuser),
                cancellationToken);

        if (!result.IsSuccess)
            return ToResult(result);

        var fileName = $"clients_export_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx";
        return Results.File(result.Value!, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileName);
    }

    [HttpGet("by-affiliate/{affiliateId}")]
    [Permission(38, "View Clients by Affiliate", "Clients", ActionType.V, RoleConstants.All)]
    [ProducesResponseType(typeof(GetClientsQueryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IResult> GetClientsByAffiliate(
        Guid affiliateId,
        [FromQuery] string? searchTerm,
        [FromQuery] ClientStatus? status,
        [FromQuery] bool? isProblematic,
        [FromQuery] bool? isBonusAbuser,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        return await SendAsync(
            new GetClientsQuery(searchTerm, affiliateId, status, isProblematic, isBonusAbuser, pageNumber, pageSize),
            cancellationToken);
    }

    [HttpGet("by-affiliate/{affiliateId}/export")]
    [Permission(39, "Export Clients by Affiliate", "Clients", ActionType.V, RoleConstants.AllExceptUser)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IResult> ExportClientsByAffiliate(
        Guid affiliateId,
        [FromQuery] string? searchTerm,
        [FromQuery] ClientStatus? status,
        [FromQuery] bool? isProblematic,
        [FromQuery] bool? isBonusAbuser,
        CancellationToken cancellationToken)
    {
        var result =
            await _send.Send(new ExportClientsQuery(searchTerm, affiliateId, status, isProblematic, isBonusAbuser),
                cancellationToken);

        if (!result.IsSuccess)
            return ToResult(result);

        var fileName = $"clients_affiliate_{affiliateId}_export_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx";
        return Results.File(result.Value, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileName);
    }

    [HttpGet("import-template")]
    [Permission(40, "Download Client Import Template", "Clients", ActionType.V, RoleConstants.AllExceptUser)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IResult> GetImportTemplate(CancellationToken cancellationToken)
    {
        var result = await _send.Send(new GenerateClientTemplateQuery(), cancellationToken);

        if (!result.IsSuccess)
            return ToResult(result);

        var fileName = $"client_import_template_{DateTime.UtcNow:yyyyMMdd}.xlsx";
        return Results.File(result.Value!, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }
}