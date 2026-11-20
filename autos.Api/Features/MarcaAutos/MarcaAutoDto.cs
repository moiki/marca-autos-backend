using System.ComponentModel.DataAnnotations;

namespace autos.Api.Features.MarcaAutos;

public sealed record MarcaAutoDto(
    Guid Id,
    string Name,
    string? Description,
    string? OriginCountry,
    bool IsActive,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);

public sealed record CreateMarcaAutoRequest(
    [Required, StringLength(100)] string Name,
    [StringLength(500)] string? Description,
    [StringLength(100)] string? OriginCountry);

public sealed record UpdateMarcaAutoRequest(
    [Required, StringLength(100)] string Name,
    [StringLength(500)] string? Description,
    [StringLength(100)] string? OriginCountry,
    bool IsActive);