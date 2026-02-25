FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["Common/Common.csproj", "Common/"]
COPY ["Services/EventService/EventService_Api/EventService_Api.csproj", "Services/EventService/EventService_Api/"]
COPY ["Services/EventService/EventService_Application/EventService_Application.csproj", "Services/EventService/EventService_Application/"]
COPY ["Services/EventService/EventService_Domain/EventService_Domain.csproj", "Services/EventService/EventService_Domain/"]
COPY ["Services/EventService/EventService_Infrastructure/EventService_Infrastructure.csproj", "Services/EventService/EventService_Infrastructure/"]

RUN dotnet restore "Services/EventService/EventService_Api/EventService_Api.csproj"

COPY Common/ Common/
COPY Services/EventService/ Services/EventService/

RUN dotnet publish "Services/EventService/EventService_Api/EventService_Api.csproj" \
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
ENTRYPOINT ["dotnet", "EventService_Api.dll"]
