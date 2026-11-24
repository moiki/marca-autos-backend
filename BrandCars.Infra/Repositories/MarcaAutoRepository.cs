using BrandCars.Application.Ports.Out;
using BrandCars.Domain;
using BrandCars.Infra.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BrandCars.Infra.Repositories;

public sealed class MarcaAutoRepository(AppDbContext db) : IMarcaAutoRepository
{
    public async Task<IReadOnlyList<MarcaAuto>> GetAllAsync(CancellationToken ct)
        => await db.MarcasAutos.AsNoTracking().ToListAsync(ct);
}
