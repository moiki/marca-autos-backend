using Microsoft.AspNetCore.Mvc;

namespace Common;

public static class ResultExtensions
{
    /// <summary>
    /// Traduce un Error de dominio a ProblemDetails. El ErrorType decide el status;
    /// el controller nunca compara Error.Code como string.
    /// </summary>
    public static ProblemDetails ToProblemDetails(this Error error) => new()
    {
        Status = error.Type switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Unavailable => StatusCodes.Status503ServiceUnavailable,
            _ => StatusCodes.Status500InternalServerError
        },
        Title = error.Code,
        Detail = error.Description
    };

    /// <summary>
    /// Para controladores: ControllerBase.Problem() no acepta ProblemDetails directamente
    /// en ASP.NET Core 10, así que envolvemos el ProblemDetails en un ObjectResult.
    /// </summary>
    public static ObjectResult ToObjectResult(this Error error)
    {
        var problem = error.ToProblemDetails();
        return new ObjectResult(problem) { StatusCode = problem.Status };
    }
}
