using Common;
using Microsoft.AspNetCore.Mvc;

namespace autos.Api.Features.MarcaAutos;

[ApiController]
[Route("api/marcas-autos")]
[Produces("application/json")]
public sealed class MarcasAutosController(IMarcaAutoService service) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<MarcaAutoDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<MarcaAutoDto>>> GetAll(CancellationToken ct)
    {
        var marcas = await service.GetAllAsync(ct);
        return Ok(marcas);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<MarcaAutoDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MarcaAutoDto>> GetById(Guid id, CancellationToken ct)
    {
        var marca = await service.GetByIdAsync(id, ct);
        return marca is null ? NotFound() : Ok(marca);
    }

    [HttpPost]
    [ProducesResponseType<MarcaAutoDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<MarcaAutoDto>> Create(CreateMarcaAutoRequest request, CancellationToken ct)
    {
        var result = await service.CreateAsync(request, ct);
        if (result.IsFailure)
        {
            return result.Error.ToObjectResult();
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType<MarcaAutoDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<MarcaAutoDto>> Update(
        Guid id,
        UpdateMarcaAutoRequest request,
        CancellationToken ct)
    {
        var result = await service.UpdateAsync(id, request, ct);
        if (result.IsFailure)
        {
            return result.Error.ToObjectResult();
        }

        return Ok(result.Value);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await service.DeleteAsync(id, ct);
        return result.IsFailure ? result.Error.ToObjectResult() : NoContent();
    }
}
