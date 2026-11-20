using autos.Api.Features.MarcaAutos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace autos.Api.Interceptors;


public sealed class SoftDeleteInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        ApplySoftDelete(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ApplySoftDelete(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void ApplySoftDelete(DbContext? context)
    {
        if (context is null) return;

        var deleted = context.ChangeTracker
            .Entries<ISoftDeletable>()
            .Where(e => e.State == EntityState.Deleted)
            .ToList();

        foreach (var entry in deleted)
        {
            // Avoid direct delete of a resource
            entry.State = EntityState.Unchanged;
            entry.Property(e => e.IsActive).CurrentValue = false;
            entry.Property(e => e.IsActive).IsModified = true;
        }
    }
}