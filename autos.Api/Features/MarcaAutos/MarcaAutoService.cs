using autos.Api.Data;
using Common;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace autos.Api.Features.MarcaAutos;

public sealed class MarcaAutoService(AppDbContext db) : IMarcaAutoService
{
    public async Task<IReadOnlyList<MarcaAutoDto>> GetAllAsync(CancellationToken ct = default)
    {
        return await db.MarcasAutos
            .AsNoTracking()
            .OrderBy(m => m.Name)
            .Select(m => new MarcaAutoDto(
                m.Id,
                m.Name,
                m.Description,
                m.OriginCountry,
                m.IsActive,
                m.CreatedDate,
                m.UpdatedDate))
            .ToListAsync(ct);
    }

    public async Task<MarcaAutoDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await db.MarcasAutos
            .AsNoTracking()
            .Where(m => m.Id == id)
            .Select(m => new MarcaAutoDto(
                m.Id,
                m.Name,
                m.Description,
                m.OriginCountry,
                m.IsActive,
                m.CreatedDate,
                m.UpdatedDate))
            .SingleOrDefaultAsync(ct);
    }

    public async Task<Result<MarcaAutoDto>> CreateAsync(
        CreateMarcaAutoRequest request,
        CancellationToken ct = default)
    {
        if (await NameExistsAsync(request.Name, ct: ct))
        {
            return MarcaAutoErrors.DuplicateName(request.Name);
        }

        var now = DateTime.UtcNow;

        var entity = new MarcaAuto
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Description = request.Description?.Trim() ?? string.Empty,
            OriginCountry = request.OriginCountry?.Trim() ?? string.Empty,
            IsActive = true,
            CreatedDate = now,
            UpdatedDate = now
        };

        db.MarcasAutos.Add(entity);

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            // El índice único es la garantía final: cubre la race condition del pre-check.
            return MarcaAutoErrors.DuplicateName(request.Name);
        }

        return ToDto(entity);
    }

    public async Task<Result<MarcaAutoDto>> UpdateAsync(
        Guid id,
        UpdateMarcaAutoRequest request,
        CancellationToken ct = default)
    {
        var entity = await db.MarcasAutos.SingleOrDefaultAsync(m => m.Id == id, ct);
        if (entity is null)
        {
            return MarcaAutoErrors.NotFound(id);
        }

        if (await NameExistsAsync(request.Name, id, ct))
        {
            return MarcaAutoErrors.DuplicateName(request.Name);
        }

        entity.Name = request.Name.Trim();
        entity.Description = request.Description?.Trim() ?? string.Empty;
        entity.OriginCountry = request.OriginCountry?.Trim() ?? string.Empty;
        entity.UpdatedDate = DateTime.UtcNow;

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            return MarcaAutoErrors.DuplicateName(request.Name);
        }

        return ToDto(entity);
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await db.MarcasAutos.SingleOrDefaultAsync(m => m.Id == id, ct);
        if (entity is null)
        {
            return MarcaAutoErrors.NotFound(id);
        }

        entity.IsActive = false;
        await db.SaveChangesAsync(ct);

        return Result.Success();
    }

    private async Task<bool> NameExistsAsync(
        string name,
        Guid? excludeId = null,
        CancellationToken ct = default)
    {
        var normalized = name.Trim();

        var query = db.MarcasAutos
            .AsNoTracking()
            .Where(m => m.Name == normalized);

        if (excludeId.HasValue)
        {
            query = query.Where(m => m.Id != excludeId.Value);
        }

        return await query.AnyAsync(ct);
    }

    private static bool IsUniqueViolation(DbUpdateException ex) =>
        ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation };

    private static MarcaAutoDto ToDto(MarcaAuto entity) => new(
        entity.Id,
        entity.Name,
        entity.Description,
        entity.OriginCountry,
        entity.IsActive,
        entity.CreatedDate,
        entity.UpdatedDate);
}
