FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copiar primero los .csproj para aprovechar la caché de capas de restore.
COPY ["BrandCars.Api/BrandCars.Api.csproj", "BrandCars.Api/"]
COPY ["BrandCars.Application/BrandCars.Application.csproj", "BrandCars.Application/"]
COPY ["BrandCars.Domain/BrandCars.Domain.csproj", "BrandCars.Domain/"]
COPY ["BrandCars.Infra/BrandCars.Infra.csproj", "BrandCars.Infra/"]
RUN dotnet restore "BrandCars.Api/BrandCars.Api.csproj"

COPY . .
WORKDIR /src/BrandCars.Api
RUN dotnet build "BrandCars.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build --no-restore

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "BrandCars.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish --no-restore /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BrandCars.Api.dll"]
