FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["UT.MicroserviceEco.OrderService/UT.MicroserviceEco.OrderService.csproj", "UT.MicroserviceEco.OrderService/"]
COPY ["UT.MicroserviceEco.ProductService.Sdk/UT.MicroserviceEco.ProductService.Sdk.csproj", "UT.MicroserviceEco.ProductService.Sdk/"]
COPY ["UT.MicroserviceEco.IntegrationContracts/UT.MicroserviceEco.IntegrationContracts.csproj", "UT.MicroserviceEco.IntegrationContracts/"]
COPY ["UT.MicroserviceEco.Host/UT.MicroserviceEco.Host.ServiceDefaults/UT.MicroserviceEco.Host.ServiceDefaults.csproj", "UT.MicroserviceEco.Host/UT.MicroserviceEco.Host.ServiceDefaults/"]
RUN dotnet restore "./UT.MicroserviceEco.OrderService/UT.MicroserviceEco.OrderService.csproj"
COPY . .
WORKDIR "/src/UT.MicroserviceEco.OrderService"
RUN dotnet build "./UT.MicroserviceEco.OrderService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./UT.MicroserviceEco.OrderService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "UT.MicroserviceEco.OrderService.dll"]
