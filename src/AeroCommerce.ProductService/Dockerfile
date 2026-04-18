FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5003

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/AeroCommerce.ProductService/AeroCommerce.ProductService.csproj", "src/AeroCommerce.ProductService/"]
COPY ["src/AeroCommerce.Shared/AeroCommerce.Shared.csproj", "src/AeroCommerce.Shared/"]
RUN dotnet restore "src/AeroCommerce.ProductService/AeroCommerce.ProductService.csproj"
COPY . .
WORKDIR "/src/src/AeroCommerce.ProductService"
RUN dotnet build "AeroCommerce.ProductService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AeroCommerce.ProductService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AeroCommerce.ProductService.dll"]
