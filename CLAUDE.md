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

<!-- gitnexus:start -->
# GitNexus — Code Intelligence

This project is indexed by GitNexus as **Hostlistic_Server** (6921 symbols, 30573 relationships, 300 execution flows). Use the GitNexus MCP tools to understand code, assess impact, and navigate safely.

> If any GitNexus tool warns the index is stale, run `npx gitnexus analyze` in terminal first.

## Always Do

- **MUST run impact analysis before editing any symbol.** Before modifying a function, class, or method, run `gitnexus_impact({target: "symbolName", direction: "upstream"})` and report the blast radius (direct callers, affected processes, risk level) to the user.
- **MUST run `gitnexus_detect_changes()` before committing** to verify your changes only affect expected symbols and execution flows.
- **MUST warn the user** if impact analysis returns HIGH or CRITICAL risk before proceeding with edits.
- When exploring unfamiliar code, use `gitnexus_query({query: "concept"})` to find execution flows instead of grepping. It returns process-grouped results ranked by relevance.
- When you need full context on a specific symbol — callers, callees, which execution flows it participates in — use `gitnexus_context({name: "symbolName"})`.

## When Debugging

1. `gitnexus_query({query: "<error or symptom>"})` — find execution flows related to the issue
2. `gitnexus_context({name: "<suspect function>"})` — see all callers, callees, and process participation
3. `READ gitnexus://repo/Hostlistic_Server/process/{processName}` — trace the full execution flow step by step
4. For regressions: `gitnexus_detect_changes({scope: "compare", base_ref: "main"})` — see what your branch changed

## When Refactoring

- **Renaming**: MUST use `gitnexus_rename({symbol_name: "old", new_name: "new", dry_run: true})` first. Review the preview — graph edits are safe, text_search edits need manual review. Then run with `dry_run: false`.
- **Extracting/Splitting**: MUST run `gitnexus_context({name: "target"})` to see all incoming/outgoing refs, then `gitnexus_impact({target: "target", direction: "upstream"})` to find all external callers before moving code.
- After any refactor: run `gitnexus_detect_changes({scope: "all"})` to verify only expected files changed.

## Never Do

- NEVER edit a function, class, or method without first running `gitnexus_impact` on it.
- NEVER ignore HIGH or CRITICAL risk warnings from impact analysis.
- NEVER rename symbols with find-and-replace — use `gitnexus_rename` which understands the call graph.
- NEVER commit changes without running `gitnexus_detect_changes()` to check affected scope.

## Tools Quick Reference

| Tool | When to use | Command |
|------|-------------|---------|
| `query` | Find code by concept | `gitnexus_query({query: "auth validation"})` |
| `context` | 360-degree view of one symbol | `gitnexus_context({name: "validateUser"})` |
| `impact` | Blast radius before editing | `gitnexus_impact({target: "X", direction: "upstream"})` |
| `detect_changes` | Pre-commit scope check | `gitnexus_detect_changes({scope: "staged"})` |
| `rename` | Safe multi-file rename | `gitnexus_rename({symbol_name: "old", new_name: "new", dry_run: true})` |
| `cypher` | Custom graph queries | `gitnexus_cypher({query: "MATCH ..."})` |

## Impact Risk Levels

| Depth | Meaning | Action |
|-------|---------|--------|
| d=1 | WILL BREAK — direct callers/importers | MUST update these |
| d=2 | LIKELY AFFECTED — indirect deps | Should test |
| d=3 | MAY NEED TESTING — transitive | Test if critical path |

## Resources

| Resource | Use for |
|----------|---------|
| `gitnexus://repo/Hostlistic_Server/context` | Codebase overview, check index freshness |
| `gitnexus://repo/Hostlistic_Server/clusters` | All functional areas |
| `gitnexus://repo/Hostlistic_Server/processes` | All execution flows |
| `gitnexus://repo/Hostlistic_Server/process/{name}` | Step-by-step execution trace |

## Self-Check Before Finishing

Before completing any code modification task, verify:
1. `gitnexus_impact` was run for all modified symbols
2. No HIGH/CRITICAL risk warnings were ignored
3. `gitnexus_detect_changes()` confirms changes match expected scope
4. All d=1 (WILL BREAK) dependents were updated

## Keeping the Index Fresh

After committing code changes, the GitNexus index becomes stale. Re-run analyze to update it:

```bash
npx gitnexus analyze
```

If the index previously included embeddings, preserve them by adding `--embeddings`:

```bash
npx gitnexus analyze --embeddings
```

To check whether embeddings exist, inspect `.gitnexus/meta.json` — the `stats.embeddings` field shows the count (0 means no embeddings). **Running analyze without `--embeddings` will delete any previously generated embeddings.**

> Claude Code users: A PostToolUse hook handles this automatically after `git commit` and `git merge`.

## CLI

| Task | Read this skill file |
|------|---------------------|
| Understand architecture / "How does X work?" | `.claude/skills/gitnexus/gitnexus-exploring/SKILL.md` |
| Blast radius / "What breaks if I change X?" | `.claude/skills/gitnexus/gitnexus-impact-analysis/SKILL.md` |
| Trace bugs / "Why is X failing?" | `.claude/skills/gitnexus/gitnexus-debugging/SKILL.md` |
| Rename / extract / split / refactor | `.claude/skills/gitnexus/gitnexus-refactoring/SKILL.md` |
| Tools, resources, schema reference | `.claude/skills/gitnexus/gitnexus-guide/SKILL.md` |
| Index, status, clean, wiki CLI commands | `.claude/skills/gitnexus/gitnexus-cli/SKILL.md` |
| Work in the Services area (1334 symbols) | `.claude/skills/generated/services/SKILL.md` |
| Work in the Interfaces area (619 symbols) | `.claude/skills/generated/interfaces/SKILL.md` |
| Work in the Repositories area (160 symbols) | `.claude/skills/generated/repositories/SKILL.md` |
| Work in the Controllers area (74 symbols) | `.claude/skills/generated/controllers/SKILL.md` |
| Work in the ServiceClients area (22 symbols) | `.claude/skills/generated/serviceclients/SKILL.md` |
| Work in the Interface area (16 symbols) | `.claude/skills/generated/interface/SKILL.md` |
| Work in the DTOs area (15 symbols) | `.claude/skills/generated/dtos/SKILL.md` |
| Work in the Consumers area (8 symbols) | `.claude/skills/generated/consumers/SKILL.md` |
| Work in the Entities area (7 symbols) | `.claude/skills/generated/entities/SKILL.md` |

<!-- gitnexus:end -->
