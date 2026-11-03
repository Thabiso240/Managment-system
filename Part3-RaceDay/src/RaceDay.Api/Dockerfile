# ---------------------------------------------------------------------------
# RaceDay API - multi-stage Dockerfile
# Stage 1 restores and publishes the application, stage 2 runs it on the
# lightweight ASP.NET Core runtime image.
# ---------------------------------------------------------------------------

# -----------------------------
# Build stage
# -----------------------------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Restore first so that NuGet packages are cached in their own layer - the
# restore step only re-runs when the project file changes.
COPY RaceDay.Api.csproj ./
RUN dotnet restore RaceDay.Api.csproj

# Copy the remaining source and publish a release build.
COPY . ./
RUN dotnet publish RaceDay.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

# -----------------------------
# Runtime stage
# -----------------------------
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Listen on the container port, not on the development loopback address.
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

# Writable folders for the SQLite database and locally stored uploads.
RUN mkdir -p /app/data /app/wwwroot/uploads

COPY --from=build /app/publish ./

ENTRYPOINT ["dotnet", "RaceDay.Api.dll"]
