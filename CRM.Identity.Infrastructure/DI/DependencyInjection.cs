namespace CRM.Identity.Infrastructure.DI;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        //services.AddHttpContextAccessor();
        //services.AddScoped<IUserContext, UserContext>();
        //services.AddScoped<IOutboxService, OutboxService>();
        //services.AddScoped<IEventPublisher, EventPublisher>();


        return services;
    }
}