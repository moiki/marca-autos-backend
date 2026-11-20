#!/usr/bin/env bash
# Arranque de desarrollo: cierra instancias anteriores, restaura, avisa de modelo
# sin migrar y lanza la API con hot reload. Las migraciones (incluido el seed
# HasData) las aplica la propia app en Program.cs al arrancar.
set -euo pipefail

cd "$(dirname "$0")"

PORT=5191

echo "==> Limpiando instancias anteriores del API"

# 1. Dueño actual del puerto (terminación graceful y, si insiste, forzada).
for _ in 1 2 3; do
  PORT_PIDS=$(lsof -nP -tiTCP:$PORT -sTCP:LISTEN 2>/dev/null || true)
  [ -z "$PORT_PIDS" ] && break
  echo "$PORT_PIDS" | while read -r pid; do
    [ -n "$pid" ] || continue
    echo "    Cerrando PID $pid (puerto $PORT)"
    kill "$pid" 2>/dev/null || true
  done
  sleep 1
done
PORT_PIDS=$(lsof -nP -tiTCP:$PORT -sTCP:LISTEN 2>/dev/null || true)
if [ -n "$PORT_PIDS" ]; then
  echo "$PORT_PIDS" | while read -r pid; do
    [ -n "$pid" ] || continue
    echo "    Forzando cierre de PID $pid"
    kill -9 "$pid" 2>/dev/null || true
  done
  sleep 1
fi

# 2. Watchers de este proyecto: si sobreviven, reinician al hijo y recuperan el puerto.
pkill -f 'dotnet watch run --project autos.Api' 2>/dev/null || true
pkill -f 'dotnet-watch.dll run --project autos.Api' 2>/dev/null || true
sleep 1
pkill -9 -f 'dotnet watch run --project autos.Api' 2>/dev/null || true
pkill -9 -f 'dotnet-watch.dll run --project autos.Api' 2>/dev/null || true

# 3. Si aún está ocupado, fallar temprano con información accionable.
if lsof -nP -tiTCP:$PORT -sTCP:LISTEN >/dev/null 2>&1; then
  echo "    ⚠️  El puerto $PORT sigue ocupado:"
  lsof -nP -iTCP:$PORT -sTCP:LISTEN || true
  echo "    Detén ese proceso manualmente y vuelve a ejecutar ./dev.sh"
  exit 1
fi
echo "    Puerto $PORT libre."

echo "==> Restore"
dotnet restore autosBackend.slnx

echo "==> ¿Cambios de modelo sin migrar?"
if dotnet ef migrations has-pending-model-changes --project autos.Api; then
  echo "    Modelo alineado con las migraciones."
else
  echo "    ⚠️  Hay cambios de modelo sin migrar."
  echo "    Ejecuta: dotnet ef migrations add <Nombre> --project autos.Api"
fi

echo "==> Arrancando API con watch (http://localhost:$PORT)"
echo "    Migraciones y seed HasData se aplican automáticamente en el arranque."
dotnet watch run --project autos.Api --launch-profile http
