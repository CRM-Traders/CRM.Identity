namespace CRM.Identity.Application.Common.Abstractions.Mediators;

public delegate ValueTask<Result<TResponse>> RequestHandlerDelegate<TResponse>();
