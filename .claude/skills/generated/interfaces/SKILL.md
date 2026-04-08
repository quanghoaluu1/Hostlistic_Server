---
name: interfaces
description: "Skill for the Interfaces area of Hostlistic_Server. 619 symbols across 240 files."
---

# Interfaces

619 symbols | 240 files | Cohesion: 73%

## When to Use

- Working with code in `Services/`
- Understanding how InventoryReservation, CheckIn, TicketTypeInfoDto work
- Modifying interfaces-related functionality

## Key Files

| File | Symbols |
|------|---------|
| `Services/NotificationService/NotificationService_Api/Controllers/UserNotificationController.cs` | GetById, Create, Update, Delete, GetUnread (+5) |
| `Services/NotificationService/NotificationService_Infrastructure/Repositories/UserNotificationRepository.cs` | GetByIdAsync, UpdateAsync, DeleteAsync, SaveChangesAsync, AddAsync (+4) |
| `Services/NotificationService/NotificationService_Application/Services/UserNotificationService.cs` | GetByIdAsync, MarkAsReadAsync, CreateAsync, UpdateAsync, DeleteAsync (+4) |
| `Services/NotificationService/NotificationService_Application/Interfaces/IUserNotificationService.cs` | GetByIdAsync, CreateAsync, UpdateAsync, DeleteAsync, GetUnreadByUserIdAsync (+4) |
| `Services/NotificationService/NotificationService_Domain/Interfaces/IUserNotificationRepository.cs` | GetByIdAsync, UpdateAsync, DeleteAsync, SaveChangesAsync, AddAsync (+4) |
| `Services/BookingService/BookingService_Application/Services/TicketPurchaseService.cs` | CheckTicketAvailabilityAsync, PurchaseTicketsAsync, InitiatePayOsPurchaseAsync, PurchaseFreeTicketsAsync, GenerateTicketsWithQrCodesAsync (+3) |
| `Services/NotificationService/NotificationService_Domain/Interfaces/IEmailLogRepository.cs` | AddRangeAsync, GetByIdAsync, UpdateAsync, DeleteAsync, SaveChangesAsync (+2) |
| `Services/NotificationService/NotificationService_Api/Controllers/EmailCampaignController.cs` | GetById, Create, Update, Delete, Preview (+2) |
| `Services/EventService/EventService_Infrastructure/Repositories/SessionRepository.cs` | HasOverlapInTrackAsync, HasOverlapInVenueAsync, UpdateSessionAsync, GetMaxSortOrderInTrackAsync, AddSessionAsync (+2) |
| `Services/EventService/EventService_Domain/Interfaces/ISessionRepository.cs` | HasOverlapInTrackAsync, HasOverlapInVenueAsync, UpdateSessionAsync, GetMaxSortOrderInTrackAsync, AddSessionAsync (+2) |

## Entry Points

Start here when exploring this area:

- **`InventoryReservation`** (Class) — `Services/BookingService/BookingService_Domain/Entities/InventoryReservation.cs:6`
- **`CheckIn`** (Class) — `Services/BookingService/BookingService_Domain/Entities/CheckIn.cs:4`
- **`TicketTypeInfoDto`** (Class) — `Services/BookingService/BookingService_Application/Services/TicketPurchaseService.cs:721`
- **`PurchaseTicketRequest`** (Class) — `Services/BookingService/BookingService_Application/DTOs/PurchaseTicketRequest.cs:6`
- **`TicketItemRequest`** (Class) — `Services/BookingService/BookingService_Application/DTOs/PurchaseTicketRequest.cs:23`

## Key Symbols

