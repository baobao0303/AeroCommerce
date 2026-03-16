# ── Stage 1: Build ────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy Shared library
COPY src/CodePulse.Shared/ src/CodePulse.Shared/

# Copy PostService
COPY src/CodePulse.PostService/ src/CodePulse.PostService/

# Restore & Publish
WORKDIR /src/src/CodePulse.PostService
RUN dotnet publish -c Release -o /app/publish --no-self-contained

# ── Stage 2: Runtime ───────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 5002
ENV ASPNETCORE_URLS=http://+:5002
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "CodePulse.PostService.dll"]
