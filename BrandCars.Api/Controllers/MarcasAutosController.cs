using BrandCars.Application.Models;
using BrandCars.Application.Ports.In;
using Microsoft.AspNetCore.Mvc;

namespace BrandCars.Controllers;

/// <summary>
/// Adaptador entrante: traduce HTTP a una llamada al puerto de entrada.
/// No contiene reglas de negocio ni acceso a datos.
/// </summary>
[ApiController]
[Route("api/marcas-autos")]
[Produces("application/json")]
public sealed class MarcasAutosController(IGetMarcasAutos getMarcasAutos) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<MarcaAutoDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<MarcaAutoDto>>> GetAll(CancellationToken ct)
        => Ok(await getMarcasAutos.ExecuteAsync(ct));
}
