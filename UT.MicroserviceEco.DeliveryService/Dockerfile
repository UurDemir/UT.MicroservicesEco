FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["UT.MicroserviceEco.DeliveryService/UT.MicroserviceEco.DeliveryService.csproj", "UT.MicroserviceEco.DeliveryService/"]
COPY ["UT.MicroserviceEco.IntegrationContracts/UT.MicroserviceEco.IntegrationContracts.csproj", "UT.MicroserviceEco.IntegrationContracts/"]
COPY ["UT.MicroserviceEco.Host/UT.MicroserviceEco.Host.ServiceDefaults/UT.MicroserviceEco.Host.ServiceDefaults.csproj", "UT.MicroserviceEco.Host/UT.MicroserviceEco.Host.ServiceDefaults/"]
RUN dotnet restore "./UT.MicroserviceEco.DeliveryService/UT.MicroserviceEco.DeliveryService.csproj"
COPY . .
WORKDIR "/src/UT.MicroserviceEco.DeliveryService"
RUN dotnet build "./UT.MicroserviceEco.DeliveryService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./UT.MicroserviceEco.DeliveryService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "UT.MicroserviceEco.DeliveryService.dll"]
