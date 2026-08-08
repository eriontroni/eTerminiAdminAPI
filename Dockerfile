# syntax=docker/dockerfile:1
#
# eTermini Admin API
#
# KUJDES: projektet e këtij repo-je referojnë eTerminiAPI me shtigje relative
# (..\..\eTerminiAPI\...), prandaj build context-i duhet të jetë DOSJA PRIND
# që i mban të dy repot krah për krah:
#
#   <workspace>/
#     ├── eTerminiAPI/
#     └── eTerminiAdminAPI/
#
#   docker build -f eTerminiAdminAPI/Dockerfile -t etermini-adminapi .
#
# (docker-compose.yml i repo-s së deploy-it e vendos context-in saktë automatikisht.)

# ---------- build ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Fillimisht vetëm .csproj-at, për cache të restore-it.
COPY eTerminiAdminAPI/eTerminiAdminAPI.API/eTerminiAdminAPI.API.csproj                       eTerminiAdminAPI/eTerminiAdminAPI.API/
COPY eTerminiAdminAPI/eTerminiAdminAPI.Application/eTerminiAdminAPI.Application.csproj       eTerminiAdminAPI/eTerminiAdminAPI.Application/
COPY eTerminiAdminAPI/eTerminiAdminAPI.Infrastructure/eTerminiAdminAPI.Infrastructure.csproj eTerminiAdminAPI/eTerminiAdminAPI.Infrastructure/
COPY eTerminiAPI/eTerminiApi.Application/eTerminiAPI.Application.csproj                      eTerminiAPI/eTerminiApi.Application/
COPY eTerminiAPI/eTerminiAPI.Domain/eTerminiAPI.Domain.csproj                                eTerminiAPI/eTerminiAPI.Domain/
COPY eTerminiAPI/eTerminiAPI.Infrastructure/eTerminiAPI.Infrastructure.csproj                eTerminiAPI/eTerminiAPI.Infrastructure/

RUN dotnet restore eTerminiAdminAPI/eTerminiAdminAPI.API/eTerminiAdminAPI.API.csproj

# Pastaj kodin — nga të dy repot, por vetëm projektet që na duhen.
COPY eTerminiAdminAPI/eTerminiAdminAPI.API/            eTerminiAdminAPI/eTerminiAdminAPI.API/
COPY eTerminiAdminAPI/eTerminiAdminAPI.Application/    eTerminiAdminAPI/eTerminiAdminAPI.Application/
COPY eTerminiAdminAPI/eTerminiAdminAPI.Infrastructure/ eTerminiAdminAPI/eTerminiAdminAPI.Infrastructure/
COPY eTerminiAPI/eTerminiApi.Application/              eTerminiAPI/eTerminiApi.Application/
COPY eTerminiAPI/eTerminiAPI.Domain/                   eTerminiAPI/eTerminiAPI.Domain/
COPY eTerminiAPI/eTerminiAPI.Infrastructure/           eTerminiAPI/eTerminiAPI.Infrastructure/

RUN dotnet publish eTerminiAdminAPI/eTerminiAdminAPI.API/eTerminiAdminAPI.API.csproj \
      -c Release \
      -o /app/publish \
      --no-restore \
      /p:UseAppHost=false

# ---------- runtime ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

RUN apt-get update \
 && apt-get install -y --no-install-recommends curl \
 && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

RUN mkdir -p /app/logs && chown -R app:app /app/logs
USER app

ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_HTTP_PORTS=8080 \
    DOTNET_RUNNING_IN_CONTAINER=true

EXPOSE 8080

ENTRYPOINT ["dotnet", "eTerminiAdminAPI.API.dll"]
