FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["UT.MicroserviceEco.AuthService/UT.MicroserviceEco.AuthService.csproj", "UT.MicroserviceEco.AuthService/"]
COPY ["UT.MicroserviceEco.Host/UT.MicroserviceEco.Host.ServiceDefaults/UT.MicroserviceEco.Host.ServiceDefaults.csproj", "UT.MicroserviceEco.Host/UT.MicroserviceEco.Host.ServiceDefaults/"]
RUN dotnet restore "./UT.MicroserviceEco.AuthService/UT.MicroserviceEco.AuthService.csproj"
COPY . .
WORKDIR "/src/UT.MicroserviceEco.AuthService"
RUN dotnet build "./UT.MicroserviceEco.AuthService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./UT.MicroserviceEco.AuthService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "UT.MicroserviceEco.AuthService.dll"]
