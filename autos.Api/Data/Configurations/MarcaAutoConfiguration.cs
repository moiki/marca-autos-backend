using autos.Api.Features.MarcaAutos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace autos.Api.Data.Configurations;

public sealed class MarcaAutoConfiguration : IEntityTypeConfiguration<MarcaAuto>
{
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
        // Sin HasColumnType: Npgsql usa 'timestamp with time zone', que acepta DateTime.UtcNow.

        builder.HasIndex(m => m.Name).IsUnique();
    }
}
