using Common;

namespace autos.Api.Features.MarcaAutos;

public interface IMarcaAutoService
{
    Task<IReadOnlyList<MarcaAutoDto>> GetAllAsync(CancellationToken ct = default);

    Task<MarcaAutoDto?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Falla con <c>marca_auto_duplicate_name</c> si el nombre ya existe.</summary>
    Task<Result<MarcaAutoDto>> CreateAsync(CreateMarcaAutoRequest request, CancellationToken ct = default);

    /// <summary>Falla con <c>marca_auto_not_found</c> o <c>marca_auto_duplicate_name</c>.</summary>
    Task<Result<MarcaAutoDto>> UpdateAsync(Guid id, UpdateMarcaAutoRequest request, CancellationToken ct = default);

    /// <summary>Falla con <c>marca_auto_not_found</c> si el id no existe.</summary>
    Task<Result> DeleteAsync(Guid id, CancellationToken ct = default);
}
