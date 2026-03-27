# ── Stage 1: Build ────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy ApiGateway
COPY src/CodePulse.ApiGateway/ src/CodePulse.ApiGateway/

# Restore & Publish
WORKDIR /src/src/CodePulse.ApiGateway
RUN dotnet publish -c Release -o /app/publish --no-self-contained

# ── Stage 2: Runtime ───────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 5000
ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "CodePulse.ApiGateway.dll"]
