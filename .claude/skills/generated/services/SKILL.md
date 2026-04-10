---
name: services
description: "Skill for the Services area of Hostlistic_Server. 1334 symbols across 335 files."
---

# Services

1334 symbols | 335 files | Cohesion: 78%

## When to Use

- Working with code in `Services/`
- Understanding how QaVote, EventType, EventTeamMember work
- Modifying services-related functionality

## Key Files

| File | Symbols |
|------|---------|
| `Services/EventService/EventService_Test/Services/TeamMemberServiceTest.cs` | InviteAsync_WhenEventNotFound_ReturnsFail404, InviteAsync_WhenNonOrganizerWithoutPermission_ReturnsFail403, InviteAsync_WhenNonOrganizerInvitesCoOrganizer_ReturnsFail403, InviteAsync_WhenInvitingSelf_ReturnsFail400, InviteAsync_WhenUserAlreadyInvited_ReturnsFail409 (+13) |
| `Services/EventService/EventService_Test/Services/EventServiceTest.cs` | GetEventByIdAsync_WhenEventExists_ReturnsSuccess200, GetEventByIdAsync_WhenEventNotFound_ReturnsFail404, UpdateEventAsync_WhenEventExists_ReturnsSuccess200, UpdateEventAsync_WhenEventNotFound_ReturnsFail404, UpdateEventAsync_WhenCapacityExceedsPlanLimit_ReturnsFail403 (+11) |
| `Services/EventService/EventService_Test/Services/EventDayServiceTest.cs` | GenerateDaysAsync_WhenEventNotFound_ReturnsFail404, GenerateDaysAsync_WhenEventHasNoDates_ReturnsFail400, GenerateDaysAsync_WhenDaysAlreadyExist_ReturnsFail409, GenerateDaysAsync_WithInvalidTimezone_ReturnsFail400, GenerateDaysAsync_WithValidEvent_GeneratesCorrectNumberOfDays (+10) |
| `Services/AIService/AIService_Application/Services/PromptTemplateEngine.cs` | BuildParametersFromEvent, ParseEmailResponse, AddToneAndLanguage, SanitizeHtml, MyRegex (+10) |
| `Services/EventService/EventService_Test/Services/TrackServiceTest.cs` | CreateTrackAsync_WhenEventNotFound_ReturnsFail404, CreateTrackAsync_WithEmptyName_ReturnsFail400, CreateTrackAsync_WithInvalidColorHex_ReturnsFail400, CreateTrackAsync_WithStartTimeAfterEndTime_ReturnsFail400, CreateTrackAsync_WithValidRequest_ReturnsSuccess201 (+9) |
| `Services/EventService/EventService_Test/Services/TicketTypeServiceTest.cs` | GetTicketTypeByIdAsync_WhenNotFound_ReturnsFail404, GetTicketTypeByIdAsync_WhenExists_ReturnsSuccess200, UpdateTicketTypeAsync_WhenNotFound_ReturnsFail404, UpdateTicketTypeAsync_WithSaleStartAfterEnd_ReturnsFail400, UpdateTicketTypeAsync_WithValidRequest_ReturnsSuccess200 (+9) |
| `Services/BookingService/BookingService_Test/Services/PaymentMethodServiceTest.cs` | GetActivePaymentMethodsAsync_ReturnsSuccess200WithCollection, GetPaymentOptionsAsync_WithZeroTotalAmount_ReturnsFreeMethodOnly, GetPaymentOptionsAsync_WithPositiveTotalAmount_ReturnsActivePaymentMethods, DeletePaymentMethodAsync_WhenNotFound_ReturnsFail404, GetPaymentMethodByIdAsync_WhenNotFound_ReturnsFail404 (+9) |
| `Services/EventService/EventService_Test/Services/FeedbackServiceTest.cs` | AddFeedbackAsync_WhenEventNotFound_ReturnsFail404, AddFeedbackAsync_WhenSessionNotFound_ReturnsFail404, AddFeedbackAsync_WithInvalidRating_ReturnsFail400, AddFeedbackAsync_WithEmptyUserId_ReturnsFail400, AddFeedbackAsync_WithEmptyComment_ReturnsFail400 (+8) |
| `Services/EventService/EventService_Test/Services/SessionServiceTest.cs` | GetSessionByIdAsync_WhenSessionNotFound_ReturnsFail404, GetSessionByIdAsync_WhenSessionExists_ReturnsSuccess200, UpdateSessionAsync_WhenSessionIsCancelled_ReturnsFail400, DeleteSessionAsync_WhenSessionNotFound_ReturnsFail404, UpdateSessionStatusAsync_ValidTransitions_ReturnsSuccess200 (+8) |
| `Services/BookingService/BookingService_Test/Services/WalletServiceTest.cs` | GetWalletByIdAsync_WhenFound_ReturnsSuccess200, UpdateWalletBalanceAsync_WhenInsufficientFundsForWithdraw_ReturnsFail400, UpdateWalletBalanceAsync_WhenWalletNotFound_ReturnsFail404, UpdateWalletBalanceAsync_WithValidWithdraw_ReturnsSuccess200, UpdateWalletBalanceAsync_WithInvalidTransactionType_ReturnsFail400 (+8) |

