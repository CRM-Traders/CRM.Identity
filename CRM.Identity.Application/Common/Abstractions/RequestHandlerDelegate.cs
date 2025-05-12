namespace CRM.Identity.Application.Common.Abstractions;

public delegate ValueTask<Result<TResponse>> RequestHandlerDelegate<TResponse>();
