FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Directory.Build.props carries the target framework, so it must be restored
# before any csproj is evaluated.
COPY Directory.Build.props ./
COPY TenantManagement.sln ./
COPY src/TenantManagement.Core/TenantManagement.Core.csproj src/TenantManagement.Core/
COPY src/TenantManagement.Application/TenantManagement.Application.csproj src/TenantManagement.Application/
COPY src/TenantManagement.Infrastructure/TenantManagement.Infrastructure.csproj src/TenantManagement.Infrastructure/
COPY src/TenantManagement.Api/TenantManagement.Api.csproj src/TenantManagement.Api/

RUN dotnet restore src/TenantManagement.Api/TenantManagement.Api.csproj

COPY src/ src/
RUN dotnet publish src/TenantManagement.Api/TenantManagement.Api.csproj \
    -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish ./

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

USER $APP_UID

ENTRYPOINT ["dotnet", "TenantManagement.Api.dll"]