| Symbol | Type | File | Line |
|--------|------|------|------|
| `InventoryReservation` | Class | `Services/BookingService/BookingService_Domain/Entities/InventoryReservation.cs` | 6 |
| `CheckIn` | Class | `Services/BookingService/BookingService_Domain/Entities/CheckIn.cs` | 4 |
| `TicketTypeInfoDto` | Class | `Services/BookingService/BookingService_Application/Services/TicketPurchaseService.cs` | 721 |
| `PurchaseTicketRequest` | Class | `Services/BookingService/BookingService_Application/DTOs/PurchaseTicketRequest.cs` | 6 |
| `TicketItemRequest` | Class | `Services/BookingService/BookingService_Application/DTOs/PurchaseTicketRequest.cs` | 23 |
| `PurchaseTicketResponse` | Class | `Services/BookingService/BookingService_Application/DTOs/PurchaseTicketRequest.cs` | 32 |
| `InventoryCheckRequest` | Class | `Services/BookingService/BookingService_Application/DTOs/PurchaseTicketRequest.cs` | 62 |
| `InventoryCheckResponse` | Class | `Services/BookingService/BookingService_Application/DTOs/PurchaseTicketRequest.cs` | 67 |
| `TicketAvailabilityInfo` | Class | `Services/BookingService/BookingService_Application/DTOs/PurchaseTicketRequest.cs` | 74 |
| `CreatePaymentRequest` | Class | `Services/BookingService/BookingService_Application/DTOs/PaymentDto.cs` | 17 |
| `CreatePayOsPaymentRequest` | Class | `Services/BookingService/BookingService_Application/DTOs/PayOsDto.cs` | 3 |
| `PayOsItemDto` | Class | `Services/BookingService/BookingService_Application/DTOs/PayOsDto.cs` | 12 |
| `PayOsCheckoutResult` | Class | `Services/BookingService/BookingService_Application/DTOs/PayOsDto.cs` | 20 |
| `PayOsCheckoutResponse` | Class | `Services/BookingService/BookingService_Application/DTOs/PayOsDto.cs` | 35 |
| `CreateOrderRequest` | Class | `Services/BookingService/BookingService_Application/DTOs/OrderDto.cs` | 17 |
| `UpdateOrderRequest` | Class | `Services/BookingService/BookingService_Application/DTOs/OrderDto.cs` | 28 |
| `CreateOrderDetailRequest` | Class | `Services/BookingService/BookingService_Application/DTOs/OrderDetailDto.cs` | 13 |
| `FreeTicketPurchaseRequest` | Class | `Services/BookingService/BookingService_Application/DTOs/FreeTicketPurchaseDto.cs` | 2 |
| `FreeTicketPurchaseResponse` | Class | `Services/BookingService/BookingService_Application/DTOs/FreeTicketPurchaseDto.cs` | 10 |
| `CampaignSendResponse` | Class | `Services/NotificationService/NotificationService_Application/Dtos/CampaignSendDtos.cs` | 2 |

## Execution Flows

| Flow | Type | Steps |
|------|------|-------|
| `GetMyTickets → Success` | cross_community | 5 |
| `GetMyTickets → Fail` | cross_community | 5 |
| `GetMyTicketsWithDetails → Success` | cross_community | 5 |
| `GetMyTicketsWithDetails → Fail` | cross_community | 5 |
| `HandleWebhook → GetOrderByOrderCodeAsync` | cross_community | 4 |
| `HandleWebhook → GetOrderByOrderCodeAsync` | cross_community | 4 |
| `HandleWebhook → Fail` | cross_community | 4 |
| `HandleWebhook → Success` | cross_community | 4 |
| `EndStreamRoom → GetByIdAsync` | cross_community | 4 |
| `EndStreamRoom → GetByIdAsync` | cross_community | 4 |

## Connected Areas

| Area | Connections |
|------|-------------|
| Services | 102 calls |
| Repositories | 16 calls |
| Controllers | 14 calls |
| ServiceClients | 2 calls |

## How to Explore

1. `gitnexus_context({name: "InventoryReservation"})` — see callers and callees
2. `gitnexus_query({query: "interfaces"})` — find related execution flows
3. Read key files listed above for implementation details
