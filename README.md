# BrandCars — API de marcas de autos

API REST construida con **.NET 10**, **EF Core + Npgsql** y **PostgreSQL**, organizada con arquitectura hexagonal. Expone un endpoint de solo lectura (`GET /api/marcas-autos`), un health check y documentación OpenAPI con Scalar.

## Inicio rápido

Elige una vía:

| Vía | Cuándo usarla | Comando | API |
|---|---|---|---|
| **Local** con PostgreSQL + `./dev.sh` | Desarrollo con hot reload | `./dev.sh` | <http://localhost:5137> |
| **Docker Compose** | Stack completo (API + BD) en contenedores | `docker compose up --build` | <http://localhost:8080> |

En ambas vías las migraciones y el seed (`Toyota`, `Ford`, `Volkswagen`) se aplican automáticamente al arrancar: **no hay que ejecutar SQL manualmente**.

## Requisitos

| Para | Necesitas |
|---|---|
| Arranque local | .NET 10 SDK y PostgreSQL accesible en `localhost:5432` |
| Arranque local (opcional) | `dotnet-ef` como herramienta global, solo para el aviso de cambios de modelo sin migrar |
| Docker Compose | Docker Engine / Docker Desktop con Compose v2 |
| Probar la API | `curl`, el archivo `BrandCars.Api/BrandCars.http` o la UI de Scalar |

## Opción A — Local con PostgreSQL y `./dev.sh`

La API lee `ConnectionStrings:DefaultConnection` de `BrandCars.Api/appsettings.Development.json`:

```text
Host=localhost;Port=5432;Database=autos;Username=autos;Password=autos
```

### 1. Prepara la base de datos

Si PostgreSQL ya está instalado en la máquina, crea el rol y la base una sola vez:

```bash
psql -U postgres -c "CREATE ROLE autos WITH LOGIN PASSWORD 'autos';"
psql -U postgres -c "CREATE DATABASE autos OWNER autos;"
```

Si prefieres no instalar PostgreSQL localmente, levanta solo la BD en Docker (el puerto **5432** es el que espera `dev.sh`):

```bash
docker run --name brandcars-postgres-local \
  -e POSTGRES_DB=autos -e POSTGRES_USER=autos -e POSTGRES_PASSWORD=autos \
  -p 5432:5432 -d postgres:17-alpine
```

### 2. Arranca la API

```bash
./dev.sh
```

El script, desde la raíz del repositorio:

1. Libera el puerto `5137` (cierra instancias anteriores y watchers de `dotnet watch`).
2. Ejecuta `dotnet restore BrandCars.slnx`.
3. Comprueba `dotnet ef migrations has-pending-model-changes` si `dotnet-ef` está instalado.
4. Lanza `dotnet watch run --project BrandCars.Api --launch-profile http` con `ASPNETCORE_ENVIRONMENT=Development`.

> Si no quieres usar el script: `ASPNETCORE_ENVIRONMENT=Development dotnet run --project BrandCars.Api --launch-profile http`.

### 3. Verifica

```bash
curl http://localhost:5137/api/marcas-autos
curl http://localhost:5137/health
```

Abre <http://localhost:5137/scalar> para la UI de Scalar o <http://localhost:5137/openapi/v1.json> para el documento OpenAPI.

## Opción B — Docker Compose

Levanta PostgreSQL y la API en contenedores, sin instalar .NET ni PostgreSQL en el host:

```bash
docker compose up --build        # primer arranque, en primer plano
docker compose up -d --build     # en segundo plano
```

Comandos útiles:

```bash
docker compose ps                       # estado de los contenedores
docker compose logs -f brandcars-api    # logs de la API
docker compose down                     # detener y eliminar contenedores
docker compose down -v                  # además, eliminar el volumen de datos
```

| Servicio / contenedor | Expuesto en el host | Uso |
|---|---|---|
| `postgres` / `brandcars-postgres` | `localhost:5433` | BD `autos`, usuario `autos`, password `autos` |
| `brandcars-api` / `brandcars-api` | <http://localhost:8080> | API, `/scalar`, `/health` |

Dentro de la red de Compose la BD se alcanza como `postgres:5432`.

Notas:

