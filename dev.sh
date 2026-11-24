#!/usr/bin/env bash
# Arranque de desarrollo de BrandCars: cierra instancias anteriores, restaura, avisa de modelo
# sin migrar y lanza la API con hot reload. Las migraciones (incluido el seed HasData) las
# aplica la propia app en Program.cs al arrancar en Development.
set -euo pipefail

cd "$(dirname "$0")"

# Debe coincidir con profiles.http.applicationUrl en BrandCars.Api/Properties/launchSettings.json.
PORT=5137
API_PROJECT=BrandCars.Api
SOLUTION=BrandCars.slnx

# AddInfrastructure lee ConnectionStrings:DefaultConnection desde appsettings.Development.json.
export ASPNETCORE_ENVIRONMENT=Development

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
pkill -f "dotnet watch run --project $API_PROJECT" 2>/dev/null || true
pkill -f "dotnet-watch.dll run --project $API_PROJECT" 2>/dev/null || true
sleep 1
pkill -9 -f "dotnet watch run --project $API_PROJECT" 2>/dev/null || true
pkill -9 -f "dotnet-watch.dll run --project $API_PROJECT" 2>/dev/null || true

# 3. Si aún está ocupado, fallar temprano con información accionable.
if lsof -nP -tiTCP:$PORT -sTCP:LISTEN >/dev/null 2>&1; then
  echo "    ⚠️  El puerto $PORT sigue ocupado:"
  lsof -nP -iTCP:$PORT -sTCP:LISTEN || true
  echo "    Detén ese proceso manualmente y vuelve a ejecutar ./dev.sh"
  exit 1
fi
echo "    Puerto $PORT libre."

echo "==> Restore"
dotnet restore "$SOLUTION"

echo "==> ¿Cambios de modelo sin migrar?"
if ! dotnet ef --version >/dev/null 2>&1; then
  echo "    ⚠️  dotnet-ef no está instalado; se omite la comprobación."
  echo "    Instálalo con: dotnet tool install --global dotnet-ef"
elif EF_OUTPUT=$(dotnet ef migrations has-pending-model-changes \
      --project BrandCars.Infra --startup-project "$API_PROJECT" 2>&1); then
  echo "    Modelo alineado con las migraciones."
else
  echo "    ⚠️  Hay cambios de modelo sin migrar (o falló la comprobación):"
  echo "$EF_OUTPUT" | sed 's/^/       /'
  echo "    Ejecuta: dotnet ef migrations add <Nombre> --project BrandCars.Infra --startup-project $API_PROJECT"
fi

echo "==> Arrancando API con watch (http://localhost:$PORT/scalar)"
echo "    El perfil 'http' abre el navegador en Scalar."
echo "    Migraciones y seed HasData se aplican al arrancar (Database:MigrateOnStartup=true en Development)."
dotnet watch run --project "$API_PROJECT" --launch-profile http
