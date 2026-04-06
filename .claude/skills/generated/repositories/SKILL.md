---
name: repositories
description: "Skill for the Repositories area of Hostlistic_Server. 160 symbols across 113 files."
---

# Repositories

160 symbols | 113 files | Cohesion: 87%

## When to Use

- Working with code in `Services/`
- Understanding how PromptTemplate, EventRecipient, SessionSnapshot work
- Modifying repositories-related functionality

## Key Files

| File | Symbols |
|------|---------|
| `Services/AIService/AIService_Infrastructure/Repositories/PromptTemplateRepository.cs` | GetByKeyAsync, SaveChangesAsync, GetAllAsync, GetByCategoryAsync, PromptTemplateRepository |
| `Services/NotificationService/NotificationService_Infrastructure/Repositories/EventRecipientRepository.cs` | UpsertAsync, SaveChangesAsync, GetRecipientsAsync, BuildQuery, EventRecipientRepository |
| `Services/AIService/AIService_Domain/Interfaces/IPromptTemplateRepository.cs` | GetByKeyAsync, GetAllAsync, GetByCategoryAsync, IPromptTemplateRepository |
| `Services/AIService/AIService_Application/Services/PromptTemplateService.cs` | GetByKeyAsync, CreateAsync, GetAllAsync, GetByCategoryAsync |
| `Services/AIService/AIService_Application/Interface/IPromptTemplateService.cs` | GetByKeyAsync, CreateAsync, GetAllAsync, GetByCategoryAsync |
| `Services/NotificationService/NotificationService_Domain/Interfaces/IEventRecipientRepository.cs` | UpsertAsync, SaveChangesAsync, GetRecipientsAsync, IEventRecipientRepository |
| `Services/AIService/AIService_Api/Controllers/PromptTemplateController.cs` | GetByKey, Create, GetAll |
| `Services/EventService/EventService_Infrastructure/Repositories/SessionBookingRepository.cs` | UpdateSessionBookingAsync, GetBookedSessionIdsAsync, SessionBookingRepository |
| `Services/NotificationService/NotificationService_Infrastructure/Repositories/EmailLogRepository.cs` | AddAsync, SaveChangesAsync, EmailLogRepository |
| `Services/IdentityService/IdentityService_Infrastructure/Repositories/SubscriptionPlanRepository.cs` | DeleteAsync, SaveChangesAsync, SubscriptionPlanRepository |

## Entry Points

Start here when exploring this area:

- **`PromptTemplate`** (Class) — `Services/AIService/AIService_Domain/Entities/PromptTemplate.cs:4`
- **`EventRecipient`** (Class) — `Services/NotificationService/NotificationService_Domain/Entities/EventRecipient.cs:2`
- **`SessionSnapshot`** (Class) — `Services/BookingService/BookingService_Domain/Entities/SessionSnapshot.cs:7`
- **`UserNotificationRepository`** (Class) — `Services/NotificationService/NotificationService_Infrastructure/Repositories/UserNotificationRepository.cs:7`
- **`NotificationRepository`** (Class) — `Services/NotificationService/NotificationService_Infrastructure/Repositories/NotificationRepository.cs:7`

## Key Symbols

| Symbol | Type | File | Line |
|--------|------|------|------|
| `PromptTemplate` | Class | `Services/AIService/AIService_Domain/Entities/PromptTemplate.cs` | 4 |
| `EventRecipient` | Class | `Services/NotificationService/NotificationService_Domain/Entities/EventRecipient.cs` | 2 |
| `SessionSnapshot` | Class | `Services/BookingService/BookingService_Domain/Entities/SessionSnapshot.cs` | 7 |
| `UserNotificationRepository` | Class | `Services/NotificationService/NotificationService_Infrastructure/Repositories/UserNotificationRepository.cs` | 7 |
| `NotificationRepository` | Class | `Services/NotificationService/NotificationService_Infrastructure/Repositories/NotificationRepository.cs` | 7 |
| `EventRecipientRepository` | Class | `Services/NotificationService/NotificationService_Infrastructure/Repositories/EventRecipientRepository.cs` | 8 |
| `EmailLogRepository` | Class | `Services/NotificationService/NotificationService_Infrastructure/Repositories/EmailLogRepository.cs` | 7 |
| `EmailCampaignRepository` | Class | `Services/NotificationService/NotificationService_Infrastructure/Repositories/EmailCampaignRepository.cs` | 7 |
| `UserRepository` | Class | `Services/IdentityService/IdentityService_Infrastructure/Repositories/UserRepository.cs` | 7 |
| `UserPlanRepository` | Class | `Services/IdentityService/IdentityService_Infrastructure/Repositories/UserPlanRepository.cs` | 7 |
| `SubscriptionPlanRepository` | Class | `Services/IdentityService/IdentityService_Infrastructure/Repositories/SubscriptionPlanRepository.cs` | 7 |
| `RefreshTokenRepository` | Class | `Services/IdentityService/IdentityService_Infrastructure/Repositories/RefreshTokenRepository.cs` | 7 |
| `OrganizerBankInfoRepository` | Class | `Services/IdentityService/IdentityService_Infrastructure/Repositories/OrganizerBankInfoRepository.cs` | 7 |
| `VenueRepository` | Class | `Services/EventService/EventService_Infrastructure/Repositories/VenueRepository.cs` | 7 |
| `TrackRepository` | Class | `Services/EventService/EventService_Infrastructure/Repositories/TrackRepository.cs` | 7 |
| `TicketTypeRepository` | Class | `Services/EventService/EventService_Infrastructure/Repositories/TicketTypeRepository.cs` | 7 |
| `TalentRepository` | Class | `Services/EventService/EventService_Infrastructure/Repositories/TalentRepository.cs` | 8 |
| `SponsorTierRepository` | Class | `Services/EventService/EventService_Infrastructure/Repositories/SponsorTierRepository.cs` | 7 |
| `SponsorRepository` | Class | `Services/EventService/EventService_Infrastructure/Repositories/SponsorRepository.cs` | 7 |
| `SponsorInteractionRepository` | Class | `Services/EventService/EventService_Infrastructure/Repositories/SponsorInteractionRepository.cs` | 7 |

## Execution Flows

| Flow | Type | Steps |
|------|------|-------|
| `GetAll → GetByCategoryAsync` | intra_community | 3 |
| `GetAll → GetByCategoryAsync` | intra_community | 3 |
| `GetAll → Success` | cross_community | 3 |
| `GetAll → GetAllAsync` | intra_community | 3 |
| `GetAll → GetAllAsync` | intra_community | 3 |
| `GetSessions → GetSessionsByEventIdAsync` | cross_community | 3 |
| `Create → AddAsync` | intra_community | 3 |
| `Create → AddAsync` | intra_community | 3 |
| `Create → SaveChangesAsync` | cross_community | 3 |
| `Create → SaveChangesAsync` | intra_community | 3 |

## Connected Areas

| Area | Connections |
|------|-------------|
| Interfaces | 20 calls |
| Services | 12 calls |

## How to Explore

1. `gitnexus_context({name: "PromptTemplate"})` — see callers and callees
2. `gitnexus_query({query: "repositories"})` — find related execution flows
3. Read key files listed above for implementation details
