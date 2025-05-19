using CRM.Identity.Api.Transformers;

var builder = WebApplication.CreateBuilder(args);
{
    builder.Services.AddHttpContextAccessor();

    builder.Services
        .AddApplication()
        .AddInfrastructure(builder.Configuration)
        .AddPersistence(builder.Configuration);

    builder.Services.AddRouting(options =>
    {
        options.LowercaseUrls = true;
        options.LowercaseQueryStrings = true;
    });
    builder.Services.AddControllers();

    builder.Services.AddOpenApi(options => { options.AddDocumentTransformer<BearerSecuritySchemeTransformer>(); });

    builder.Logging.ClearProviders();
    builder.Logging.AddConsole();
}

var app = builder.Build();
{
    app.UseCors("AllowAll");

    app.UseMiddleware<ExceptionHandlingMiddleware>();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.MapScalarApiReference(options =>
        {
            options
                .WithTitle("CRM Identity API")
                .WithTheme(ScalarTheme.Purple)
                .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
        });
    }
    else
    {
        app.UseHsts();
    }

    app.UseHttpsRedirection();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    using (var scope = app.Services.CreateScope())
    {
        var permissionSynchronizer = scope.ServiceProvider.GetRequiredService<IPermissionSynchronizer>();
        await permissionSynchronizer.SynchronizePermissionsAsync();
    }

    app.Run();
}