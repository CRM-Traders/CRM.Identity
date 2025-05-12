namespace CRM.Identity.Application.Common.Abstractions.Mediators;

public interface IMediator
{
    ValueTask<Result<TResponse>> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);
}