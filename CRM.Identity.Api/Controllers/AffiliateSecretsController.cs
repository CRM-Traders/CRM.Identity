using CRM.Identity.Application.Features.AffiliateSecrets.Commands.ActivateAffiliateSecret;
using CRM.Identity.Application.Features.AffiliateSecrets.Commands.CreateAffiliateSecret;
using CRM.Identity.Application.Features.AffiliateSecrets.Commands.DeactivateAffiliateSecret;
using CRM.Identity.Application.Features.AffiliateSecrets.Commands.DeleteAffiliateSecret;
using CRM.Identity.Application.Features.AffiliateSecrets.Commands.UpdateAffiliateSecret;
using CRM.Identity.Application.Features.AffiliateSecrets.Queries.ExportAffiliateSecrets;
using CRM.Identity.Application.Features.AffiliateSecrets.Queries.GetAffiliateSecretById;
using CRM.Identity.Application.Features.AffiliateSecrets.Queries.GetAffiliateSecrets;
using CRM.Identity.Application.Features.AffiliateSecrets.Queries.GetAffiliateSecretsByAffiliate;
using CRM.Identity.Infrastructure.Attributes;

namespace CRM.Identity.Api.Controllers;

public class AffiliateSecretsController(IMediator _send) : BaseController(_send)
{
    [HttpPost]
    //[Permission(20, "Create Affiliate Secret", "AffiliateSecrets", ActionType.C, RoleConstants.Admin)]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IResult> CreateAffiliateSecret([FromBody] CreateAffiliateSecretCommand command,
        CancellationToken cancellationToken)
    {
        return await SendAsync(command, cancellationToken);
    }

    [HttpPut("{id}")]
    //[Permission(21, "Update Affiliate Secret", "AffiliateSecrets", ActionType.E, RoleConstants.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IResult> UpdateAffiliateSecret(Guid id, [FromBody] UpdateAffiliateSecretCommand command,
        CancellationToken cancellationToken)
    {
        if (id != command.Id)
            return Results.BadRequest("ID mismatch");

        return await SendAsync(command, cancellationToken);
    }

    [HttpPost("{id}/activate")]
    //[Permission(22, "Activate Affiliate Secret", "AffiliateSecrets", ActionType.E, RoleConstants.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IResult> ActivateAffiliateSecret(Guid id, CancellationToken cancellationToken)
    {
        return await SendAsync(new ActivateAffiliateSecretCommand(id), cancellationToken);
    }

    [HttpPost("{id}/deactivate")]
    //[Permission(23, "Deactivate Affiliate Secret", "AffiliateSecrets", ActionType.E, RoleConstants.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IResult> DeactivateAffiliateSecret(Guid id, CancellationToken cancellationToken)
    {
        return await SendAsync(new DeactivateAffiliateSecretCommand(id), cancellationToken);
    }

    [HttpDelete("{id}")]
    //[Permission(24, "Delete Affiliate Secret", "AffiliateSecrets", ActionType.D, RoleConstants.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IResult> DeleteAffiliateSecret(Guid id, CancellationToken cancellationToken)
    {
        return await SendAsync(new DeleteAffiliateSecretCommand(id), cancellationToken);
    }

    [HttpGet]
    //[Permission(25, "View Affiliate Secrets", "AffiliateSecrets", ActionType.V, RoleConstants.Admin)]
    [ProducesResponseType(typeof(GetAffiliateSecretsQueryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IResult> GetAffiliateSecrets(
        [FromQuery] Guid? affiliateId,
        [FromQuery] bool? isActive,
        [FromQuery] bool? isExpired,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        return await SendAsync(new GetAffiliateSecretsQuery(affiliateId, isActive, isExpired, pageNumber, pageSize),
            cancellationToken);
    }

    [HttpGet("{id}")]
    //[Permission(26, "View Affiliate Secret Details", "AffiliateSecrets", ActionType.V, RoleConstants.Admin)]
    [ProducesResponseType(typeof(AffiliateSecretDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IResult> GetAffiliateSecretById(Guid id, CancellationToken cancellationToken)
    {
        return await SendAsync(new GetAffiliateSecretByIdQuery(id), cancellationToken);
    }

    [HttpGet("by-affiliate/{affiliateId}")]
    //[Permission(27, "View Affiliate Secrets by Affiliate", "AffiliateSecrets", ActionType.V,RoleConstants.AllExceptUser)]
    [ProducesResponseType(typeof(List<AffiliateSecretDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IResult> GetAffiliateSecretsByAffiliate(Guid affiliateId, CancellationToken cancellationToken)
    {
        return await SendAsync(new GetAffiliateSecretsByAffiliateQuery(affiliateId), cancellationToken);
    }

    [HttpGet("export")]
    //[Permission(28, "Export Affiliate Secrets", "AffiliateSecrets", ActionType.V, RoleConstants.Admin)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IResult> ExportAffiliateSecrets(
        [FromQuery] Guid? affiliateId,
        [FromQuery] bool? isActive,
        [FromQuery] bool? isExpired,
        CancellationToken cancellationToken)
    {
        var result = await _send.Send(new ExportAffiliateSecretsQuery(affiliateId, isActive, isExpired),
            cancellationToken);

        if (!result.IsSuccess)
            return ToResult(result);

        var fileName = $"affiliate_secrets_export_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx";
        return Results.File(result.Value!, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileName);
    }
}