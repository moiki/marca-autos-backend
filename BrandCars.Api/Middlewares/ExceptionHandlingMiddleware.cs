using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace BrandCars.Middlewares;

/// <summary>
/// Última red de seguridad del adaptador HTTP: convierte cualquier excepción no
/// controlada en un ProblemDetails. El enunciado pide no filtrar el detalle ni el
/// stack trace; en producción conviene limitar esta información.
/// </summary>
public sealed class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Excepción no controlada al procesar {Path}", context.Request.Path);

            if (context.Response.HasStarted)
                throw;

            context.Response.Clear();
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/problem+json";

            var problem = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Error interno del servidor",
                Detail = ex.ToString()
            };

            await context.Response.WriteAsJsonAsync(
                problem,
                options: new JsonSerializerOptions(JsonSerializerDefaults.Web));
        }
    }
}
