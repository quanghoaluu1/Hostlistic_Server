FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["Common/Common.csproj", "Common/"]
COPY ["Services/NotificationService/NotificationService_Api/NotificationService_Api.csproj", "Services/NotificationService/NotificationService_Api/"]
COPY ["Services/NotificationService/NotificationService_Application/NotificationService_Application.csproj", "Services/NotificationService/NotificationService_Application/"]
COPY ["Services/NotificationService/NotificationService_Domain/NotificationService_Domain.csproj", "Services/NotificationService/NotificationService_Domain/"]
COPY ["Services/NotificationService/NotificationService_Infrastructure/NotificationService_Infrastructure.csproj", "Services/NotificationService/NotificationService_Infrastructure/"]

RUN dotnet restore "Services/NotificationService/NotificationService_Api/NotificationService_Api.csproj"

COPY Common/ Common/
COPY Services/NotificationService/ Services/NotificationService/

RUN dotnet publish "Services/NotificationService/NotificationService_Api/NotificationService_Api.csproj" \
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
ENTRYPOINT ["dotnet", "NotificationService_Api.dll"]
