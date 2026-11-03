# ---------------------------------------------------------------------------
# RaceDay MVC - multi-stage Dockerfile
# The published output is copied onto the ASP.NET Core runtime image that only
# contains what the application needs to run.
# ---------------------------------------------------------------------------

# -----------------------------
# Build stage
# -----------------------------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Restore first to keep NuGet packages in a dedicated cache layer.
COPY RaceDay.Mvc.csproj ./
RUN dotnet restore RaceDay.Mvc.csproj

COPY . ./
RUN dotnet publish RaceDay.Mvc.csproj -c Release -o /app/publish /p:UseAppHost=false

# -----------------------------
# Runtime stage
# -----------------------------
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

COPY --from=build /app/publish ./

# The MVC app talks to the API over the internal Docker network.
# Override ApiBaseUrl at run time, e.g. -e ApiBaseUrl=http://raceday-api:8080/
ENV ApiBaseUrl=http://raceday-api:8080/

ENTRYPOINT ["dotnet", "RaceDay.Mvc.dll"]
