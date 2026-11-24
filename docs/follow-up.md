# Follow-up — elementos para una siguiente fase

Items deliberadamente **fuera del alcance** de la fase actual (arquitectura hexagonal + `GET api/marcas-autos`). Registrarlos aquí evita improvisarlos dentro de la capa equivocada. Los que ya se cerraron se conservan al final con su resultado.

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

## Otros items detectados

### Pendientes

| Item | Siguiente paso |
|---|---|
| **Pruebas** | No hay proyecto de tests en la solución. Cubrir el caso de uso (orden por nombre, mapeo a DTO, cancelación) y el repositorio. |
| **Test de arquitectura** | NetArchTest / ArchUnitNET para blindar la regla de dependencia y la ausencia de `PackageReference` en Domain/Application. |
| **Seguridad** | `ExceptionHandlingMiddleware` expone `ex.ToString()` en el `ProblemDetails` (tal como pide el enunciado). Como Compose corre en `Production`, limitar el detalle antes de publicar la API. |

### Resueltos

| Item | Resultado |
|---|---|
| **Docker** | `compose.yaml` apunta a `BrandCars.Api/Dockerfile`, incluye el servicio `postgres` con healthcheck, puerto host `5433` y volumen `postgres-data`; la API recibe `ConnectionStrings__DefaultConnection` y `Database__MigrateOnStartup=true`. |
| **README** | `README.md` en la raíz documenta el arranque local con `./dev.sh`, el arranque con Docker Compose, los endpoints y la configuración. |
| **Git** | Repositorio inicializado en la rama `feat/apply-hex-arq`, con `origin` → `git@github_moiki:moiki/marca-autos-backend.git` e historial por fases (`first commit`, seed, `dev.sh`, README, Scalar, arquitectura hexagonal). |
| **Configuración local** | Documentada en `README.md`: crear el rol/base `autos` o levantar PostgreSQL en Docker; `appsettings.Development.json` apunta a `localhost:5432` y el `5433` queda para el PostgreSQL de Compose. |

> Nota de entorno: en la máquina donde se validó, el rol `postgres` no permite login; se usó otra cuenta y una BD temporal. Cada desarrollador debe ajustar `appsettings.Development.json` a su entorno.
