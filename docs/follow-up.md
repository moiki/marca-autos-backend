# Follow-up — elementos para una siguiente fase

Items deliberadamente **fuera del alcance** de la fase actual (arquitectura hexagonal + `GET api/marcas-autos`). Registrarlos aquí evita improvisarlos dentro de la capa equivocada.

## 1. Soft delete automático con `SaveChangesInterceptor`

**Estado:** pendiente. No se implementó porque hoy no existen endpoints de escritura (POST/PUT/DELETE).

**Dónde va:** `BrandCars.Infra/Persistence/Interceptors/SoftDeleteInterceptor.cs`, namespace `BrandCars.Infra.Persistence.Interceptors`. Es un detalle de persistencia: manipula el `ChangeTracker` de EF Core, por lo que no puede ir en Domain ni en Application sin romper la regla de dependencia. `ISoftDeletable` se resuelve por transitividad (`Infra → Application → Domain`).

```csharp
using BrandCars.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace BrandCars.Infra.Persistence.Interceptors;

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
            // Evita el borrado físico: convierte el delete en una desactivación.
            entry.State = EntityState.Unchanged;
            entry.Property(e => e.IsActive).CurrentValue = false;
            entry.Property(e => e.IsActive).IsModified = true;
        }
    }
}
```

**Registro** en `BrandCars.Infra/DependencyInjection.cs`, dentro de `AddInfrastructure`:

```csharp
using BrandCars.Infra.Persistence.Interceptors;

services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(connectionString);
    options.AddInterceptors(new SoftDeleteInterceptor());
});
```

El interceptor es stateless, por eso se instancia con `new`. Si más adelante necesita `ILogger` u otra dependencia, registrar en DI y usar el overload de `AddDbContext` con `IServiceProvider`.

**Verificación cuando se implemente:** al eliminar una marca, la fila debe permanecer en la tabla con `IsActive = false` y debe desaparecer de `GET /api/marcas-autos` gracias al `HasQueryFilter(m => m.IsActive)` de `MarcaAutoConfiguration`.

**Atención:** `ISoftDeletable.IsActive` es solo get; hoy funciona porque `MarcaAuto.IsActive` tiene `private set`. Si la entidad cambia a `init` o pierde el setter, revisar la asignación `CurrentValue`.

---

## Otros pendientes detectados

- **Pruebas** unitarias y de cobertura del caso de uso (orden por nombre, mapeo a DTO, cancelación) y del repositorio.
- **Test de arquitectura** (NetArchTest / ArchUnitNET) para blindar la regla de dependencia y la ausencia de `PackageReference` en Domain/Application.
- **Docker**: `compose.yaml` apunta a `BrandCars/Dockerfile`, pero el Dockerfile real está en `BrandCars.Api/Dockerfile`; además hay que pasar la connection string al contenedor.
- **README** con las decisiones de arquitectura y cómo ejecutar la solución.
- **Seguridad**: el middleware de excepciones expone `ex.ToString()` (tal como pide el enunciado); limitar el detalle antes de exponer la API a producción.
- **Git**: el directorio aún no es repositorio; inicializarlo y crear un commit por fase si se decide versionar el proyecto.
- **Configuración local**: el rol `postgres` de la máquina no tiene permiso de login; la verificación se hizo con otra cuenta y una BD temporal. Ajustar `appsettings.Development.json` según el entorno de cada desarrollador.
