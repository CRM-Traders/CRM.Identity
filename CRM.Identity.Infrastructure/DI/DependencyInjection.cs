namespace CRM.Identity.Infrastructure.DI;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingeltonServices();
        services.AddScopedServices();
        services.AddEventHandlers();

        services.AddCompression();
        services.ConfigureCors();

        services.AddOptions(configuration);
        services.AddRedisConnection();

        services.AddAsymmetricAuthentication(configuration);

        return services;
    }

    private static void ConfigureCors(this IServiceCollection services) 
    {
        // TODO Restrict In Future Base On Origins Options
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAll",
                builder =>
                {
                    builder.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
        });
    }

    private static void AddOptions(this IServiceCollection services, IConfiguration configuration) 
    {
        var jwtOptions = configuration.GetSection(nameof(JwtOptions)).Get<JwtOptions>()!;
        services.AddSingleton(jwtOptions);

        var redisOptions = configuration.GetSection(nameof(RedisOptions)).Get<RedisOptions>()!;
        services.AddSingleton(redisOptions);
    }

    private static void AddSingeltonServices(this IServiceCollection services) 
    {
        services.TryAddSingleton<IPasswordService, PasswordService>();
        services.TryAddSingleton<IJwtTokenService, JwtTokenService>();
        services.TryAddSingleton<IRedisManager, RedisManager>();
    }

    private static void AddRedisConnection(this IServiceCollection services) 
    {
        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var redisOptions = sp.GetRequiredService<RedisOptions>();
            var configOptions = new ConfigurationOptions
            {
                EndPoints = { redisOptions.ConnectionString },
                Password = redisOptions.Password,
                AbortOnConnectFail = false,
                ConnectTimeout = 5000,
                SyncTimeout = 5000
            };

            return ConnectionMultiplexer.Connect(configOptions);
        });
    }

    private static void AddScopedServices(this IServiceCollection services) 
    {
        services.AddScoped<IUserContext, UserContext>();
        services.AddScoped<IOutboxService, OutboxService>();
        services.AddScoped<IEventPublisher, EventPublisher>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
    }

    private static void AddEventHandlers(this IServiceCollection services) 
    {
        services.AddScoped<IDomainEventHandler<UserCreatedEvent>, UserCreatedEventHandler>();
    }

    private static void AddCompression(this IServiceCollection services)
    {
        services.Configure<GzipCompressionProviderOptions>(options =>
        {
            options.Level = System.IO.Compression.CompressionLevel.Fastest;
        });

        services.AddResponseCompression(options =>
        {
            options.Providers.Add<GzipCompressionProvider>();
            options.EnableForHttps = true;
        });
    }

    private static void AddAsymmetricAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtOptions = configuration.GetSection(nameof(JwtOptions)).Get<JwtOptions>()!;

        byte[] publicKeyBytes = Convert.FromBase64String(jwtOptions.PublicKey);

        RSA rsaPublicKey = RSA.Create();
        rsaPublicKey.ImportSubjectPublicKeyInfo(publicKeyBytes, out _);

        var issuerSigningKey = new RsaSecurityKey(rsaPublicKey);


        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    IssuerSigningKey = issuerSigningKey,
                    ClockSkew = TimeSpan.Zero
                };
            });

        services.AddAuthorization();
    }
}