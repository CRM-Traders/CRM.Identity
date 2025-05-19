using CRM.Identity.Application.Common.Models.Grids;
using CRM.Identity.Application.Features.Workers.Queries.WorkersGrid;

namespace CRM.Identity.Api.Controllers;

public class WorkersController(IMediator _sender) : BaseController(_sender)
{
    [HttpPost("grid")]
    [ProducesResponseType(typeof(GridResponse<WorkersGridQueryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IResult> GetUsersGrid(
    [FromBody] WorkersGridQuery query,
    CancellationToken cancellationToken)
    {
        return await SendAsync(query, cancellationToken);
    }
}
