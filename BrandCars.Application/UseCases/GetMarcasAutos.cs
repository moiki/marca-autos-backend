using BrandCars.Application.Models;
using BrandCars.Application.Ports.In;
using BrandCars.Application.Ports.Out;

namespace BrandCars.Application.UseCases;

public sealed class GetMarcasAutos(IMarcaAutoRepository repository) : IGetMarcasAutos
{
    public async Task<IReadOnlyList<MarcaAutoDto>> ExecuteAsync(CancellationToken ct)
    {
        var marcas = await repository.GetAllAsync(ct);

        return marcas
            .OrderBy(m => m.Name)
            .Select(m => new MarcaAutoDto(
                m.Id, m.Name, m.Description, m.OriginCountry,
                m.IsActive, m.CreatedAtUtc, m.UpdatedAtUtc))
            .ToList();
    }
}
