namespace CRM.Identity.Api.Controllers.Base;

[Route("api/[controller]")]
[ApiController]
public class BaseController(IMediator _mediator) : ControllerBase
{
    protected async Task<IResult> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(request, cancellationToken);
        return ToResult(result);
    }

    protected IResult ToResult<TResponse>(Result<TResponse> result)
    {
        if (result.IsSuccess)
        {
            if (result.Value == null || typeof(TResponse) == typeof(Unit))
            {
                return TypedResults.NoContent();
            }

            if (Request.Method == "POST" && (
                typeof(TResponse) == typeof(Guid) ||
                typeof(TResponse) == typeof(int) ||
                typeof(TResponse) == typeof(string)))
            {
                string path = $"{Request.Path}/{result.Value}";
                var uri = new Uri(path, UriKind.Relative);
                return TypedResults.Created(uri, result.Value);
            }

            return TypedResults.Ok(result.Value);
        }

        if (result.ValidationErrors?.Count > 0)
        {
            return TypedResults.ValidationProblem(result.ValidationErrors);
        }

        if (!string.IsNullOrEmpty(result.ErrorCode))
        {
            int statusCode = result.ErrorCode switch
            {
                "NotFound" => StatusCodes.Status404NotFound,
                "Unauthorized" => StatusCodes.Status401Unauthorized,
                "Forbidden" => StatusCodes.Status403Forbidden,
                "Conflict" => StatusCodes.Status409Conflict,
                "PreconditionFailed" => StatusCodes.Status412PreconditionFailed,
                "TooManyRequests" => StatusCodes.Status429TooManyRequests,
                "PaymentRequired" => StatusCodes.Status402PaymentRequired,
                _ => StatusCodes.Status400BadRequest
            };

            return TypedResults.Problem(
                detail: result.Error,
                statusCode: statusCode,
                title: result.ErrorCode);
        }

        return TypedResults.Problem(
            detail: result.Error ?? "An unexpected error occurred",
            statusCode: StatusCodes.Status400BadRequest);
    }
}

public readonly struct Unit
{
    public static Unit Value => new();
}