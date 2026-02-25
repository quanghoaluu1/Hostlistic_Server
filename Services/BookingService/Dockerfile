FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["Common/Common.csproj", "Common/"]
COPY ["Services/BookingService/BookingService_Api/BookingService_Api.csproj", "Services/BookingService/BookingService_Api/"]
COPY ["Services/BookingService/BookingService_Application/BookingService_Application.csproj", "Services/BookingService/BookingService_Application/"]
COPY ["Services/BookingService/BookingService_Domain/BookingService_Domain.csproj", "Services/BookingService/BookingService_Domain/"]
COPY ["Services/BookingService/BookingService_Infrastructure/BookingService_Infrastructure.csproj", "Services/BookingService/BookingService_Infrastructure/"]

RUN dotnet restore "Services/BookingService/BookingService_Api/BookingService_Api.csproj"

COPY Common/ Common/
COPY Services/BookingService/ Services/BookingService/

RUN dotnet publish "Services/BookingService/BookingService_Api/BookingService_Api.csproj" \
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
ENTRYPOINT ["dotnet", "BookingService_Api.dll"]
