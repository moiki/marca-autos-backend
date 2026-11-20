using autos.Api.Data;
using autos.Api.Features.MarcaAutos;
using autos.Api.Interceptors;
using autos.Api.Middlewares;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddOpenApi();

var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException(
        "Connection string 'ConnectionStrings:Default' is missing. " +
        "Define it at appsettings.Development.json or in the env var ConnectionStrings__Default.");

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(connectionString, npgsql => npgsql.EnableRetryOnFailure()).AddInterceptors(new SoftDeleteInterceptor());

    if (builder.Environment.IsDevelopment())
    {
        options.EnableDetailedErrors();
        options.EnableSensitiveDataLogging();
    }
});

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks();

builder.Services.AddScoped<IMarcaAutoService, MarcaAutoService>();

var app = builder.Build();

// Las migraciones se aplican siempre; los datos de ejemplo solo en Development.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();

    if (app.Environment.IsDevelopment())
    {
        await DbSeeder.SeedAsync(db);
    }
}

app.UseExceptionHandler();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// app.UseHttpsRedirection();

// app.UseAuthorization();
app.MapHealthChecks("/health");
app.MapControllers();

app.Run();
