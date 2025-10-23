# ===== Stage 1: build =====
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# copia csproj por separado para cacheo
COPY ["DiaplesWeb.csproj", "./"]
RUN dotnet restore "./DiaplesWeb.csproj"

# copia el resto y publica
COPY . .
RUN dotnet publish "DiaplesWeb.csproj" -c Release -o /app/publish

# ===== Stage 2: runtime =====
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Render te inyecta PORT; atamos Kestrel a ese puerto (o 8080 por defecto)
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

# copiamos la app publicada
COPY --from=build /app/publish .

# Usamos sh para respetar PORT: si no existe, 8080
ENTRYPOINT [ "sh", "-c", "dotnet DiaplesWeb.dll --urls http://0.0.0.0:${PORT:-8080}" ]
