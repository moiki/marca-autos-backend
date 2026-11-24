namespace BrandCars.Domain;

public sealed class MarcaAuto : ISoftDeletable
{
    private MarcaAuto() { } // requerido por EF Core

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string? OriginCountry { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }
    
    public static MarcaAuto Crear(
        Guid id, string name, string? description, string? originCountry, DateTime createdAtUtc)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("El nombre de la marca es obligatorio.", nameof(name));

        return new MarcaAuto
        {
            Id = id,
            Name = name.Trim(),
            Description = description?.Trim(),
            OriginCountry = originCountry?.Trim(),
            IsActive = true,
            CreatedAtUtc = createdAtUtc
        };
    }
}
