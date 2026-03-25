
# ── Stage 1: build & publish ──────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copia i file di progetto e ripristina le dipendenze (layer cacheable)
COPY CQRS.POC.slnx ./
COPY src/Domain/CQRS.POC.Domain.csproj             src/Domain/
COPY src/Application/CQRS.POC.Application.csproj   src/Application/
COPY src/Infrastructure/CQRS.POC.Infrastructure.csproj src/Infrastructure/
COPY src/api/CQRS.POC.API.csproj                   src/api/

RUN dotnet restore CQRS.POC.slnx

# Copia il sorgente e pubblica in modalità Release
COPY . .
RUN dotnet publish src/api/CQRS.POC.API.csproj \
        --configuration Release \
        --no-restore \
        --output /app/publish

# ── Stage 2: runtime ──────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

# Porta su cui ASP.NET Core ascolta dentro il container (default .NET 8+)
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

# Il file SQLite viene creato in /app/cqrs-poc.db (EnsureCreated all'avvio).
# Monta un volume su /app per persistere il database tra i riavvii del container.
VOLUME ["/app"]

ENTRYPOINT ["dotnet", "CQRS.POC.API.dll"]