## Entry Points

Start here when exploring this area:

- **`QaVote`** (Class) — `Services/EventService/EventService_Domain/Entities/QaVote.cs:4`
- **`EventType`** (Class) — `Services/EventService/EventService_Domain/Entities/EventType.cs:4`
- **`EventTeamMember`** (Class) — `Services/EventService/EventService_Domain/Entities/EventTeamMember.cs:7`
- **`CreateTrackRequest`** (Class) — `Services/EventService/EventService_Application/DTOs/TrackDto.cs:16`
- **`StreamAuthResponseDto`** (Class) — `Services/EventService/EventService_Application/DTOs/StreamAuthResponseDto.cs:6`

## Key Symbols

| Symbol | Type | File | Line |
|--------|------|------|------|
| `QaVote` | Class | `Services/EventService/EventService_Domain/Entities/QaVote.cs` | 4 |
| `EventType` | Class | `Services/EventService/EventService_Domain/Entities/EventType.cs` | 4 |
| `EventTeamMember` | Class | `Services/EventService/EventService_Domain/Entities/EventTeamMember.cs` | 7 |
| `CreateTrackRequest` | Class | `Services/EventService/EventService_Application/DTOs/TrackDto.cs` | 16 |
| `StreamAuthResponseDto` | Class | `Services/EventService/EventService_Application/DTOs/StreamAuthResponseDto.cs` | 6 |
| `CreateLineupsRequest` | Class | `Services/EventService/EventService_Application/DTOs/LineupDto.cs` | 9 |
| `FeedbackDto` | Class | `Services/EventService/EventService_Application/DTOs/FeedbackDto.cs` | 2 |
| `UserPlanLookupResult` | Class | `Services/EventService/EventService_Application/DTOs/UserPlanLookupResult.cs` | 2 |
| `TicketType` | Class | `Services/EventService/EventService_Domain/Entities/TicketType.cs` | 5 |
| `UpdateTicketTypeRequest` | Class | `Services/EventService/EventService_Application/DTOs/TicketTypeDto.cs` | 41 |
| `EventInfoDto` | Class | `Services/BookingService/BookingService_Application/Services/TicketPurchaseService.cs` | 702 |
| `UserInfoDto` | Class | `Services/BookingService/BookingService_Application/Services/TicketPurchaseService.cs` | 714 |
| `TicketDto` | Class | `Services/BookingService/BookingService_Application/DTOs/TicketDto.cs` | 2 |
| `PurchaseConfirmationRequest` | Class | `Services/BookingService/BookingService_Application/DTOs/PurchaseTicketRequest.cs` | 49 |
| `PaymentConfirmedPayload` | Class | `Services/BookingService/BookingService_Application/DTOs/PaymentNotificationDto.cs` | 3 |
| `TicketSummaryDto` | Class | `Services/BookingService/BookingService_Application/DTOs/PaymentNotificationDto.cs` | 12 |
| `PaymentDto` | Class | `Services/BookingService/BookingService_Application/DTOs/PaymentDto.cs` | 4 |
| `PayOsVerifiedPaymentData` | Class | `Services/BookingService/BookingService_Application/DTOs/PayOsWebhookPayload.cs` | 34 |
| `OrderDto` | Class | `Services/BookingService/BookingService_Application/DTOs/OrderDto.cs` | 4 |
| `OrderDetailDto` | Class | `Services/BookingService/BookingService_Application/DTOs/OrderDetailDto.cs` | 2 |

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
| `HandleWebhook → GetOrderByOrderCodeAsync` | cross_community | 4 |
| `HandleWebhook → GetOrderByOrderCodeAsync` | cross_community | 4 |

## Connected Areas

| Area | Connections |
|------|-------------|
| Interfaces | 337 calls |
| Repositories | 23 calls |
| Controllers | 19 calls |
| Interface | 11 calls |
| Entities | 4 calls |
| ServiceClients | 4 calls |
| Consumers | 1 calls |

## How to Explore

1. `gitnexus_context({name: "QaVote"})` — see callers and callees
2. `gitnexus_query({query: "services"})` — find related execution flows
3. Read key files listed above for implementation details
