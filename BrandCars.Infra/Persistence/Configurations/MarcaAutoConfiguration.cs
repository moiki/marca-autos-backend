using BrandCars.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BrandCars.Infra.Persistence.Configurations;

public sealed class MarcaAutoConfiguration : IEntityTypeConfiguration<MarcaAuto>
{
    private static readonly DateTime SeedCreatedAtUtc =
        new(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public void Configure(EntityTypeBuilder<MarcaAuto> builder)
    {
        builder.ToTable("MarcasAutos");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Name).IsRequired().HasMaxLength(100);
        builder.HasIndex(m => m.Name).IsUnique();
        builder.Property(m => m.Description).HasMaxLength(500);
        builder.Property(m => m.OriginCountry).HasMaxLength(100);
        builder.Property(m => m.CreatedAtUtc).HasColumnType("timestamp with time zone");
        builder.Property(m => m.UpdatedAtUtc).HasColumnType("timestamp with time zone");

        // El soft delete se aplica en la consulta: es una decisión de persistencia y
        // evita que cada caso de uso tenga que recordar filtrar las marcas inactivas.
        builder.HasQueryFilter(m => m.IsActive);

        // Valores literales: la migración debe ser reproducible y no cambiar entre ejecuciones.
        builder.HasData(
            MarcaAuto.Crear(
                new Guid("11111111-1111-1111-1111-111111111111"),
                "Toyota", "Fabricante japonés de automóviles.", "Japón", SeedCreatedAtUtc),
            MarcaAuto.Crear(
                new Guid("22222222-2222-2222-2222-222222222222"),
                "Ford", "Fabricante estadounidense de automóviles.", "Estados Unidos", SeedCreatedAtUtc),
            MarcaAuto.Crear(
                new Guid("33333333-3333-3333-3333-333333333333"),
                "Volkswagen", "Fabricante alemán de automóviles.", "Alemania", SeedCreatedAtUtc));
    }
}
