using System;
using autos.Api.Features.MarcaAutos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace autos.Api.Data.Configurations;

public sealed class MarcaAutoConfiguration : IEntityTypeConfiguration<MarcaAuto>
{
    // Valores fijos exigidos por HasData: nunca Guid.NewGuid() ni DateTime.UtcNow aquí.
    private static readonly DateTime SeedDate = new(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private static readonly Guid ToyotaId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid HondaId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid FordId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    private static readonly Guid BmwId = Guid.Parse("44444444-4444-4444-4444-444444444444");

    public void Configure(EntityTypeBuilder<MarcaAuto> builder)
    {
        builder.ToTable("MarcasAutos");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(m => m.Description)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(m => m.OriginCountry)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(m => m.Name).IsUnique();

        builder.HasData(
            new MarcaAuto
            {
                Id = ToyotaId,
                Name = "Toyota",
                Description = "Fabricante japonés de automóviles",
                OriginCountry = "Japón",
                IsActive = true,
                CreatedDate = SeedDate,
                UpdatedDate = SeedDate
            },
            new MarcaAuto
            {
                Id = HondaId,
                Name = "Honda",
                Description = "Fabricante japonés de automóviles y motocicletas",
                OriginCountry = "Japón",
                IsActive = true,
                CreatedDate = SeedDate,
                UpdatedDate = SeedDate
            },
            new MarcaAuto
            {
                Id = FordId,
                Name = "Ford",
                Description = "Fabricante estadounidense de automóviles",
                OriginCountry = "Estados Unidos",
                IsActive = true,
                CreatedDate = SeedDate,
                UpdatedDate = SeedDate
            },
            new MarcaAuto
            {
                Id = BmwId,
                Name = "BMW",
                Description = "Fabricante alemán de automóviles",
                OriginCountry = "Alemania",
                IsActive = true,
                CreatedDate = SeedDate,
                UpdatedDate = SeedDate
            });
    }
}
