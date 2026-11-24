using BrandCars.Application.Ports.In;
using BrandCars.Application.UseCases;
using BrandCars.Infra;
using BrandCars.Middlewares;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<IGetMarcasAutos, GetMarcasAutos>();
builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks();
// Scalar solo renderiza: el documento OpenAPI que consume se genera aparte.
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.MapControllers();
app.MapHealthChecks("/health");
app.MapOpenApi();
app.MapScalarApiReference();

// Cómodo en Docker/desarrollo para tener la BD lista al arrancar; no es una
// estrategia de producción con varias réplicas (migraciones concurrentes).
if (app.Configuration.GetValue("Database:MigrateOnStartup", false))
    await app.Services.MigrateDatabaseAsync();

app.Run();
