namespace CRM.Identity.Application.Common.Abstractions;

public interface IRequestHandler<in TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    ValueTask<Result<TResponse>> Handle(TRequest request, CancellationToken cancellationToken);
}