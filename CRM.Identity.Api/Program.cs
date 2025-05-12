var builder = WebApplication.CreateBuilder(args);
{
    builder.Services.AddApplication();

    builder.Services.AddControllers();

    builder.Services.AddOpenApi();

    builder.Logging.ClearProviders();
    builder.Logging.AddConsole();
}

var app = builder.Build();
{
    app.UseMiddleware<ExceptionHandlingMiddleware>();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.MapScalarApiReference();
    }
    else
    {
        app.UseHsts();
    }

    app.UseHttpsRedirection();

    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}

