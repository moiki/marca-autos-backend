using BrandCars.Domain;

namespace BrandCars.Application.Ports.Out;

public interface IMarcaAutoRepository
{
    Task<IReadOnlyList<MarcaAuto>> GetAllAsync(CancellationToken ct);
}
