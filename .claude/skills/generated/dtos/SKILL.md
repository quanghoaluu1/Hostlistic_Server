---
name: dtos
description: "Skill for the DTOs area of Hostlistic_Server. 15 symbols across 8 files."
---

# DTOs

15 symbols | 8 files | Cohesion: 93%

## When to Use

- Working with code in `Services/`
- Understanding how AttendeeDto, AttendeeTicketDto, AttendeeListResponse work
- Modifying dtos-related functionality

## Key Files

| File | Symbols |
|------|---------|
| `Services/BookingService/BookingService_Application/DTOs/AttendeeDto.cs` | AttendeeDto, AttendeeTicketDto, AttendeeListResponse, AttendeeSummaryDto, TicketTypeSummaryDto |
| `Services/BookingService/BookingService_Infrastructure/Services/AttendeeService.cs` | GetAttendeesAsync, GetAttendeeSummaryAsync |
| `Services/BookingService/BookingService_Application/Interfaces/IAttendeeService.cs` | GetAttendeesAsync, GetAttendeeSummaryAsync |
| `Services/BookingService/BookingService_Api/Controllers/AttendeesController.cs` | GetAttendees, GetSummary |
| `Common/DTOs/UserDto.cs` | UserDto |
| `Services/IdentityService/IdentityService_Application/DTOs/UserProfileDto.cs` | UserProfileDto |
| `Services/BookingService/BookingService_Application/DTOs/PaymentNotificationDto.cs` | PaymentFailedPayload |
| `Services/BookingService/BookingService_Api/Services/SignalRPaymentNotifier.cs` | NotifyPaymentFailedAsync |

## Entry Points

Start here when exploring this area:

- **`AttendeeDto`** (Class) — `Services/BookingService/BookingService_Application/DTOs/AttendeeDto.cs:15`
- **`AttendeeTicketDto`** (Class) — `Services/BookingService/BookingService_Application/DTOs/AttendeeDto.cs:30`
- **`AttendeeListResponse`** (Class) — `Services/BookingService/BookingService_Application/DTOs/AttendeeDto.cs:42`
- **`AttendeeSummaryDto`** (Class) — `Services/BookingService/BookingService_Application/DTOs/AttendeeDto.cs:52`
- **`TicketTypeSummaryDto`** (Class) — `Services/BookingService/BookingService_Application/DTOs/AttendeeDto.cs:61`

## Key Symbols

| Symbol | Type | File | Line |
|--------|------|------|------|
| `AttendeeDto` | Class | `Services/BookingService/BookingService_Application/DTOs/AttendeeDto.cs` | 15 |
| `AttendeeTicketDto` | Class | `Services/BookingService/BookingService_Application/DTOs/AttendeeDto.cs` | 30 |
| `AttendeeListResponse` | Class | `Services/BookingService/BookingService_Application/DTOs/AttendeeDto.cs` | 42 |
| `AttendeeSummaryDto` | Class | `Services/BookingService/BookingService_Application/DTOs/AttendeeDto.cs` | 52 |
| `TicketTypeSummaryDto` | Class | `Services/BookingService/BookingService_Application/DTOs/AttendeeDto.cs` | 61 |
| `UserDto` | Class | `Common/DTOs/UserDto.cs` | 2 |
| `UserProfileDto` | Class | `Services/IdentityService/IdentityService_Application/DTOs/UserProfileDto.cs` | 11 |
| `PaymentFailedPayload` | Class | `Services/BookingService/BookingService_Application/DTOs/PaymentNotificationDto.cs` | 21 |
| `GetAttendeesAsync` | Method | `Services/BookingService/BookingService_Infrastructure/Services/AttendeeService.cs` | 11 |
| `GetAttendees` | Method | `Services/BookingService/BookingService_Api/Controllers/AttendeesController.cs` | 12 |
| `GetAttendeeSummaryAsync` | Method | `Services/BookingService/BookingService_Infrastructure/Services/AttendeeService.cs` | 92 |
| `GetSummary` | Method | `Services/BookingService/BookingService_Api/Controllers/AttendeesController.cs` | 22 |
| `NotifyPaymentFailedAsync` | Method | `Services/BookingService/BookingService_Api/Services/SignalRPaymentNotifier.cs` | 25 |
| `GetAttendeesAsync` | Method | `Services/BookingService/BookingService_Application/Interfaces/IAttendeeService.cs` | 7 |
| `GetAttendeeSummaryAsync` | Method | `Services/BookingService/BookingService_Application/Interfaces/IAttendeeService.cs` | 8 |

## Execution Flows

| Flow | Type | Steps |
|------|------|-------|
| `GetAttendees → AttendeeDto` | intra_community | 3 |
| `GetAttendees → AttendeeTicketDto` | intra_community | 3 |
| `GetAttendees → AttendeeListResponse` | intra_community | 3 |
| `GetAttendees → Success` | cross_community | 3 |

## Connected Areas

| Area | Connections |
|------|-------------|
| Interfaces | 2 calls |

## How to Explore

1. `gitnexus_context({name: "AttendeeDto"})` — see callers and callees
2. `gitnexus_query({query: "dtos"})` — find related execution flows
3. Read key files listed above for implementation details
