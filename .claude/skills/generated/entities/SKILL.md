---
name: entities
description: "Skill for the Entities area of Hostlistic_Server. 7 symbols across 7 files."
---

# Entities

7 symbols | 7 files | Cohesion: 55%

## When to Use

- Working with code in `Services/`
- Understanding how BaseClass, StreamRecording, EmailCampaign work
- Modifying entities-related functionality

## Key Files

| File | Symbols |
|------|---------|
| `Common/BaseClass.cs` | BaseClass |
| `Services/StreamingService/StreamingService_Domain/Entities/StreamRecording.cs` | StreamRecording |
| `Services/NotificationService/NotificationService_Domain/Entities/EmailCampaign.cs` | EmailCampaign |
| `Services/IdentityService/IdentityService_Domain/Entities/SubscriptionPlan.cs` | SubscriptionPlan |
| `Services/EventService/EventService_Domain/Entities/QaQuestion.cs` | QaQuestion |
| `Services/EventService/EventService_Domain/Entities/Feedback.cs` | Feedback |
| `Services/EventService/EventService_Domain/Entities/Event.cs` | Event |

## Entry Points

Start here when exploring this area:

- **`BaseClass`** (Class) — `Common/BaseClass.cs:2`
- **`StreamRecording`** (Class) — `Services/StreamingService/StreamingService_Domain/Entities/StreamRecording.cs:6`
- **`EmailCampaign`** (Class) — `Services/NotificationService/NotificationService_Domain/Entities/EmailCampaign.cs:5`
- **`SubscriptionPlan`** (Class) — `Services/IdentityService/IdentityService_Domain/Entities/SubscriptionPlan.cs:4`
- **`QaQuestion`** (Class) — `Services/EventService/EventService_Domain/Entities/QaQuestion.cs:6`

## Key Symbols

| Symbol | Type | File | Line |
|--------|------|------|------|
| `BaseClass` | Class | `Common/BaseClass.cs` | 2 |
| `StreamRecording` | Class | `Services/StreamingService/StreamingService_Domain/Entities/StreamRecording.cs` | 6 |
| `EmailCampaign` | Class | `Services/NotificationService/NotificationService_Domain/Entities/EmailCampaign.cs` | 5 |
| `SubscriptionPlan` | Class | `Services/IdentityService/IdentityService_Domain/Entities/SubscriptionPlan.cs` | 4 |
| `QaQuestion` | Class | `Services/EventService/EventService_Domain/Entities/QaQuestion.cs` | 6 |
| `Feedback` | Class | `Services/EventService/EventService_Domain/Entities/Feedback.cs` | 5 |
| `Event` | Class | `Services/EventService/EventService_Domain/Entities/Event.cs` | 7 |

## How to Explore

1. `gitnexus_context({name: "BaseClass"})` — see callers and callees
2. `gitnexus_query({query: "entities"})` — find related execution flows
3. Read key files listed above for implementation details
