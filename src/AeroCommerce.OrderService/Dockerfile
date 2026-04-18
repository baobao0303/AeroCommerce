FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5003

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/AeroCommerce.OrderService/AeroCommerce.OrderService.csproj", "src/AeroCommerce.OrderService/"]
COPY ["src/AeroCommerce.Shared/AeroCommerce.Shared.csproj", "src/AeroCommerce.Shared/"]
RUN dotnet restore "src/AeroCommerce.OrderService/AeroCommerce.OrderService.csproj"
COPY . .
WORKDIR "/src/src/AeroCommerce.OrderService"
RUN dotnet build "AeroCommerce.OrderService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AeroCommerce.OrderService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AeroCommerce.OrderService.dll"]
