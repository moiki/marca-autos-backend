namespace BrandCars.Application.Models;

public sealed record MarcaAutoDto(
    Guid Id,
    string Name,
    string? Description,
    string? OriginCountry,
    bool IsActive,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);
