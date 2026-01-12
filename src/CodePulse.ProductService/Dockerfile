FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5003

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/CodePulse.ProductService/CodePulse.ProductService.csproj", "src/CodePulse.ProductService/"]
COPY ["src/CodePulse.Shared/CodePulse.Shared.csproj", "src/CodePulse.Shared/"]
RUN dotnet restore "src/CodePulse.ProductService/CodePulse.ProductService.csproj"
COPY . .
WORKDIR "/src/src/CodePulse.ProductService"
RUN dotnet build "CodePulse.ProductService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CodePulse.ProductService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CodePulse.ProductService.dll"]
