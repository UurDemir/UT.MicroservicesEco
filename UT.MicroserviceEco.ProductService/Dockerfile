FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["UT.MicroserviceEco.ProductService/UT.MicroserviceEco.ProductService.csproj", "UT.MicroserviceEco.ProductService/"]
COPY ["UT.MicroserviceEco.Host/UT.MicroserviceEco.Host.ServiceDefaults/UT.MicroserviceEco.Host.ServiceDefaults.csproj", "UT.MicroserviceEco.Host/UT.MicroserviceEco.Host.ServiceDefaults/"]
RUN dotnet restore "./UT.MicroserviceEco.ProductService/UT.MicroserviceEco.ProductService.csproj"
COPY . .
WORKDIR "/src/UT.MicroserviceEco.ProductService"
RUN dotnet build "./UT.MicroserviceEco.ProductService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./UT.MicroserviceEco.ProductService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "UT.MicroserviceEco.ProductService.dll"]
