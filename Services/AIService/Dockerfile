FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["Common/Common.csproj", "Common/"]
COPY ["Services/AIService/AIService_Api/AIService_Api.csproj", "Services/AIService/AIService_Api/"]
COPY ["Services/AIService/AIService_Application/AIService_Application.csproj", "Services/AIService/AIService_Application/"]
COPY ["Services/AIService/AIService_Domain/AIService_Domain.csproj", "Services/AIService/AIService_Domain/"]
COPY ["Services/AIService/AIService_Infrastructure/AIService_Infrastructure.csproj", "Services/AIService/AIService_Infrastructure/"]

RUN dotnet restore "Services/AIService/AIService_Api/AIService_Api.csproj"

COPY Common/ Common/
COPY Services/AIService/ Services/AIService/

RUN dotnet publish "Services/AIService/AIService_Api/AIService_Api.csproj" \
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
ENTRYPOINT ["dotnet", "AIService_Api.dll"]
