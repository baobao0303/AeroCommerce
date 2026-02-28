FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5003

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/CodePulse.OrderService/CodePulse.OrderService.csproj", "src/CodePulse.OrderService/"]
COPY ["src/CodePulse.Shared/CodePulse.Shared.csproj", "src/CodePulse.Shared/"]
RUN dotnet restore "src/CodePulse.OrderService/CodePulse.OrderService.csproj"
COPY . .
WORKDIR "/src/src/CodePulse.OrderService"
RUN dotnet build "CodePulse.OrderService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CodePulse.OrderService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CodePulse.OrderService.dll"]
