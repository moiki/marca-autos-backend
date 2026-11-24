using BrandCars.Application.Models;

namespace BrandCars.Application.Ports.In;

public interface IGetMarcasAutos
{
    Task<IReadOnlyList<MarcaAutoDto>> ExecuteAsync(CancellationToken ct);
}
