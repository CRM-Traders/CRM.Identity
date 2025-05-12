namespace CRM.Identity.Application.Common;

public sealed class Mediator(IServiceProvider _serviceProvider) : IMediator
{
    private static readonly ConcurrentDictionary<Type, object> _handlerFactories = new();

    public ValueTask<Result<TResponse>> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        var requestType = request.GetType();

        var factory = (Func<IServiceProvider, IRequest<TResponse>, CancellationToken, ValueTask<TResponse>>)
            _handlerFactories.GetOrAdd(requestType, type => CreateHandlerFactory<TResponse>(type));

        return factory(_serviceProvider, request, cancellationToken);
    }

    private static object CreateHandlerFactory<TResponse>(Type requestType)
    {
        var handlerInterfaceType = typeof(IRequestHandler<,>).MakeGenericType(requestType, typeof(TResponse));

        var handlerServiceType = handlerInterfaceType;

        var pipelineBehaviorType = typeof(IPipelineBehavior<,>).MakeGenericType(requestType, typeof(TResponse));
        var pipelineServiceType = typeof(IEnumerable<>).MakeGenericType(pipelineBehaviorType);

        var serviceProviderParam = Expression.Parameter(typeof(IServiceProvider), "serviceProvider");
        var requestParam = Expression.Parameter(typeof(IRequest<TResponse>), "request");
        var cancellationTokenParam = Expression.Parameter(typeof(CancellationToken), "cancellationToken");

        var castRequest = Expression.Convert(requestParam, requestType);

        var getHandlerExpr = Expression.Call(
            typeof(ServiceProviderServiceExtensions).GetMethod("GetRequiredService")!.MakeGenericMethod(handlerServiceType),
            serviceProviderParam);

        var getPipelineExpr = Expression.Call(
            typeof(ServiceProviderServiceExtensions).GetMethod("GetService")!.MakeGenericMethod(pipelineServiceType),
            serviceProviderParam);

        var handlerVar = Expression.Variable(handlerServiceType, "handler");
        var pipelineVar = Expression.Variable(pipelineServiceType, "pipeline");

        var assignHandler = Expression.Assign(handlerVar, getHandlerExpr);
        var assignPipeline = Expression.Assign(pipelineVar, getPipelineExpr);

        var executeBlock = BuildExecutionExpressions<TResponse>(
            handlerVar, pipelineVar, castRequest, cancellationTokenParam, requestType);

        var combinedBlock = Expression.Block(
            new[] { handlerVar, pipelineVar },
            assignHandler,
            assignPipeline,
            executeBlock
        );

        var lambda = Expression.Lambda<Func<IServiceProvider, IRequest<TResponse>, CancellationToken, ValueTask<TResponse>>>(
            combinedBlock, serviceProviderParam, requestParam, cancellationTokenParam);

        return lambda.Compile();
    }

    private static Expression BuildExecutionExpressions<TResponse>(
        Expression handlerExpr,
        Expression pipelineExpr,
        Expression requestExpr,
        Expression cancellationTokenExpr,
        Type requestType)
    {
        var handleMethod = typeof(IRequestHandler<,>)
            .MakeGenericType(requestType, typeof(TResponse))
            .GetMethod("Handle")!;

        var directHandleCall = Expression.Call(
            handlerExpr,
            handleMethod,
            requestExpr,
            cancellationTokenExpr);

        var pipelineNotNullOrEmpty = Expression.Not(
            Expression.Or(
                Expression.Equal(pipelineExpr, Expression.Constant(null)),
                Expression.Call(
                    typeof(Enumerable),
                    "Any",
                    new[] { typeof(IPipelineBehavior<,>).MakeGenericType(requestType, typeof(TResponse)) },
                    pipelineExpr
                )
            )
        );

        var pipelineExecutionExpr = BuildPipelineExecution<TResponse>(
            pipelineExpr, handlerExpr, requestExpr, cancellationTokenExpr, handleMethod, requestType);

        return Expression.Condition(
            pipelineNotNullOrEmpty,
            pipelineExecutionExpr,
            directHandleCall
        );
    }

    private static Expression BuildPipelineExecution<TResponse>(
        Expression pipelineExpr,
        Expression handlerExpr,
        Expression requestExpr,
        Expression cancellationTokenExpr,
        MethodInfo handleMethod,
        Type requestType)
    {
        var pipelineBehaviorType = typeof(IPipelineBehavior<,>).MakeGenericType(requestType, typeof(TResponse));
        var pipelineHandleMethod = pipelineBehaviorType.GetMethod("Handle")!;

        var getBehaviorExpr = Expression.Call(
            typeof(Enumerable),
            "FirstOrDefault",
            new[] { pipelineBehaviorType },
            pipelineExpr
        );

        var behaviorVar = Expression.Variable(pipelineBehaviorType, "behavior");
        var assignBehavior = Expression.Assign(behaviorVar, getBehaviorExpr);

        var delegateType = typeof(RequestHandlerDelegate<>).MakeGenericType(typeof(TResponse));
        var delegateMethod = delegateType.GetMethod("Invoke")!;

        var handlerDelegateExpr = Expression.Lambda(
            delegateType,
            Expression.Call(handlerExpr, handleMethod, requestExpr, cancellationTokenExpr)
        );

        var behaviorHandleCall = Expression.Call(
            behaviorVar,
            pipelineHandleMethod,
            requestExpr,
            handlerDelegateExpr,
            cancellationTokenExpr
        );

        return Expression.Block(
            new[] { behaviorVar },
            assignBehavior,
            behaviorHandleCall
        );
    }
}
