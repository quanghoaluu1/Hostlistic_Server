---
name: consumers
description: "Skill for the Consumers area of Hostlistic_Server. 8 symbols across 6 files."
---

# Consumers

8 symbols | 6 files | Cohesion: 82%

## When to Use

- Working with code in `Services/`
- Understanding how User, SendTeamMemberInviteEmailAsync, Consume work
- Modifying consumers-related functionality

## Key Files

| File | Symbols |
|------|---------|
| `Services/NotificationService/NotificationService_Application/Consumers/TeamMemberInvitedConsumer.cs` | Consume, SendInviteEmailAsync, PushRealTimeNotificationAsync |
| `Services/NotificationService/NotificationService_Application/Services/EmailService.cs` | SendTeamMemberInviteEmailAsync |
| `Services/NotificationService/NotificationService_Application/Interfaces/INotificationPushService.cs` | PushToUserAsync |
| `Services/NotificationService/NotificationService_Application/Interfaces/IEmailService.cs` | SendTeamMemberInviteEmailAsync |
| `Services/NotificationService/NotificationService_Api/Services/SignalRNotificationPushService.cs` | PushToUserAsync |
| `Services/IdentityService/IdentityService_Domain/Entities/User.cs` | User |

## Entry Points

Start here when exploring this area:

- **`User`** (Class) — `Services/IdentityService/IdentityService_Domain/Entities/User.cs:6`
- **`SendTeamMemberInviteEmailAsync`** (Method) — `Services/NotificationService/NotificationService_Application/Services/EmailService.cs:115`
- **`Consume`** (Method) — `Services/NotificationService/NotificationService_Application/Consumers/TeamMemberInvitedConsumer.cs:18`
- **`PushToUserAsync`** (Method) — `Services/NotificationService/NotificationService_Api/Services/SignalRNotificationPushService.cs:8`

## Key Symbols

| Symbol | Type | File | Line |
|--------|------|------|------|
| `User` | Class | `Services/IdentityService/IdentityService_Domain/Entities/User.cs` | 6 |
| `SendTeamMemberInviteEmailAsync` | Method | `Services/NotificationService/NotificationService_Application/Services/EmailService.cs` | 115 |
| `Consume` | Method | `Services/NotificationService/NotificationService_Application/Consumers/TeamMemberInvitedConsumer.cs` | 18 |
| `PushToUserAsync` | Method | `Services/NotificationService/NotificationService_Api/Services/SignalRNotificationPushService.cs` | 8 |
| `PushToUserAsync` | Method | `Services/NotificationService/NotificationService_Application/Interfaces/INotificationPushService.cs` | 4 |
| `SendTeamMemberInviteEmailAsync` | Method | `Services/NotificationService/NotificationService_Application/Interfaces/IEmailService.cs` | 8 |
| `SendInviteEmailAsync` | Method | `Services/NotificationService/NotificationService_Application/Consumers/TeamMemberInvitedConsumer.cs` | 36 |
| `PushRealTimeNotificationAsync` | Method | `Services/NotificationService/NotificationService_Application/Consumers/TeamMemberInvitedConsumer.cs` | 89 |

## Connected Areas

| Area | Connections |
|------|-------------|
| Interfaces | 1 calls |

## How to Explore

1. `gitnexus_context({name: "User"})` — see callers and callees
2. `gitnexus_query({query: "consumers"})` — find related execution flows
3. Read key files listed above for implementation details
