---
name: serviceclients
description: "Skill for the ServiceClients area of Hostlistic_Server. 22 symbols across 16 files."
---

# ServiceClients

22 symbols | 16 files | Cohesion: 80%

## When to Use

- Working with code in `Services/`
- Understanding how UserPlanServiceClient, UserServiceClient, UserPlanServiceClient work
- Modifying serviceclients-related functionality

## Key Files

| File | Symbols |
|------|---------|
| `Services/EventService/EventService_Infrastructure/ServiceClients/UserPlanServiceClient.cs` | GetByUserIdAsync, ForwardAuthorizationHeader, UserPlanServiceClient |
| `Services/AIService/AIService_Infrastructure/ServiceClients/UserPlanServiceClient.cs` | GetByUserIdAsync, ForwardAuthorizationHeader, UserPlanServiceClient |
| `Services/AIService/AIService_Application/Interface/IUserPlanServiceClient.cs` | GetByUserIdAsync, IUserPlanServiceClient |
| `Services/EventService/EventService_Application/Interfaces/IUserPlanServiceClient.cs` | GetByUserIdAsync, IUserPlanServiceClient |
| `Services/AIService/AIService_Application/Services/AiPlanEntitlementService.cs` | EnsureCanUseAiAsync |
| `Services/AIService/AIService_Application/Interface/IAiPlanEntitlementService.cs` | EnsureCanUseAiAsync |
| `Services/AIService/AIService_Api/Filters/RequireAiSubscriptionFilter.cs` | OnActionExecutionAsync |
| `Services/EventService/EventService_Application/Services/EventService.cs` | GetActiveEntitlementAsync |
| `Services/BookingService/BookingService_Infrastructure/ServiceClients/UserServiceClient.cs` | UserServiceClient |
| `Services/BookingService/BookingService_Application/Interfaces/IUserServiceClient.cs` | IUserServiceClient |

## Entry Points

Start here when exploring this area:

- **`UserPlanServiceClient`** (Class) — `Services/EventService/EventService_Infrastructure/ServiceClients/UserPlanServiceClient.cs:9`
- **`UserServiceClient`** (Class) — `Services/BookingService/BookingService_Infrastructure/ServiceClients/UserServiceClient.cs:9`
- **`UserPlanServiceClient`** (Class) — `Services/BookingService/BookingService_Infrastructure/ServiceClients/UserPlanServiceClient.cs:9`
- **`NotificationServiceClient`** (Class) — `Services/BookingService/BookingService_Infrastructure/ServiceClients/NotificationServiceClient.cs:7`
- **`EventServiceClient`** (Class) — `Services/BookingService/BookingService_Infrastructure/ServiceClients/EventServiceClient.cs:9`

## Key Symbols

| Symbol | Type | File | Line |
|--------|------|------|------|
| `UserPlanServiceClient` | Class | `Services/EventService/EventService_Infrastructure/ServiceClients/UserPlanServiceClient.cs` | 9 |
| `UserServiceClient` | Class | `Services/BookingService/BookingService_Infrastructure/ServiceClients/UserServiceClient.cs` | 9 |
| `UserPlanServiceClient` | Class | `Services/BookingService/BookingService_Infrastructure/ServiceClients/UserPlanServiceClient.cs` | 9 |
| `NotificationServiceClient` | Class | `Services/BookingService/BookingService_Infrastructure/ServiceClients/NotificationServiceClient.cs` | 7 |
| `EventServiceClient` | Class | `Services/BookingService/BookingService_Infrastructure/ServiceClients/EventServiceClient.cs` | 9 |
| `UserPlanServiceClient` | Class | `Services/AIService/AIService_Infrastructure/ServiceClients/UserPlanServiceClient.cs` | 9 |
| `IUserPlanServiceClient` | Interface | `Services/EventService/EventService_Application/Interfaces/IUserPlanServiceClient.cs` | 4 |
| `IUserServiceClient` | Interface | `Services/BookingService/BookingService_Application/Interfaces/IUserServiceClient.cs` | 5 |
| `IUserPlanServiceClient` | Interface | `Services/BookingService/BookingService_Application/Interfaces/IUserPlanServiceClient.cs` | 4 |
| `INotificationServiceClient` | Interface | `Services/BookingService/BookingService_Application/Interfaces/INotificationServiceClient.cs` | 4 |
| `IEventServiceClient` | Interface | `Services/BookingService/BookingService_Application/Interfaces/IEventServiceClient.cs` | 5 |
| `IUserPlanServiceClient` | Interface | `Services/AIService/AIService_Application/Interface/IUserPlanServiceClient.cs` | 4 |
| `GetByUserIdAsync` | Method | `Services/EventService/EventService_Infrastructure/ServiceClients/UserPlanServiceClient.cs` | 14 |
| `EnsureCanUseAiAsync` | Method | `Services/AIService/AIService_Application/Services/AiPlanEntitlementService.cs` | 7 |
| `OnActionExecutionAsync` | Method | `Services/AIService/AIService_Api/Filters/RequireAiSubscriptionFilter.cs` | 9 |
| `GetByUserIdAsync` | Method | `Services/AIService/AIService_Infrastructure/ServiceClients/UserPlanServiceClient.cs` | 14 |
| `ForwardAuthorizationHeader` | Method | `Services/EventService/EventService_Infrastructure/ServiceClients/UserPlanServiceClient.cs` | 54 |
| `GetByUserIdAsync` | Method | `Services/AIService/AIService_Application/Interface/IUserPlanServiceClient.cs` | 6 |
| `EnsureCanUseAiAsync` | Method | `Services/AIService/AIService_Application/Interface/IAiPlanEntitlementService.cs` | 6 |
| `GetActiveEntitlementAsync` | Method | `Services/EventService/EventService_Application/Services/EventService.cs` | 332 |

## Execution Flows

| Flow | Type | Steps |
|------|------|-------|
| `CreateEventAsync → ForwardAuthorizationHeader` | cross_community | 5 |
| `CreateEventAsync → UserPlanLookupResult` | cross_community | 5 |
| `CreateEventAsync → ForwardAuthorizationHeader` | cross_community | 5 |
| `CreateEventAsync → ForwardAuthorizationHeader` | cross_community | 5 |
| `UpdateEvent → ForwardAuthorizationHeader` | cross_community | 5 |
| `UpdateEvent → UserPlanLookupResult` | cross_community | 5 |
| `UpdateEvent → ForwardAuthorizationHeader` | cross_community | 5 |
| `UpdateEvent → ForwardAuthorizationHeader` | cross_community | 5 |
| `CreateEventAsync → GetByUserIdAsync` | cross_community | 4 |
| `UpdateEvent → GetByUserIdAsync` | cross_community | 4 |

## Connected Areas

| Area | Connections |
|------|-------------|
| Services | 3 calls |

## How to Explore

1. `gitnexus_context({name: "UserPlanServiceClient"})` — see callers and callees
2. `gitnexus_query({query: "serviceclients"})` — find related execution flows
3. Read key files listed above for implementation details
