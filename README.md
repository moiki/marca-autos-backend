# marca-autos-backend

API REST para administrar marcas de autos, construida con .NET 10, EF Core y PostgreSQL.

## Arranque en desarrollo

```bash
./dev.sh
```

El script cierra instancias anteriores del API, restaura dependencias, avisa si hay cambios de modelo sin migrar y levanta la API con hot reload en `http://localhost:5191`. Las migraciones y el seed (`Toyota`, `Honda`, `Ford`, `BMW`) se aplican automáticamente al arrancar.

Con la API en marcha, la interfaz web para explorar y probar los endpoints está en **http://localhost:5191/scalar**. El documento OpenAPI está en `/openapi/v1.json`. Ambos se exponen únicamente en el entorno `Development`; en `Production` devuelven 404.

## Arranque con Docker Compose

Requiere Docker Desktop en ejecución. Levanta PostgreSQL y la API en contenedores:

```bash
docker compose up --build
```

Para dejarlo corriendo en segundo plano:

```bash
docker compose up -d --build
```

| Servicio | URL / conexión |
|---|---|
| API | `http://localhost:8080` |
| PostgreSQL (desde el host) | `localhost:5433`, base `autos`, usuario `autos`, password `autos` |
| PostgreSQL (desde la red de compose) | `postgres:5432` |

Comandos útiles:

```bash
docker compose ps                      # estado de los contenedores
docker compose logs -f autos-api       # logs de la API
docker compose down                    # detener y eliminar contenedores
docker compose down -v                 # además, borrar el volumen de datos
```

Notas:

- El compose usa el puerto host **5433** para no chocar con un PostgreSQL local en 5432.
- El contenedor de la API corre con `ASPNETCORE_ENVIRONMENT=Production` y se conecta a `postgres:5432`.
- Las migraciones y el seed `HasData` se aplican al arrancar la API, así que no hay pasos manuales de base de datos.
- Los datos se conservan en el volumen `postgres-data`; `docker compose down -v` los elimina.

## Requisitos

- .NET 10 SDK y herramienta global `dotnet-ef` (arranque local)
- PostgreSQL local con la base de datos `autos` y las credenciales de `appsettings.Development.json` (arranque local)
- Docker Desktop (arranque con compose)
