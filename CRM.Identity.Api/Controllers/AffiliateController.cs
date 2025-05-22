using CRM.Identity.Application.Features.Affiliates.Commands.CreateAffiliate;
using CRM.Identity.Application.Features.Affiliates.Commands.DeleteAffiliate;
using CRM.Identity.Application.Features.Affiliates.Commands.ImportAffiliates;
using CRM.Identity.Application.Features.Affiliates.Commands.UpdateAffiliate;
using CRM.Identity.Application.Features.Affiliates.Queries.ExportAffiliates;
using CRM.Identity.Application.Features.Affiliates.Queries.GenerateAffiliateTemplate;
using CRM.Identity.Application.Features.Affiliates.Queries.GetAffiliateById;
using CRM.Identity.Application.Features.Affiliates.Queries.GetAffiliates;
using CRM.Identity.Infrastructure.Attributes;

namespace CRM.Identity.Api.Controllers;

public class AffiliatesController(IMediator _send) : BaseController(_send)
{
    [HttpPost]
    [Permission(10, "Create Affiliate", "Affiliates", ActionType.C, RoleConstants.AllExceptUser)]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IResult> CreateAffiliate([FromBody] CreateAffiliateCommand command,
        CancellationToken cancellationToken)
    {
        return await SendAsync(command, cancellationToken);
    }

    [HttpPut("{id}")]
    [Permission(11, "Update Affiliate", "Affiliates", ActionType.E, RoleConstants.AllExceptUser)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IResult> UpdateAffiliate(Guid id, [FromBody] UpdateAffiliateCommand command,
        CancellationToken cancellationToken)
    {
        if (id != command.Id)
            return Results.BadRequest("ID mismatch");

        return await SendAsync(command, cancellationToken);
    }

    [HttpDelete("{id}")]
    [Permission(12, "Delete Affiliate", "Affiliates", ActionType.D, RoleConstants.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IResult> DeleteAffiliate(Guid id, CancellationToken cancellationToken)
    {
        return await SendAsync(new DeleteAffiliateCommand(id), cancellationToken);
    }

    [HttpGet]
    [Permission(13, "View Affiliates", "Affiliates", ActionType.V, RoleConstants.All)]
    [ProducesResponseType(typeof(GetAffiliatesQueryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IResult> GetAffiliates(
        [FromQuery] string? searchTerm,
        [FromQuery] bool? isActive,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        return await SendAsync(new GetAffiliatesQuery(searchTerm, isActive, pageNumber, pageSize), cancellationToken);
    }

    [HttpGet("{id}")]
    [Permission(14, "View Affiliate Details", "Affiliates", ActionType.V, RoleConstants.All)]
    [ProducesResponseType(typeof(AffiliateDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IResult> GetAffiliateById(Guid id, CancellationToken cancellationToken)
    {
        return await SendAsync(new GetAffiliateByIdQuery(id), cancellationToken);
    }

    [HttpPost("import")]
    [Permission(15, "Import Affiliates", "Affiliates", ActionType.C, RoleConstants.AllExceptUser)]
    [ProducesResponseType(typeof(ImportAffiliatesResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IResult> ImportAffiliates(IFormFile file, CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
            return Results.BadRequest("File is required");

        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream, cancellationToken);
        var fileContent = memoryStream.ToArray();

        return await SendAsync(new ImportAffiliatesCommand(fileContent), cancellationToken);
    }

    [HttpGet("export")]
    [Permission(16, "Export Affiliates", "Affiliates", ActionType.V, RoleConstants.AllExceptUser)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IResult> ExportAffiliates(
        [FromQuery] string? searchTerm,
        [FromQuery] bool? isActive,
        CancellationToken cancellationToken)
    {
        var result = await _send.Send(new ExportAffiliatesQuery(searchTerm, isActive), cancellationToken);

        if (!result.IsSuccess)
            return ToResult(result);

        var fileName = $"affiliates_export_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx";
        return Results.File(result.Value!, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileName);
    }

    [HttpGet("import-template")]
    [Permission(17, "Download Affiliate Import Template", "Affiliates", ActionType.V, RoleConstants.AllExceptUser)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IResult> GetImportTemplate(CancellationToken cancellationToken)
    {
        var result = await _send.Send(new GenerateAffiliateTemplateQuery(), cancellationToken);

        if (!result.IsSuccess)
            return ToResult(result);

        var fileName = $"affiliate_import_template_{DateTime.UtcNow:yyyyMMdd}.xlsx";
        return Results.File(result.Value!, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }
}