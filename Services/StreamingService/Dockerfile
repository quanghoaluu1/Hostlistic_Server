FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["Common/Common.csproj", "Common/"]
COPY ["Services/StreamingService/StreamingService_Api/StreamingService_Api.csproj", "Services/StreamingService/StreamingService_Api/"]
COPY ["Services/StreamingService/StreamingService_Application/StreamingService_Application.csproj", "Services/StreamingService/StreamingService_Application/"]
COPY ["Services/StreamingService/StreamingService_Domain/StreamingService_Domain.csproj", "Services/StreamingService/StreamingService_Domain/"]
COPY ["Services/StreamingService/StreamingService_Infrastructure/StreamingService_Infrastructure.csproj", "Services/StreamingService/StreamingService_Infrastructure/"]

RUN dotnet restore "Services/StreamingService/StreamingService_Api/StreamingService_Api.csproj"

COPY Common/ Common/
COPY Services/StreamingService/ Services/StreamingService/

RUN dotnet publish "Services/StreamingService/StreamingService_Api/StreamingService_Api.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    /p:UseAppHost=false

FROM base AS final
WORKDIR /app
RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*
COPY --from=build /app/publish .

HEALTHCHECK --interval=30s --timeout=3s --start-period=10s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENTRYPOINT ["dotnet", "StreamingService_Api.dll"]
