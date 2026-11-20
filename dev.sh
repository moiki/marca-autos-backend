#!/usr/bin/env bash
# Arranque de desarrollo: restaura, avisa de modelo sin migrar y lanza la API con hot reload.
# Las migraciones y los seeders los aplica la propia app en Program.cs al arrancar.
set -euo pipefail

cd "$(dirname "$0")"

echo "==> Restore"
dotnet restore autosBackend.slnx

echo "==> ¿Cambios de modelo sin migrar?"
if dotnet ef migrations has-pending-model-changes --project autos.Api; then
  echo "    Modelo alineado con las migraciones."
else
  echo "    ⚠️  Hay cambios de modelo sin migrar."
  echo "    Ejecuta: dotnet ef migrations add <Nombre> --project autos.Api"
fi

echo "==> Arrancando API con watch (http://localhost:5191)"
echo "    Migraciones y seeders se aplican automáticamente en el arranque."
dotnet watch run --project autos.Api --launch-profile http
