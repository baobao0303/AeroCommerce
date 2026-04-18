# ── Stage 1: Build ────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy Shared library
COPY src/AeroCommerce.Shared/ src/AeroCommerce.Shared/

# Copy PostService
COPY src/AeroCommerce.PostService/ src/AeroCommerce.PostService/

# Restore & Publish
WORKDIR /src/src/AeroCommerce.PostService
RUN dotnet publish -c Release -o /app/publish --no-self-contained

# ── Stage 2: Runtime ───────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 5002
ENV ASPNETCORE_URLS=http://+:5002
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "AeroCommerce.PostService.dll"]
