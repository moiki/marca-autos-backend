using autos.Api.Features.MarcaAutos;
using Microsoft.EntityFrameworkCore;

namespace autos.Api.Data;

/// <summary>
/// Datos de referencia. Idempotente: solo inserta las marcas que aún no existen.
/// Para agregar una marca nueva, añádela a <see cref="Marcas"/>.
/// </summary>
public static class DbSeeder
{
    private static readonly (string Name, string Description, string OriginCountry)[] Marcas =
    [
        ("Toyota", "Fabricante japonés de automóviles", "Japón"),
        ("Honda", "Fabricante japonés de automóviles y motocicletas", "Japón"),
        ("Ford", "Fabricante estadounidense de automóviles", "Estados Unidos"),
        ("BMW", "Fabricante alemán de automóviles", "Alemania")
    ];

    public static async Task SeedAsync(AppDbContext db, CancellationToken ct = default)
    {
        var nombres = Marcas.Select(m => m.Name).ToArray();

        var existentes = await db.MarcasAutos
            .AsNoTracking()
            .Where(m => nombres.Contains(m.Name))
            .Select(m => m.Name)
            .ToListAsync(ct);

        var ahora = DateTime.UtcNow;

        foreach (var (name, description, originCountry) in Marcas)
        {
            if (existentes.Contains(name))
            {
                continue;
            }

            db.MarcasAutos.Add(new MarcaAuto
            {
                Id = Guid.NewGuid(),
                Name = name,
                Description = description,
                OriginCountry = originCountry,
                IsActive = true,
                CreatedDate = ahora,
                UpdatedDate = ahora
            });
        }

        await db.SaveChangesAsync(ct);
    }
}
