# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Hostlistic Server is a **.NET 10 microservices** application for event management, built with **Clean Architecture**. The frontend (Next.js on `http://localhost:3000`) is a separate repository.

## Commands

### Run a service
```bash
dotnet run --project Services/EventService/EventService_Api/EventService_Api.csproj
dotnet run --project Services/IdentityService/IdentityService_Api/IdentityService_Api.csproj
dotnet run --project Services/BookingService/BookingService_Api/BookingService_Api.csproj
```

### Build
```bash
dotnet build                         # Build all projects
dotnet build Services/EventService/  # Build specific service
```

### Migrations (EF Core)
```bash
# Run from repo root; --project points to the Infrastructure layer
dotnet ef migrations add <MigrationName> \
  --project Services/EventService/EventService_Infrastructure \
  --startup-project Services/EventService/EventService_Api

dotnet ef database update \
  --project Services/EventService/EventService_Infrastructure \
  --startup-project Services/EventService/EventService_Api
```

### Docker
```bash
docker compose up
```

## Service Ports

| Service           | HTTP Port |
|-------------------|-----------|
| ApiGateway        | 5270      |
| EventService      | 5139      |
| IdentityService   | 5049      |
| BookingService    | 5077      |

API docs (Scalar UI) available at `http://localhost:<port>/scalar` in Development.

## Architecture

### Clean Architecture Per Service

Each microservice is split into four projects:

```
ServiceName_Api/           # Controllers, validators, GlobalExceptionHandler, Program.cs
ServiceName_Application/   # Service interfaces + implementations, DTOs, Mapster mappings
ServiceName_Domain/        # Entities, enums, repository interfaces
ServiceName_Infrastructure/ # DbContext, repository implementations, EF migrations
```

Dependencies flow inward: `Api → Application → Domain ← Infrastructure`.

### Microservices

- **EventService** — Core service. 20+ domain entities: `Event`, `Track`, `Session`, `TicketType`, `Talent`, `Lineup`, `Sponsor`, `Poll`, `QaQuestion`, `Feedback`, `CheckIn`, etc. Event is the aggregate root.
- **IdentityService** — JWT token issuance, user/organization management, subscription plans.
- **BookingService** — Event booking and reservation management.
- **NotificationService** — User notifications.
- **AIService** — AI-powered features.
- **ApiGateway** — Minimal entry point; routes traffic to services.

### Shared Libraries

- `Common/` — `ApiResponse<T>` (standardized response wrapper with `IsSuccess`, `StatusCode`, `Message`, `Data`, `Errors`).
- `BaseClass/` — Abstract `BaseClass` entity with `Id` (Guid), `CreatedAt`, `UpdatedAt` audit fields. All domain entities inherit from it.

## Key Patterns & Conventions

### API Response Wrapper
All endpoints return `ApiResponse<T>`:
```csharp
return ApiResponse<EventResponseDto>.Success(201, "Created", dto);
return ApiResponse<EventResponseDto>.Fail(404, "Not found");
```

### Repository Pattern
- Repository interface in `Domain/Interfaces/` (e.g., `IEventRepository`)
- Implementation in `Infrastructure/Repositories/` (e.g., `EventRepository`)
- Registered as `AddScoped<IRepo, RepoImpl>()` in `Program.cs`

### Object Mapping
Uses **Mapster** (not AutoMapper):
```csharp
var dto = entity.Adapt<EventResponseDto>();
TypeAdapterConfig<Event, EventResponseDto>.NewConfig().MaxDepth(3); // for nested objects
```

### DTOs
- **Request DTOs** use `record` with nullable optional fields for partial updates.
- **Response DTOs** use `record` with required fields.

### Database
- **PostgreSQL** via **EF Core 10** Code-First.
- JSONB column used for `RichTextContent` (event description).
- Default connection string in `appsettings.json`: `Host=localhost;Database=<service>_service;Username=postgres;Password=123456`

### JWT Authentication
- Issuer/Audience: `"hostlistic"` (both).
- Claims include: `sub` (user GUID), `email`, `name`, `Role`.
- `[Authorize]` attribute on controllers — currently disabled on some controllers during development.

### Validation
- Model state errors returned as `ApiResponse` via `InvalidModelStateResponseFactory` in `Program.cs`.
- Business rule validation done inline in Application services before repository calls.

### CORS
Configured for `http://localhost:3000` with credentials allowed (NextApp policy).