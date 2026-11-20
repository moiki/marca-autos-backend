# marca-autos-backend

API REST para administrar marcas de autos, construida con .NET 10, EF Core y PostgreSQL.

## Arranque en desarrollo

```bash
./dev.sh
```

El script restaura dependencias, avisa si hay cambios de modelo sin migrar y levanta la API con hot reload en `http://localhost:5191`. Las migraciones se aplican automáticamente al arrancar; el seed de marcas (`Toyota`, `Honda`, `Ford`, `BMW`) solo se inserta en el entorno `Development`.

## Requisitos

- .NET 10 SDK y herramienta global `dotnet-ef`
- PostgreSQL local con la base de datos `autos` y las credenciales de `appsettings.Development.json`

También hay un `compose.yaml` para levantar la API junto con PostgreSQL en contenedores.
