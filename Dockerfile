# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY backend/Directory.Build.props backend/
COPY backend/src/JotaNunesForms.Domain/JotaNunesForms.Domain.csproj backend/src/JotaNunesForms.Domain/
COPY backend/src/JotaNunesForms.Application/JotaNunesForms.Application.csproj backend/src/JotaNunesForms.Application/
COPY backend/src/JotaNunesForms.Infrastructure/JotaNunesForms.Infrastructure.csproj backend/src/JotaNunesForms.Infrastructure/
COPY backend/src/JotaNunesForms.Api/JotaNunesForms.Api.csproj backend/src/JotaNunesForms.Api/

RUN dotnet restore backend/src/JotaNunesForms.Api/JotaNunesForms.Api.csproj

COPY backend/ backend/
RUN dotnet publish backend/src/JotaNunesForms.Api/JotaNunesForms.Api.csproj \
    -c Release \
    -o /app \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
RUN useradd --create-home --uid 1001 appuser
COPY --from=build --chown=appuser:appuser /app .
USER appuser
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "JotaNunesForms.Api.dll"]
