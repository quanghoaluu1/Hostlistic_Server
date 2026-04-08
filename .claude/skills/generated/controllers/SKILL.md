---
name: controllers
description: "Skill for the Controllers area of Hostlistic_Server. 74 symbols across 36 files."
---

# Controllers

74 symbols | 36 files | Cohesion: 76%

## When to Use

- Working with code in `Services/`
- Understanding how UpdateUserProfileRequest, WeatherForecast, UploadPhotoAsync work
- Modifying controllers-related functionality

## Key Files

| File | Symbols |
|------|---------|
| `Services/StreamingService/StreamingService_Api/Controllers/LiveKitWebhooksController.cs` | ReceiveWebhook, ValidateWebhookSignature, HandleRoomStartedAsync, HandleRoomFinishedAsync, HandleParticipantJoinedAsync (+3) |
| `Services/EventService/EventService_Api/Controllers/TeamMemberController.cs` | Invite, GetByEventId, RespondByUser, RemoveMember, UpdatePermissions (+2) |
| `Services/EventService/EventService_Application/Interfaces/ITeamMemberService.cs` | InviteAsync, GetByEventIdAsync, RespondByUserAsync, RemoveMemberAsync, UpdatePermissionsAsync |
| `Services/EventService/EventService_Api/Controllers/EventSessionBookingsController.cs` | BookSession, CancelBooking, GetSessionsWithBookingStatus, GetBookingStatus, GetCurrentUserId |
| `Services/StreamingService/StreamingService_Api/Controllers/StreamsController.cs` | CreateStreamRoom, GetStreamToken, EndStreamRoom, TryGetCurrentUserId |
| `Services/EventService/EventService_Application/Interfaces/ISessionBookingService.cs` | BookSessionAsync, CancelBookingAsync, GetSessionsWithBookingStatusAsync, GetBookingStatusForSessionAsync |
| `Services/IdentityService/IdentityService_Api/Controllers/AuthController.cs` | Login, RefreshToken, GoogleLogin, SetRefreshTokenCookie |
| `Services/IdentityService/IdentityService_Api/Controllers/UserController.cs` | UpdateAvatar, GetUserProfile, GetUserById |
| `Services/StreamingService/StreamingService_Api/Hubs/StreamingHub.cs` | JoinEventGroup, LeaveEventGroup, BuildEventGroup |
| `Services/IdentityService/IdentityService_Application/Interfaces/IAuthService.cs` | LoginAsync, RefreshTokenAsync, GoogleLoginAsync |

## Entry Points

Start here when exploring this area:

- **`UpdateUserProfileRequest`** (Class) — `Services/IdentityService/IdentityService_Application/DTOs/UserProfileDto.cs:4`
- **`WeatherForecast`** (Class) — `Services/AIService/AIService_Api/WeatherForecast.cs:2`
- **`UploadPhotoAsync`** (Method) — `Services/IdentityService/IdentityService_Application/Services/PhotoService.cs:18`
- **`UpdateAvatar`** (Method) — `Services/IdentityService/IdentityService_Api/Controllers/UserController.cs:53`
- **`UploadPhotoAsync`** (Method) — `Services/EventService/EventService_Application/Services/PhotoService.cs:17`

## Key Symbols

| Symbol | Type | File | Line |
|--------|------|------|------|
| `UpdateUserProfileRequest` | Class | `Services/IdentityService/IdentityService_Application/DTOs/UserProfileDto.cs` | 4 |
| `WeatherForecast` | Class | `Services/AIService/AIService_Api/WeatherForecast.cs` | 2 |
| `UploadPhotoAsync` | Method | `Services/IdentityService/IdentityService_Application/Services/PhotoService.cs` | 18 |
| `UpdateAvatar` | Method | `Services/IdentityService/IdentityService_Api/Controllers/UserController.cs` | 53 |
| `UploadPhotoAsync` | Method | `Services/EventService/EventService_Application/Services/PhotoService.cs` | 17 |
| `UploadPhoto` | Method | `Services/EventService/EventService_Api/Controllers/MediaController.cs` | 10 |
| `SetEventCover` | Method | `Services/EventService/EventService_Api/Controllers/EventController.cs` | 42 |
| `UploadPhotoAsync` | Method | `Services/BookingService/BookingService_Application/Services/PhotoService.cs` | 20 |
| `UpdatePayoutRequestWithProofAsync` | Method | `Services/BookingService/BookingService_Application/Services/PayoutRequestService.cs` | 71 |
| `SetPayoutRequestProof` | Method | `Services/BookingService/BookingService_Api/Controllers/PayoutRequestsController.cs` | 56 |
| `UpdatePayoutRequest` | Method | `Services/BookingService/BookingService_Api/Controllers/PayoutRequestsController.cs` | 78 |
| `SetPaymentMethodIcon` | Method | `Services/BookingService/BookingService_Api/Controllers/PaymentMethodsController.cs` | 64 |
| `Invite` | Method | `Services/EventService/EventService_Api/Controllers/TeamMemberController.cs` | 12 |
| `GetByEventId` | Method | `Services/EventService/EventService_Api/Controllers/TeamMemberController.cs` | 20 |
| `RespondByUser` | Method | `Services/EventService/EventService_Api/Controllers/TeamMemberController.cs` | 44 |
| `RemoveMember` | Method | `Services/EventService/EventService_Api/Controllers/TeamMemberController.cs` | 52 |
| `UpdatePermissions` | Method | `Services/EventService/EventService_Api/Controllers/TeamMemberController.cs` | 60 |
| `JoinEventGroup` | Method | `Services/StreamingService/StreamingService_Api/Hubs/StreamingHub.cs` | 8 |
| `LeaveEventGroup` | Method | `Services/StreamingService/StreamingService_Api/Hubs/StreamingHub.cs` | 13 |
| `BuildEventGroup` | Method | `Services/StreamingService/StreamingService_Api/Hubs/StreamingHub.cs` | 18 |

## Execution Flows

| Flow | Type | Steps |
|------|------|-------|
| `EndStreamRoom → GetByIdAsync` | cross_community | 4 |
| `EndStreamRoom → GetByIdAsync` | cross_community | 4 |
| `EndStreamRoom → Fail` | cross_community | 4 |
| `EndStreamRoom → ResolveAsync` | cross_community | 4 |
| `GetStreamToken → GetByIdAsync` | cross_community | 4 |
| `GetStreamToken → GetByIdAsync` | cross_community | 4 |
| `GetStreamToken → Fail` | cross_community | 4 |
| `GetStreamToken → ResolveAsync` | cross_community | 4 |
| `CreateStreamRoom → GetByIdAsync` | cross_community | 4 |
| `CreateStreamRoom → GetByIdAsync` | cross_community | 4 |

## Connected Areas

| Area | Connections |
|------|-------------|
| Services | 29 calls |
| Interfaces | 19 calls |
| Repositories | 1 calls |

## How to Explore

1. `gitnexus_context({name: "UpdateUserProfileRequest"})` — see callers and callees
2. `gitnexus_query({query: "controllers"})` — find related execution flows
3. Read key files listed above for implementation details
