FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["Common/Common.csproj", "Common/"]
COPY ["Services/IdentityService/IdentityService_Api/IdentityService_Api.csproj", "Services/IdentityService/IdentityService_Api/"]
COPY ["Services/IdentityService/IdentityService_Application/IdentityService_Application.csproj", "Services/IdentityService/IdentityService_Application/"]
COPY ["Services/IdentityService/IdentityService_Domain/IdentityService_Domain.csproj", "Services/IdentityService/IdentityService_Domain/"]
COPY ["Services/IdentityService/IdentityService_Infrastructure/IdentityService_Infrastructure.csproj", "Services/IdentityService/IdentityService_Infrastructure/"]

RUN dotnet restore "Services/IdentityService/IdentityService_Api/IdentityService_Api.csproj"

COPY Common/ Common/
COPY Services/IdentityService/ Services/IdentityService/

RUN dotnet publish "Services/IdentityService/IdentityService_Api/IdentityService_Api.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

HEALTHCHECK --interval=30s --timeout=3s --start-period=10s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENTRYPOINT ["dotnet", "IdentityService_Api.dll"]