- El host expone **5433** para no chocar con un PostgreSQL local en 5432; dentro del contenedor se usa el 5432 de siempre.
- La API corre con `ASPNETCORE_ENVIRONMENT=Production`, pero Compose inyecta `ConnectionStrings__DefaultConnection` y `Database__MigrateOnStartup=true`, así que migraciones y seed se aplican igualmente.
- Los datos persisten en el volumen `postgres-data`; `docker compose down -v` los borra.
- La API espera a que el healthcheck de PostgreSQL esté sano antes de arrancar (`depends_on: condition: service_healthy`).

## Endpoints

| Método | Ruta | Descripción |
|---|---|---|
| `GET` | `/api/marcas-autos` | Marcas activas, ordenadas por nombre |
| `GET` | `/health` | Health check de la aplicación |
| `GET` | `/scalar` | UI interactiva de OpenAPI (Scalar) |
| `GET` | `/openapi/v1.json` | Documento OpenAPI generado por ASP.NET Core |

Ejemplo de respuesta:

```json
[
  {
    "id": "22222222-2222-2222-2222-222222222222",
    "name": "Ford",
    "description": "Fabricante estadounidense de automóviles.",
    "originCountry": "Estados Unidos",
    "isActive": true,
    "createdAtUtc": "2025-01-01T00:00:00Z",
    "updatedAtUtc": null
  }
]
```

## Arquitectura

Las dependencias apuntan hacia el centro (Domain no conoce a nadie):

| Proyecto | Responsabilidad | Depende de |
|---|---|---|
| `BrandCars.Domain` | Entidades y contratos (`MarcaAuto`, `ISoftDeletable`); sin paquetes NuGet | — |
| `BrandCars.Application` | Casos de uso, DTOs y puertos (`IGetMarcasAutos`, `IMarcaAutoRepository`) | Domain |
| `BrandCars.Infra` | EF Core, Npgsql, repositorio, configuración y migraciones | Application |
| `BrandCars.Api` | Controlador, middleware de excepciones, composición y OpenAPI | Application + Infra |

`MarcasAutosController` es el adaptador entrante; `MarcaAutoRepository` es el adaptador saliente. El filtro de soft delete y el seed `HasData` viven en Infra (`MarcaAutoConfiguration`).

## Configuración

| Clave | Local (`Development`) | Docker Compose |
|---|---|---|
| `ConnectionStrings:DefaultConnection` | `appsettings.Development.json` → `localhost:5432` | Variable `ConnectionStrings__DefaultConnection` → `postgres:5432` |
| `Database:MigrateOnStartup` | `true` (`appsettings.Development.json`) | `true` (variable `Database__MigrateOnStartup`) |
| `ASPNETCORE_ENVIRONMENT` | `Development` (lo fija `dev.sh`) | `Production` |

`appsettings.json` trae `MigrateOnStartup: false` para que un despliegue con varias réplicas no ejecute migraciones concurrentes; habilítalo solo en desarrollo o en un paso controlado de despliegue.

## Solución de problemas

| Síntoma | Acción |
|---|---|
| `Falta la cadena de conexión 'ConnectionStrings:DefaultConnection'` | Define la variable de entorno o completa `appsettings.Development.json` / `ConnectionStrings__DefaultConnection` en Compose. |
| `El puerto 5137 sigue ocupado` al ejecutar `./dev.sh` | `lsof -nP -iTCP:5137 -sTCP:LISTEN`, detén ese proceso y vuelve a intentarlo. |
| `dotnet-ef no está instalado` | `dotnet tool install --global dotnet-ef` (es solo el aviso de cambios pendientes; no bloquea el arranque). |
| El contenedor de la API no arranca | `docker compose logs brandcars-api` y comprueba que `docker compose ps` muestre el healthcheck de `postgres` como `healthy`. |
| Error al conectar desde el host a la BD de Compose | Usa el puerto **5433**, no el 5432. |

## Referencias

- [`docs/follow-up.md`](docs/follow-up.md) — decisiones y trabajo pendiente (soft delete automático, pruebas, etc.).
- [`BrandCars.Api/BrandCars.http`](BrandCars.Api/BrandCars.http) — peticiones listas para ejecutar desde el IDE.
- [`dev.sh`](dev.sh) — arranque local con hot reload.
